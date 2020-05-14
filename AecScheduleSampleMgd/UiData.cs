using System;
using System.Collections;
using System.Collections.Generic;
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
    // This class stores necessary information of a property.
    public class PropertyClassData
    {
        public string PropertyName { get; set; }
        public List<RXClass> ObjectTypes = new List<RXClass>();
        public string DisplayName { get; set; }
        public string FullDisplayText
        {
            get
            {
                StringCollection propertyPairs = new StringCollection();
                foreach (RXClass objType in ObjectTypes)
                    propertyPairs.Add(ScheduleSample.GetDisplayName(objType) + ":" + PropertyName);
                string[] arrayData = new string[propertyPairs.Count];
                propertyPairs.CopyTo(arrayData, 0);
                string propertyPairText = string.Join(", ", arrayData);
                return DisplayName + " (" + propertyPairText + ")";
            }
        }
    }

    public class ColumnHeaderNode
    {
        // A node could be either a header or a column. The node is a:
        // header (which contains headers and columns/properties) - when NodeData is string type.
        // column (which is a property) - when NodeData is ClassPropertyData type.
        public object NodeData;
        public List<ColumnHeaderNode> Children = new List<ColumnHeaderNode>();
        public bool IsHeader { get { return NodeData == null ? false : (NodeData is string); } }
        public bool IsColumn { get { return NodeData == null ? false : !IsHeader; } }
        public string HeaderText { get { return NodeData as string; } }
        public PropertyClassData ColumnData { get { return NodeData as PropertyClassData; } }
        // Check whether a header contains nothing. Empty header is not allowed.
        public bool HasEmptyHeader()
        {
            if (IsColumn)
                return false;
            if (Children.Count == 0)
                return true;

            foreach (ColumnHeaderNode child in Children)
            {
                if (child.IsColumn)
                    continue;
                if (child.HasEmptyHeader())
                    return true;
            }

            return false;
        }
    }

    // This is the class that UI forms save data.
    // ScheduleTableCreateEx class uses this data to create schedule table and related objects.
    public class UiData
    {
        // Object ids are grouped by object type.
        public Dictionary<RXClass, List<ObjectId>> classObjectIdsMap;
        // Objects that cannot be scheduled are also saved here.
        public Dictionary<RXClass, List<ObjectId>> ineligibleClassObjectIdsMap;
        // The name of the property set definition.
        public string propertySetDefinitionName;
        // The automatic properties user selected to schedule are stored here and grouped by class.
        public Dictionary<RXClass, StringCollection> classPropertiesMap;
        // The name of the schedule table style.
        public string scheduleTableStyleName;
        // The structure of schedule table header is stored here. It's actually a tree.
        public List<ColumnHeaderNode> headerColumnDesignData;
    }

    public interface IWizardSheet
    {
        UiData Data { get; set; }
        void OnEnter();
        bool OnLeave();
        IWizardManager Manager { get; set; }
    }

    public interface IWizardManager
    {
        void Goto(int pageIndex);
        void GoBack();
        void GoNext();
        void Cancel();
        int PageCount { get; }
        int CurrentPageIndex { get; }
    }
}