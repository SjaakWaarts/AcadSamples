using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Specialized;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.Aec.Geometry;
using Autodesk.Aec.DatabaseServices;
using Autodesk.Aec.Arch.DatabaseServices;

using Autodesk.Aec.PropertyData;
using Autodesk.Aec.PropertyData.DatabaseServices;

using DBTransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
using Dictionary = Autodesk.Aec.DatabaseServices.Dictionary;

using ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;
using ObjectIdCollection = Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection;

using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using AecEntity = Autodesk.Aec.DatabaseServices.Entity;
using AecDbObject = Autodesk.Aec.DatabaseServices.DBObject;
using AMEntity = Autodesk.Aec.Modeler.Entity;
using System.Runtime.InteropServices;


namespace AecScheduleSampleMgd
{
    public partial class WizardSheetScheduleTableStyle : UserControl, IWizardSheet
    {
        public WizardSheetScheduleTableStyle()
        {
            InitializeComponent();
        }

        #region IWizardSheet Members

        UiData runtimeData;
        public UiData Data
        {
            get
            {
                return runtimeData;
            }
            set
            {
                runtimeData = value;
            }
        }

        public void OnEnter()
        {
            FillPropertyTree();
            if (runtimeData.classPropertiesMap != null)
            {
                GenerateFullColumnHeader(runtimeData.classPropertiesMap);
            }
        }

        public bool OnLeave()
        {
            if (!SaveStyleNameToUiData())
                return false;

            if (!SaveColumnHeaderToUiData())
                return false;

            return true;
        }

        IWizardManager manager = null;
        public IWizardManager Manager
        {
            get
            {
                return manager;
            }
            set
            {
                manager = value;
            }
        }

        #endregion

        // Remove selected node from the column header tree.
        // The selected node could be either a header or a column.
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (treeColumnHeader.SelectedNode != null && treeColumnHeader.SelectedNode.Tag != null)
            {
                PropertyClassData data = treeColumnHeader.SelectedNode.Tag as PropertyClassData;
            }
            treeColumnHeader.Nodes.Remove(treeColumnHeader.SelectedNode);
        }

        // Add a header to the column header tree
        private void btnAddHeader_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeColumnHeader.SelectedNode;
            TreeNode newNode = null;
            if (selectedNode != null && selectedNode.Tag != null)
            {
                MessageBox.Show("Header could not be added as a child of property.");
                return;
            }
            if (selectedNode == null)
            {
                newNode = treeColumnHeader.Nodes.Add("Header");
            }
            else
            {
                newNode = selectedNode.Nodes.Add("Header");
            }
            newNode.EnsureVisible();
            newNode.BeginEdit();
        }

        // Add properties to the column header tree
        private void btnAddColumn_Click(object sender, EventArgs e)
        {
            TreeNode selectedPropertyNode = treeProperties.SelectedNode;
            if (selectedPropertyNode == null || selectedPropertyNode.Tag == null)
            {
                MessageBox.Show("Please selected a property first.");
                return;
            }

            PropertyClassData data = selectedPropertyNode.Tag as PropertyClassData;
            if (IsPropertyAddedToColumnHeaderTree(treeColumnHeader.Nodes, data.PropertyName))
            {
                MessageBox.Show("A property can only be added to the schedule table style once.");
                return;
            }

            TreeNode selectedHeaderNode = treeColumnHeader.SelectedNode;
            TreeNode treeNodeToExpand = null;
            if (selectedHeaderNode == null) // No selection - add properties as root nodes
                AddPropertyToNodeCollection(treeColumnHeader.Nodes, data.ObjectTypes, data.PropertyName);
            else if (selectedHeaderNode.Tag == null) // Current selection is a header - add properties to a header
            {
                AddPropertyToNodeCollection(selectedHeaderNode.Nodes, data.ObjectTypes, data.PropertyName);
                treeNodeToExpand = selectedHeaderNode;
            }
            else // Current selection is a column(property) - add properties as siblings of the current selected property
            {
                // check if it's root node already
                if (selectedHeaderNode.Parent == null) // add as root nodes
                    AddPropertyToNodeCollection(treeColumnHeader.Nodes, data.ObjectTypes, data.PropertyName);
                else // add to parent header
                {
                    AddPropertyToNodeCollection(selectedHeaderNode.Parent.Nodes, data.ObjectTypes, data.PropertyName);
                    treeNodeToExpand = selectedHeaderNode.Parent;
                }
            }
            if (treeNodeToExpand != null)
                treeNodeToExpand.Expand();
        }

        // enables the user to clean the selection by clicking on the empty area of the tree view.
        private void treeColumnHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (treeColumnHeader.HitTest(e.Location).Location == TreeViewHitTestLocations.None)
                treeColumnHeader.SelectedNode = null;
        }

        #region Tree View Tricks
        // Little tricks in .NET to make the display text of a node differ from its actual text.
        // In our case, when user edits the node by clicking the node label, they're actually
        // editing the Column name of the property.
        private void treeColumnHeader_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                PropertyClassData data = e.Node.Tag as PropertyClassData;
                IntPtr hEditBox = SendMessage(treeColumnHeader.Handle, TVM_GETEDITCONTROL, IntPtr.Zero, IntPtr.Zero);
                SetWindowText(hEditBox, data.DisplayName);
            }
        }

        private void treeColumnHeader_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null)
                return;

            if (e.Label.Length == 0)
            {
                MessageBox.Show("Please specify a valid text.");
                e.CancelEdit = true;
            }

            if (e.Node.Tag != null)
            {
                e.CancelEdit = true;
                PropertyClassData data = e.Node.Tag as PropertyClassData;
                data.DisplayName = e.Label;
                e.Node.Text = data.FullDisplayText;
            }
        }
        #endregion

        #region Helpers
        bool SaveStyleNameToUiData()
        {
            if (textStyleName.Text.Length == 0)
            {
                MessageBox.Show("Please specify a name for the schedule table style.");
                return false;
            }

            Database db = ScheduleSample.GetDatabase();
            DictionaryScheduleTableStyle dict = new DictionaryScheduleTableStyle(db);
            DBTransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                if (dict.Has(textStyleName.Text, trans))
                {
                    MessageBox.Show("The style name you specified already exists. Please specify a new one.");
                    return false;
                }
            }

            runtimeData.scheduleTableStyleName = textStyleName.Text;

            return true;
        }

        bool SaveColumnHeaderToUiData()
        {
            runtimeData.headerColumnDesignData = new List<ColumnHeaderNode>();

            foreach (TreeNode node in treeColumnHeader.Nodes)
                SaveTreeNode(node, runtimeData.headerColumnDesignData);

            foreach (ColumnHeaderNode node in runtimeData.headerColumnDesignData)
                if (node.HasEmptyHeader())
                {
                    MessageBox.Show("Each header should have at least one column. Please check your design and avoid empty headers before moving to the next step.");
                    return false;
                }

            return true;
        }

        void SaveTreeNode(TreeNode node, List<ColumnHeaderNode> array)
        {
            ColumnHeaderNode hcn = new ColumnHeaderNode();
            if (node.Tag == null)
            {
                hcn.NodeData = node.Text;
                foreach (TreeNode subNode in node.Nodes)
                    SaveTreeNode(subNode, hcn.Children);
            }
            else
            {
                hcn.NodeData = node.Tag;
            }
            array.Add(hcn);
        }

        bool IsPropertyAddedToColumnHeaderTree(TreeNodeCollection nodes, string propertyName)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag != null)
                {
                    PropertyClassData data = node.Tag as PropertyClassData;
                    if (data.PropertyName == propertyName)
                        return true;
                }
                else if (IsPropertyAddedToColumnHeaderTree(node.Nodes, propertyName))
                    return true;
            }
            return false;
        }

        // generate column header tree from scratch
        void GenerateFullColumnHeader(Dictionary<RXClass, StringCollection> classPropertiesMap)
        {
            treeColumnHeader.Nodes.Clear();
            // First, we generate a "General" header which contains common properties that are scheduled across different classes.
            Dictionary<string, List<RXClass>> propertyClassesMap = classPropertiesMap.GroupClassesByProperty();
            if (propertyClassesMap.ContainsGeneralProperty())
            {
                propertyClassesMap.FilterOutSingleProperties();
                TreeNode headerNode = treeColumnHeader.Nodes.Add("General");
                GenerateColumnHeaderByProperty(propertyClassesMap, headerNode.Nodes);
            }

            Dictionary<RXClass, StringCollection> newClassPropertiesMap = classPropertiesMap.DeepClone();
            propertyClassesMap.FilterOutGeneralProperties(newClassPropertiesMap);

            foreach (RXClass objectType in newClassPropertiesMap.Keys)
            {
                if (newClassPropertiesMap[objectType].Count > 0)
                {
                    TreeNode headerNode = treeColumnHeader.Nodes.Add(ScheduleSample.GetDisplayName(objectType));
                    GenerateColumnHeaderByObjectType(objectType, newClassPropertiesMap[objectType], headerNode.Nodes);
                }
            }
            treeColumnHeader.ExpandAll();
        }

        void GenerateColumnHeaderByProperty(Dictionary<string, List<RXClass>> propertyClassesMap, TreeNodeCollection nodes)
        {
            // First we categorize the properties.
            Dictionary<string, StringCollection> categorizedProperties = new Dictionary<string, StringCollection>();
            foreach (string propName in propertyClassesMap.Keys)
            {
                string catName = GetCategoryName(propName);
                if (!categorizedProperties.ContainsKey(catName))
                    categorizedProperties[catName] = new StringCollection();
                categorizedProperties[catName].Add(propName);
            }
            // sort category names alphabetically
            List<string> sortedCatNames = new List<string>();
            foreach (string catName in categorizedProperties.Keys)
                sortedCatNames.Add(catName);
            sortedCatNames.Sort();
            // create category headers
            foreach (string catName in sortedCatNames)
            {
                TreeNode headerNode = nodes.Add(catName);
                // create columns
                List<string> sortedPropNames = new List<string>();
                string[] propNames = new string[categorizedProperties[catName].Count];
                categorizedProperties[catName].CopyTo(propNames, 0);
                sortedPropNames.AddRange(propNames);
                sortedPropNames.Sort();
                foreach (string propName in sortedPropNames)
                    AddPropertyToNodeCollection(headerNode.Nodes, propertyClassesMap[propName], propName);
            }
        }

        void GenerateColumnHeaderByObjectType(RXClass objectType, StringCollection propertyNames, TreeNodeCollection nodes)
        {
            // First we categorize the properties.
            Dictionary<string, StringCollection> categorizedProperties = new Dictionary<string, StringCollection>();
            foreach (string propertyName in propertyNames)
            {
                string catName = GetCategoryName(propertyName);
                if (!categorizedProperties.ContainsKey(catName))
                    categorizedProperties[catName] = new StringCollection();
                categorizedProperties[catName].Add(propertyName);
            }
            // sort category names alphabetically
            List<string> sortedCatNames = new List<string>();
            foreach (string catName in categorizedProperties.Keys)
                sortedCatNames.Add(catName);
            sortedCatNames.Sort();
            // create category headers
            foreach (string catName in sortedCatNames)
            {
                TreeNode headerNode = nodes.Add(catName);
                // create columns
                List<string> sortedPropNames = new List<string>();
                string[] propNames = new string[categorizedProperties[catName].Count];
                categorizedProperties[catName].CopyTo(propNames, 0);
                sortedPropNames.AddRange(propNames);
                sortedPropNames.Sort();
                foreach (string propName in sortedPropNames)
                    AddPropertyToNodeCollection(headerNode.Nodes, objectType, propName);
            }
        }

        string GetCategoryName(string propertyName)
        {
            string loweredName = propertyName.ToLower();
            if (loweredName.Contains("width") || loweredName.Contains("height") || loweredName.Contains("length")
                || loweredName.Contains("depth") || loweredName.Contains("volume") || loweredName.Contains("area")
                || loweredName.Contains("size"))
                return "Dimension";
            if (loweredName.Contains("color") || loweredName.Contains("layer") || loweredName.Contains("linetype")
                || loweredName.Contains("lineweight") || loweredName.Contains("style"))
                return "Style";
            if (loweredName.Contains("description") || loweredName.Contains("document") || loweredName.Contains("hyperlink")
                || loweredName.Contains("note"))
                return "Document";

            return "Miscellaneous";
        }

        void AddPropertyToNodeCollection(TreeNodeCollection nodes, List<RXClass> objectTypes, string propertyName)
        {
            PropertyClassData data = new PropertyClassData();
            data.ObjectTypes.AddRange(objectTypes);
            data.PropertyName = propertyName;
            data.DisplayName = propertyName;
            TreeNode newNode = nodes.Add(data.FullDisplayText);
            newNode.Tag = data;
            newNode.EnsureVisible();
        }

        void AddPropertyToNodeCollection(TreeNodeCollection nodes, RXClass objectType, string propertyName)
        {
            List<RXClass> objectTypes = new List<RXClass>();
            objectTypes.Add(objectType);
            AddPropertyToNodeCollection(nodes, objectTypes, propertyName);
        }

        // Fill the property tree with the property definition sets defined in the previous wizard sheet.
        void FillPropertyTree()
        {
            treeProperties.Nodes.Clear();
            // Add a "General" node which contains common properties that are scheduled across different classes.
            Dictionary<string, List<RXClass>> propertyClassesMap = runtimeData.classPropertiesMap.GroupClassesByProperty();
            if (propertyClassesMap.ContainsGeneralProperty())
            {
                TreeNode headerNode = treeProperties.Nodes.Add("General");
                StringCollection generalProperties = propertyClassesMap.GetGeneralPropertyNames();
                foreach (string propName in generalProperties)
                    AddPropertyToNodeCollection(headerNode.Nodes, propertyClassesMap[propName], propName);
            }
            // Now we add properties grouped by class
            Dictionary<RXClass, StringCollection> newClassPropertiesMap = runtimeData.classPropertiesMap.DeepClone();
            propertyClassesMap.FilterOutGeneralProperties(newClassPropertiesMap); // we only need the properties that are not shared across classes at this stage

            foreach (RXClass objectType in newClassPropertiesMap.Keys)
            {
                if (newClassPropertiesMap[objectType].Count == 0)
                    continue;

                TreeNode newNode = treeProperties.Nodes.Add(ScheduleSample.GetDisplayName(objectType));
                StringCollection propertyNames = newClassPropertiesMap[objectType];
                foreach (string propertyName in propertyNames)
                    AddPropertyToNodeCollection(newNode.Nodes, objectType, propertyName);
            }
            treeProperties.ExpandAll();
        }

        #endregion

        #region Win32
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);
        const int TV_FIRST = 0x1100;
        const int TVM_GETEDITCONTROL = TV_FIRST + 15;
        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string lpString);
        #endregion
    }

    public static class PropertyClassesMapExtensionMethods
    {
        public static bool ContainsGeneralProperty(this Dictionary<string, List<RXClass>> propertyClassesMap)
        {
            foreach (string propertyName in propertyClassesMap.Keys)
                if (propertyClassesMap[propertyName].Count > 1)
                    return true;
            return false;
        }

        public static StringCollection GetGeneralPropertyNames(this Dictionary<string, List<RXClass>> propertyClassesMap)
        {
            StringCollection propNames = new StringCollection();
            foreach (string propName in propertyClassesMap.Keys)
                if (propertyClassesMap[propName].Count > 1)
                    propNames.Add(propName);
            return propNames;
        }

        public static void FilterOutSingleProperties(this Dictionary<string, List<RXClass>> propertyClassesMap)
        {
            StringCollection generalPropertyNames = propertyClassesMap.GetGeneralPropertyNames();
            StringCollection singlePropertyNames = new StringCollection();
            foreach (string prop in propertyClassesMap.Keys)
                if (!generalPropertyNames.Contains(prop))
                    singlePropertyNames.Add(prop);
            foreach (string prop in singlePropertyNames)
                propertyClassesMap.Remove(prop);
        }

        public static void FilterOutGeneralProperties(this Dictionary<string, List<RXClass>> propertyClassesMap, Dictionary<RXClass, StringCollection> classPropertiesMap)
        {
            StringCollection generalPropertyNames = propertyClassesMap.GetGeneralPropertyNames();
            foreach (RXClass objectType in classPropertiesMap.Keys)
                for (int i = classPropertiesMap[objectType].Count - 1; i >= 0; --i)
                {
                    string prop = classPropertiesMap[objectType][i];
                    if (generalPropertyNames.Contains(prop))
                        classPropertiesMap[objectType].RemoveAt(i);
                }
        }
    }

    public static class ClassPropertiesMapExtensionMethods
    {
        public static Dictionary<RXClass, StringCollection> DeepClone(this Dictionary<RXClass, StringCollection> classPropertiesMap)
        {
            Dictionary<RXClass, StringCollection> newMap = new Dictionary<RXClass, StringCollection>();
            foreach (RXClass objectType in classPropertiesMap.Keys)
            {
                StringCollection propsCopy = new StringCollection();
                string[] rawProps = new string[classPropertiesMap[objectType].Count];
                classPropertiesMap[objectType].CopyTo(rawProps, 0);
                propsCopy.AddRange(rawProps);
                newMap[objectType] = propsCopy;
            }
            return newMap;
        }

        public static Dictionary<string, List<RXClass>> GroupClassesByProperty(this Dictionary<RXClass, StringCollection> classPropertiesMap)
        {
            Dictionary<string, List<RXClass>> propertyClassesMap = new Dictionary<string, List<RXClass>>();
            foreach (RXClass objectType in classPropertiesMap.Keys)
            {
                foreach (string propertyName in classPropertiesMap[objectType])
                {
                    if (!propertyClassesMap.ContainsKey(propertyName))
                        propertyClassesMap[propertyName] = new List<RXClass>();
                    propertyClassesMap[propertyName].Add(objectType);
                }
            }
            return propertyClassesMap;
        }
    }
}
