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
    public partial class WizardSheetSummary : UserControl, IWizardSheet
    {
        public WizardSheetSummary()
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
            }
        }

        public void OnEnter()
        {
            textSummary.Text = GenerateReportOfSelectedObjects()
                + GenerateReportOfIneligibleObjects()
                + GenerateLine()
                + GenerateReportOfPropertySetDefinition()
                + GenerateLine()
                + GenerateReportOfScheduleTableStyle();
        }

        public bool OnLeave()
        {
            return true;
        }

        IWizardManager manager;
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

        string GenerateLine()
        {
            Graphics gs = textSummary.CreateGraphics();
            SizeF charSize = gs.MeasureString("-", textSummary.Font);
            SizeF doubleCharSize = gs.MeasureString("--", textSummary.Font);
            float space = doubleCharSize.Width - charSize.Width * 2;
            
            StringBuilder sb = new StringBuilder();
            sb.Append('-', Convert.ToInt32(textSummary.ClientRectangle.Width / (charSize.Width + space)));
            sb.AppendLine();
            return sb.ToString();
        }

        string GenerateReportOfSelectedObjects()
        {
            StringBuilder sb = new StringBuilder();
            int objectCount = 0;
            foreach(List<ObjectId> objectIdList in runtimeData.classObjectIdsMap.Values)
                objectCount += objectIdList.Count;
            sb.AppendFormat("You are about to schedule {0} objects:", objectCount);
            sb.AppendLine();
            foreach (RXClass objectType in runtimeData.classObjectIdsMap.Keys)
                sb.AppendFormat("{0}[{1}] ", ScheduleSample.GetDisplayName(objectType), runtimeData.classObjectIdsMap[objectType].Count);
            sb.AppendLine();
            return sb.ToString();
        }

        string GenerateReportOfIneligibleObjects()
        {
            if (runtimeData.ineligibleClassObjectIdsMap.Keys.Count == 0)
                return null;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Some objects you picked cannot be scheduled:");
            foreach (RXClass objectType in runtimeData.ineligibleClassObjectIdsMap.Keys)
            {
                sb.AppendFormat("{0} object(s) of {1}.", runtimeData.ineligibleClassObjectIdsMap[objectType].Count, ScheduleSample.GetDisplayName(objectType));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        string GenerateReportOfPropertySetDefinition()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("A new property set definition will be created with the name [{0}]", runtimeData.propertySetDefinitionName);
            sb.AppendLine();
            sb.AppendFormat("{0} types of objects are included.", runtimeData.classPropertiesMap.Keys.Count);
            sb.AppendLine();
            foreach (RXClass objectType in runtimeData.classPropertiesMap.Keys)
                GenerateReportOfPropertySet(sb, objectType, runtimeData.classPropertiesMap[objectType]);
            return sb.ToString();
        }

        void GenerateReportOfPropertySet(StringBuilder sb, RXClass objectType, StringCollection propertyNames)
        {
            if (propertyNames.Count == 0)
                return;

            sb.AppendFormat("{0} - {1} properties:", ScheduleSample.GetDisplayName(objectType), propertyNames.Count);
            sb.AppendLine();
            foreach (string name in propertyNames)
                sb.AppendLine(name);
            sb.AppendLine();
        }

        string GenerateReportOfScheduleTableStyle()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("A new schedule table style will be created with the name [{0}]", runtimeData.scheduleTableStyleName);
            sb.AppendLine();
            sb.AppendLine("The structure of  the schedule table column header will be:");

            foreach (ColumnHeaderNode node in runtimeData.headerColumnDesignData)
            {
                GenerateStringForNode(sb, node, 0);
            }
            
            return sb.ToString();
        }

        void GenerateStringForNode(StringBuilder sb, ColumnHeaderNode node, int indent)
        {
            for (int i = 0; i <= indent; ++i)
                sb.Append("    ");
            if (node.IsHeader)
            {
                sb.AppendLine(node.NodeData as string);
                foreach (ColumnHeaderNode childNode in node.Children)
                    GenerateStringForNode(sb, childNode, indent + 1);
            }
            else if (node.IsColumn)
                sb.AppendLine(node.ColumnData.FullDisplayText);
            else
                sb.AppendLine("*Error*");
        }
    }
}
