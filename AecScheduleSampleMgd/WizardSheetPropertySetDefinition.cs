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

namespace AecScheduleSampleMgd
{
    public partial class WizardSheetPropertySetDefinition : UserControl, IWizardSheet
    {
        public WizardSheetPropertySetDefinition()
        {
            InitializeComponent();
        }

        #region IWizardSheet Members

        UiData runtimeData = null;
        public UiData Data
        {
            get
            {
                return runtimeData;
            }
            set
            {
                runtimeData = value;
                if (runtimeData.classPropertiesMap == null)
                    runtimeData.classPropertiesMap = new Dictionary<RXClass, StringCollection>();
            }
        }

        public void OnEnter()
        {
            if (runtimeData == null)
                throw new ArgumentNullException("Ui data should be attached before calling this.");

            RefreshListItems();
        }

        public bool OnLeave()
        {
            if (!SavePropertySetDefinitionNameToUiData())
                return false;
            if (!CheckPropertySetDefinition())
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

        private void listObjectType_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillSourceNameList();
        }

        private void listObjectType_Format(object sender, ListControlConvertEventArgs e)
        {
            KeyValuePair<RXClass, List<ObjectId>> pair = (KeyValuePair<RXClass, List<ObjectId>>)e.ListItem;
            RXClass objectType = pair.Key;
            e.Value = ScheduleSample.GetDisplayName(objectType) + "(" + pair.Value.Count.ToString() + ")";
        }

        private void listProperties_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // return if nothing's selected
            if (listObjectType.SelectedIndex == -1)
                return;

            KeyValuePair<RXClass, List<ObjectId>> pair = (KeyValuePair<RXClass, List<ObjectId>>)listObjectType.Items[listObjectType.SelectedIndex];
            if (!runtimeData.classPropertiesMap.ContainsKey(pair.Key))
                runtimeData.classPropertiesMap[pair.Key] = new StringCollection();

            object item = listProperties.Items[e.Index];
            if (e.NewValue == CheckState.Checked)
            {
                if (!runtimeData.classPropertiesMap[pair.Key].Contains(item.ToString()))
                    runtimeData.classPropertiesMap[pair.Key].Add(item.ToString());
            }
            else if(e.CurrentValue == CheckState.Checked && e.NewValue == CheckState.Unchecked)
            {
                runtimeData.classPropertiesMap[pair.Key].Remove(item.ToString());
            }
        }

        #region Helpers
        bool SavePropertySetDefinitionNameToUiData()
        {
            if (textPsdName.Text.Length == 0)
            {
                MessageBox.Show("Please specify a name for the property set definition.");
                return false;
            }
            Database db = ScheduleSample.GetDatabase();
            DictionaryPropertySetDefinitions dict = new DictionaryPropertySetDefinitions(db);
            DBTransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                if (dict.Has(textPsdName.Text, trans))
                {
                    MessageBox.Show("The property set definition name you specified already exists. Please specify a new one.");
                    return false;
                }
            }
            runtimeData.propertySetDefinitionName = textPsdName.Text;
            return true;
        }

        bool CheckPropertySetDefinition()
        {
            bool empty = true;
            foreach (KeyValuePair<RXClass, StringCollection> pair in runtimeData.classPropertiesMap)
            {
                if (pair.Value.Count > 0)
                {
                    empty = false;
                    break;
                }
            }
            if (empty)
            {
                MessageBox.Show("Please select some properties before going to the next step.");
                return false;
            }
            return true;
        }

        void RefreshListItems()
        {
            listObjectType.Items.Clear();

            foreach (KeyValuePair<RXClass, List<ObjectId>> pair in runtimeData.classObjectIdsMap)
            {
                listObjectType.Items.Add(pair);
                if (runtimeData.classPropertiesMap.ContainsKey(pair.Key))
                {
                    string[] names = new string[runtimeData.classPropertiesMap[pair.Key].Count];
                    runtimeData.classPropertiesMap[pair.Key].CopyTo(names, 0);
                }
            }

            if (listObjectType.Items.Count > 0)
            {
                listObjectType.SelectedIndex = 0;
            }
        }

        void FillSourceNameList()
        {
            // nothing selected
            if (listObjectType.SelectedIndex == -1)
                return;

            listProperties.Items.Clear();

            KeyValuePair<RXClass, List<ObjectId>> pair = (KeyValuePair<RXClass, List<ObjectId>>)listObjectType.Items[listObjectType.SelectedIndex];
            string[] sourceNames = PropertyDataServices.FindAutomaticSourceNames(pair.Key.Name, ScheduleSample.GetDatabase());

            StringCollection selectedNames = null;
            if (runtimeData.classPropertiesMap.ContainsKey(pair.Key))
                selectedNames = runtimeData.classPropertiesMap[pair.Key];

            foreach (string name in sourceNames)
            {
                if (selectedNames == null)
                    listProperties.Items.Add(name);
                else
                    listProperties.Items.Add(name, selectedNames.Contains(name));
            }
        }
        #endregion
    }
}
