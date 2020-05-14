using System;
using System.Collections.Generic;
using System.Text;
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
using AcadDb = Autodesk.AutoCAD.DatabaseServices;
using AecDb = Autodesk.Aec.DatabaseServices;
using AecPropDb = Autodesk.Aec.PropertyData.DatabaseServices;

namespace AecScheduleSampleMgd
{
    public class ScheduleTableCreateResult
    {
        public ObjectId StyleId { get; set; }
        public ObjectId PropertySetDefinitionId { get; set; }
        public ObjectId ScheduleTableId { get; set; }
    }

    #region ScheduleTableCreateEx
    /// <summary>
    /// Creates the property set definitino, style and schedule table in database.
    /// </summary>
    public static class ScheduleTableCreateEx
    {
        #region Create property set definition, style and schedule table
        /// <summary>
        /// Creates the whole property sets, schedule table style and schedule table in database.
        /// </summary>
        /// <param name="uiData">The data saved from the wizard.</param>
        /// <returns>Returns the object id of the property set definition, schedule table style and schedule table.</returns>
        public static ScheduleTableCreateResult CreateScheduleTable(UiData uiData)
        {
            ScheduleTableCreateResult result = new ScheduleTableCreateResult();
            Database db = ScheduleSample.GetDatabase();
            DBTransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                try
                {
                    PropertySetDefinition psd = CreatePropertySetDefinition(uiData, trans);
                    result.PropertySetDefinitionId = psd.Id;
                    if (result.PropertySetDefinitionId == ObjectId.Null)
                        throw (new System.Exception("Failed to create property set definition."));

                    ScheduleTableStyle style = CreateStyle(uiData, psd, trans);
                    result.StyleId = style.Id;
                    if (result.StyleId == ObjectId.Null)
                        throw (new System.Exception("Failed to create property style."));

                    AddPropertySetToObjects(uiData, result.PropertySetDefinitionId, trans);

                    ScheduleTable table = CreateScheduleTable(uiData, result.PropertySetDefinitionId, result.StyleId, trans);
                    result.ScheduleTableId = table.Id;
                    if (result.ScheduleTableId == ObjectId.Null)
                        throw (new System.Exception("Failed to create Schedule Table."));

                    Editor editor = ScheduleSample.GetEditor();
                    PromptPointResult editorResult = editor.GetPoint("Please pick a point to insert the schedule table:");
                    if (editorResult.Status == PromptStatus.OK)
                    {
                        table.Location = editorResult.Value;
                        table.Scale = 10;
                        trans.Commit();
                    }
                    else
                    {
                        trans.Abort();
                    }
                }
                catch (System.Exception)
                {
                    trans.Abort();
                    return null;
                }
                finally
                {
                    trans.Dispose();
                }
            }

            return result;
        }

        private static PropertySetDefinition CreatePropertySetDefinition(UiData uiData, Transaction trans)
        {
            Database db = ScheduleSample.GetDatabase();

            string psdName = uiData.propertySetDefinitionName;
            StringCollection appliesTo = new StringCollection();
            foreach (RXClass rc in uiData.classPropertiesMap.Keys)
                appliesTo.Add(rc.Name);

            // create the property set definition
            PropertySetDefinition psd = new PropertySetDefinition();
            psd.SetToStandard(db);
            psd.SubSetDatabaseDefaults(db);
            psd.AlternateName = psdName;
            psd.IsLocked = false;
            psd.IsVisible = true;
            psd.IsWriteable = true;

            psd.SetAppliesToFilter(appliesTo, false);

            Dictionary<string, List<RXClass>> propertyClassesMap = uiData.classPropertiesMap.GroupClassesByProperty();
            foreach (string propertyName in propertyClassesMap.Keys)
            {
                PropertyDefinition propDef = new PropertyDefinition();
                propDef.SetToStandard(db);
                propDef.SubSetDatabaseDefaults(db);
                propDef.Name = propertyName;
                propDef.Automatic = true;
                propDef.Description = propertyName;
                propDef.IsVisible = true;
                propDef.IsReadOnly = true;
                foreach(RXClass objectType in propertyClassesMap[propertyName])
                    propDef.SetAutomaticData(objectType.Name, propertyName);
                psd.Definitions.Add(propDef);
            }

            DictionaryPropertySetDefinitions propDefs = new DictionaryPropertySetDefinitions(db);
            propDefs.AddNewRecord(uiData.propertySetDefinitionName, psd);
            trans.AddNewlyCreatedDBObject(psd, true);
            return psd;
        }

        private static List<ScheduleTableStyleHeaderNode> CreateChildNodes(List<ColumnHeaderNode> childNodes, ScheduleTableStyleHeaderTree tree, PropertySetDefinition psd, ScheduleTableStyle style)
        {
            Database db = ScheduleSample.GetDatabase();
            List<ScheduleTableStyleHeaderNode> nodesCreated = new List<ScheduleTableStyleHeaderNode>();
            foreach (ColumnHeaderNode childNode in childNodes)
            {
                if (childNode.IsColumn)
                {
                    PropertyClassData data = childNode.ColumnData;
                    ScheduleTableStyleColumn column = CreateColumn(data, psd, style);
                    style.Columns.Add(column);
                    nodesCreated.Add(column);
                }
                else if (childNode.IsHeader)
                {
                    string headerName = childNode.NodeData as string;
                    List<ScheduleTableStyleHeaderNode> myChildren = CreateChildNodes(childNode.Children, tree, psd, style);
                    ScheduleTableStyleHeaderNode header = tree.InsertNode(headerName, tree.Root.Children.Count, tree.Root, myChildren.ToArray());
                    nodesCreated.Add(header);
                }
                else
                {
                    throw new ArgumentException("Cannot resolve node type properly when creating schedule table style.");
                }
            }
            return nodesCreated;
        }

        private static ScheduleTableStyleColumn CreateColumn(PropertyClassData nodeData, PropertySetDefinition psd, ScheduleTableStyle style)
        {
            Database db = ScheduleSample.GetDatabase();
            ScheduleTableStyleColumn column = new ScheduleTableStyleColumn();
            column.SetToStandard(db);
            column.SubSetDatabaseDefaults(db);
            column.PropertySetDefinitionId = psd.Id;
            column.ColumnType = ScheduleTableStyleColumnType.Normal;
            column.Heading = nodeData.DisplayName;
            column.PropertyId = psd.Definitions.IndexOf(nodeData.PropertyName);
            return column;
        }

        // another way to find the index of a property definition
        //private static int FindPropertyDefinitionIndex(PropertySetDefinition psd, string propertyName)
        //{
        //    PropertyDefinition propDef = new PropertyDefinition();
        //    propDef.Name = propertyName;
        //    return psd.Definitions.IndexOf(propDef);
        //}

        private static ScheduleTableStyle CreateStyle(UiData uiData, PropertySetDefinition psd, Transaction trans)
        {
            Database db = ScheduleSample.GetDatabase();
            ScheduleTableStyle style = new ScheduleTableStyle();

            style.SetToStandard(db);
            style.SubSetDatabaseDefaults(db);

            // sets the object type to which the schedule table style applies
            StringCollection filter = new StringCollection();
            foreach (RXClass rc in uiData.classPropertiesMap.Keys)
                filter.Add(rc.Name);
            style.AppliesToFilter = filter;

            CreateChildNodes(uiData.headerColumnDesignData, style.Tree, psd, style);

            //add it into database
            DictionaryScheduleTableStyle scheduleTableStyleDict = new DictionaryScheduleTableStyle(db);
            style.Title = uiData.scheduleTableStyleName;
            scheduleTableStyleDict.AddNewRecord(uiData.scheduleTableStyleName, style);
            trans.AddNewlyCreatedDBObject(style, true);

            return style;
        }

        private static ScheduleTable CreateScheduleTable(UiData uiData, ObjectId propertySetDefinitionId, ObjectId styleId, Transaction trans)
        {
            Database db = ScheduleSample.GetDatabase();
            BlockTableRecord btr = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

            ScheduleTable scheduleTable = new ScheduleTable();
            scheduleTable.SetToStandard(db);
            scheduleTable.SetDatabaseDefaults(db);
            scheduleTable.StyleId = styleId;
            scheduleTable.SetDefaultLayer();

            btr.AppendEntity(scheduleTable);
            trans.AddNewlyCreatedDBObject(scheduleTable, true);

            ObjectIdCollection selectionSet = new ObjectIdCollection();
            foreach (List<ObjectId> idSetByObjectType in uiData.classObjectIdsMap.Values)
                foreach (ObjectId id in idSetByObjectType)
                    selectionSet.Add(id);

            scheduleTable.SetSelectionSet(selectionSet, new ObjectIdCollection());
            scheduleTable.AutomaticUpdate = true;

            return scheduleTable;
        }
        
        private static void AddPropertySetToObjects(UiData uiData, ObjectId propertySetDefinitionId, Transaction trans)
        {
            foreach (List<ObjectId> ids in uiData.classObjectIdsMap.Values)
            {
                foreach (ObjectId id in ids)
                {
                    Autodesk.AutoCAD.DatabaseServices.DBObject obj = trans.GetObject(id, OpenMode.ForWrite) as Autodesk.AutoCAD.DatabaseServices.DBObject;
                    PropertyDataServices.RemovePropertySet(obj, propertySetDefinitionId);
                    PropertyDataServices.AddPropertySet(obj, propertySetDefinitionId);
                }
            }
        }
        #endregion
    }
    #endregion
}