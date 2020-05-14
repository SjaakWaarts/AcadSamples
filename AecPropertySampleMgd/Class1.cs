#region Header
//      .NET Sample
//
//      Copyright (c) 2007 by Autodesk, Inc.
//
//      Permission to use, copy, modify, and distribute this software
//      for any purpose and without fee is hereby granted, provided
//      that the above copyright notice appears in all copies and
//      that both that copyright notice and the limited warranty and
//      restricted rights notice below appear in all supporting
//      documentation.
//
//      AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
//      AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
//      MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
//      DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
//      UNINTERRUPTED OR ERROR FREE.
//
//      Use, duplication, or disclosure by the U.S. Government is subject to
//      restrictions set forth in FAR 52.227-19 (Commercial Computer
//      Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
//      (Rights in Technical Data and Computer Software), as applicable.
#endregion

#region Namespaces

using System;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

using Autodesk.Aec.DatabaseServices;
using Autodesk.Aec.PropertyData.DatabaseServices;

using DBTransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
using AcadDb = Autodesk.AutoCAD.DatabaseServices;
using AecDb = Autodesk.Aec.DatabaseServices;
using AecPropDb = Autodesk.Aec.PropertyData.DatabaseServices;

using ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;
using ObjectIdCollection = Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection;

#endregion

[assembly: ExtensionApplication(typeof(Class1))]
[assembly: CommandClass(typeof(Class1))]

public class Class1 : IExtensionApplication
{
    #region IExtensionApplication
    /// <summary>
    /// Initialization. Shows load message.
    /// </summary>
    public void Initialize()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        ed.WriteMessage("Loading AecPropertySampleMgd...\r\n");
    }

    /// <summary>
    /// Termination.
    /// </summary>
    public void Terminate()
    {
    }
    #endregion

    #region CommandMethods

    #region Command_PropertyQuery
    /// <summary>
    /// Command implementation for PropertyQuery.
    /// </summary>
    [Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertySampleMgd", "PropertyQuery", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Command_PropertyQuery()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        
        // prompt for the property name and value of interest
        PromptResult res1 = ed.GetString("Property name: ");
        PromptResult res2 = ed.GetString("Property value: ");

        String propName = res1.StringResult;
        String propValue = res2.StringResult; 
        
        Database db = HostApplicationServices.WorkingDatabase;
        DBTransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        try
        {
            BlockTable bt = (BlockTable)tm.GetObject(db.BlockTableId, OpenMode.ForRead, false);
            BlockTableRecord btr = (BlockTableRecord)tm.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead, false);
        
            foreach(ObjectId id in btr) 
            {
                bool found = false;
                bool foundFromStyle = false;

                // get the property sets
                ObjectIdCollection setIds;
                GetPropertySets(id, out setIds);

                if (setIds.Count > 0)
                    found = FindPropertyValue(id, setIds, propName, propValue);
                
                // get the property sets from style
                ObjectIdCollection setIdsFromStyle;
                GetPropertySetsFromStyle(id, out setIdsFromStyle);

                if (setIdsFromStyle.Count > 0)
                    foundFromStyle = FindPropertyValue(id, setIdsFromStyle, propName, propValue);
                
                // hilight if found
                if (found || foundFromStyle)
                {
                    Object obj = tm.GetObject(id, OpenMode.ForRead, false, false);
                    
                    AcadDb.Entity ent = obj as AcadDb.Entity;
                    if (ent == null)
                        return;

                    ObjectId[] ids = new ObjectId[1]; ids[0] = ObjectId.Null;
                    SubentityId index = new SubentityId(SubentityType.Null, 0);
                    FullSubentityPath path = new FullSubentityPath(ids, index);
                    ent.Highlight(path, true);
                }
            }
        }
        catch
        {
        }
        finally
        {
            trans.Dispose();
        }
    }
    #endregion

    #region Command_ListPropertySetDefByName
    /// <summary>
    /// Command implementation for ListPropertySetDefByName.
    /// </summary>
    [Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertySampleMgd", "ListPropertySetDefByName", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Command_ListPropSetDefByName()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        // prompt for the property set definition name to list
        PromptResult res1 = ed.GetString("\nEnter Property Set Definition name: ");

        if (res1.Status != PromptStatus.OK)
            return;

        String psdName = res1.StringResult;

        // Find and get the property set definition objectId by it's name. 
        // Once we have the objectId we can open and inspect it.
        ObjectId psdId = GetPropertySetDefinitionIdByName(psdName);

        if (psdId == ObjectId.Null)
        {
            ed.WriteMessage("\nThe property set definition named \"" + psdName + "\" does not exit in this drawing.");
            return;
        }

        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;
        using (Transaction trans = tm.StartTransaction())
        {
            AecPropDb.PropertySetDefinition psd = trans.GetObject(psdId, OpenMode.ForRead) as AecPropDb.PropertySetDefinition;
            ed.WriteMessage("\n\nProperty Set Definition Name: " + psd.Name);
            ed.WriteMessage("\nDisplay Name: " + psd.DisplayName);
            ed.WriteMessage("\nDescription: " + psd.Description);
            ed.WriteMessage("\nIs Visible: " + psd.IsVisible.ToString());
            System.Collections.Specialized.StringCollection appliesto = psd.AppliesToFilter;
            foreach (string s in appliesto)
            {
                ed.WriteMessage("\n  Applies To: " + s);
            }
            AecPropDb.PropertyDefinitionCollection propdefs = psd.Definitions;
            ed.WriteMessage("\nDefinitions Count: " + propdefs.Count.ToString() + "\n");

            foreach (AecPropDb.PropertyDefinition propdef in propdefs)
            {
                ed.WriteMessage("\n  PropDef Name: " + propdef.Name);
                ed.WriteMessage("\n    Description: " + propdef.Description);
                ed.WriteMessage("\n    IsVisible: " + propdef.IsVisible.ToString());
                ed.WriteMessage("\n    IsAutomatic: " + propdef.Automatic);
                try
                {
                    ed.WriteMessage("\n    UnitType: " + propdef.UnitType.ToString());
                }
                catch { }

                ed.WriteMessage("\n    DataType: " + propdef.DataType.ToString());
                ed.WriteMessage("\n    DefaultData: " + propdef.DefaultData.ToString());
            }
        }
    }
    #endregion CommandMethods

    #region Command_CreatePropertySetDef
    /// <summary>
    /// Command implementation for CreatePropertySetDef.
    /// </summary>
    [Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertySampleMgd", "CreatePropertySetDef", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Command_CreatePropertySetDef()
    {

        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        // prompt for the property set definition name to create
        PromptResult res1 = ed.GetString("\nEnter Property Set Definition name: ");

        if (res1.Status != PromptStatus.OK)
            return;

        String psdname = res1.StringResult;

        Database db = Application.DocumentManager.MdiActiveDocument.Database;

        // Setup an array of objects that this property set definition will apply to
        System.Collections.Specialized.StringCollection appliesto = new System.Collections.Specialized.StringCollection();
        appliesto.Add("AecDbWall");
        appliesto.Add("AecDbWindow");
        appliesto.Add("AcDbLine");
        appliesto.Add("AcDbCircle");

        // Create a list of property definitions that will reside in the property set definition
        System.Collections.ArrayList defs = new System.Collections.ArrayList();
        AecPropDb.PropertyDefinition def;
        AcadDb.TransactionManager tm = db.TransactionManager;

        for (int i = 0; i < 10; i++)
        {
            def = new AecPropDb.PropertyDefinition();
            def.SetToStandard(db);
            def.SubSetDatabaseDefaults(db);
            def.Name = "Test" + i.ToString();
            def.DataType = Autodesk.Aec.PropertyData.DataType.Integer;
            def.DefaultData = (Object)i;
            defs.Add(def);
        }

        // Creates the property set and provides back the objectId.
        ObjectId id = CreatePropertySetDefinition(psdname, appliesto, defs);
    }
    #endregion

    #region Command_FindPropertySetByNameOnObject
    /// <summary>
    /// Command implementation for FindPropertySetByNameOnObject.
    /// </summary>
    [Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertySampleMgd", "FindPropertySetByNameOnObject", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Command_FindPropertySetByNameOnObject()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        try
        {
            // prompt for the property set name
            PromptResult res1 = ed.GetString("\nEnter Property Set name: ");

            if (res1.Status != PromptStatus.OK)
                return;

            String psname = res1.StringResult;

            PromptEntityOptions entopts = new PromptEntityOptions("\nSelect an entity to find property set ");
            PromptEntityResult ent = null;
            try
            {
                ent = ed.GetEntity(entopts);
            }
            catch
            {
                ed.WriteMessage("\nYou did not select a valid entity");
                return;
            }

            if (ent.Status == PromptStatus.OK)
            {
                ObjectId entid = ent.ObjectId;
                Database db = Application.DocumentManager.MdiActiveDocument.Database;
                Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;
                using (Transaction myT = tm.StartTransaction())
                {
                    Autodesk.AutoCAD.DatabaseServices.Entity entity = (Autodesk.AutoCAD.DatabaseServices.Entity)tm.GetObject(entid, OpenMode.ForRead, true);
                    bool wasFound = GetPropSetByNameFromObject(entity, psname);

                    if (wasFound)
                    {
                        ed.WriteMessage("\n Found Property Set named \"" + psname + "\" on entity selected");
                        // See Also Autodesk.Aec.PropertyData.DatabaseServices.PropertyDataServices.GetPropertySets(ObjectId, ObjectId, ObjectIdCollection)
                        // that one handles xrefs via the blockref path mechanism.
                        ObjectIdCollection ids = AecPropDb.PropertyDataServices.GetPropertySets(entity);
                        ed.WriteMessage("\n Found a total of " + ids.Count + " property sets on entity selected");
                    }
                    else
                        ed.WriteMessage("\n DID NOT Find Property Set named \"" + psname + "\" on entity selected");

                    
                }
            }
        }
        catch (SystemException e)
        {
            ed.WriteMessage("Exception: " + e.Message);
        }
        finally
        {

        }

    }
    #endregion Command_FindPropertySetByNameOnObject

    #region Command_GetPropSetDefIdFromPropertyName
    /// <summary>
    /// Command implementation for GetPropSetDefIdFromPropertyName.
    /// </summary>
    
    [Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertySampleMgd", "GetPropSetDefIdFromPropertyName", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Command_GetPropSetDefIdFromPropertyName()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        // prompt for the property name
        PromptResult res1 = ed.GetString("\nEnter Property name: ");

        if (res1.Status != PromptStatus.OK)
            return;

        String pname = res1.StringResult;

        Database db = HostApplicationServices.WorkingDatabase;
        AcadDb.TransactionManager tm = db.TransactionManager;

        using (Transaction trans = tm.StartTransaction())
        {
            BlockTable bt = (BlockTable)tm.GetObject(db.BlockTableId, OpenMode.ForRead, false);
            BlockTableRecord btr = (BlockTableRecord)tm.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead, false);

            foreach (ObjectId id in btr)
            {

                // get the property sets
                ObjectIdCollection setIds;
                AcadDb.DBObject dbobj = tm.GetObject(id, OpenMode.ForRead, false, false);
                setIds = AecPropDb.PropertyDataServices.GetPropertySets(dbobj);

                foreach (ObjectId psId in setIds)
                {
                    AecPropDb.PropertySet pset = tm.GetObject(psId, OpenMode.ForRead, false, false) as AecPropDb.PropertySet;
                    
                    // First see if the property with given name is in this set. if not, go to next set.
                    int pid;
                    try
                    {
                        pid = pset.PropertyNameToId(pname);
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception)
                    {
                        // most likely eKeyNotfound skip
                        continue;
                    }

                    ObjectId psdefId = pset.PropertySetDefinition;
                    ed.WriteMessage("\n Property Set with id: " + psId.ToString() + " contains a property with name: " + pname);
                    ed.WriteMessage("\n and refernces the property set definition with id: " + psdefId.ToString());

                    // Lets also output some property set information about the property
                    AecPropDb.PropertyValueUnitPair value_unit = pset.GetValueAndUnitAt(pid);

                    Object val = value_unit.Value;

                    ed.WriteMessage("\n UnitType = " + value_unit.UnitType.InternalName + ", IsImperial = " + value_unit.UnitType.IsImperial.ToString() + ", Type = " + value_unit.UnitType.Type.ToString());
                    ed.WriteMessage("\n DataType = " + val.GetType().ToString());
                    ed.WriteMessage("\n Value = " + val.ToString());

                }

            }
            trans.Commit();
        }
    }

    #endregion Command_GetPropSetDefIdFromPropertyName

    #region Command_GetPropertyDataByName
    /// <summary>
    /// Command implementation for GetPropertyDataByName.
    /// </summary>

    [Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertySampleMgd", "GetPropertyDataByName", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Command_GetPropertyDataByName()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        // prompt for the property name
        PromptResult res1 = ed.GetString("\nEnter Property name: ");

        if (res1.Status != PromptStatus.OK)
            return;

        String pname = res1.StringResult;

        Database db = HostApplicationServices.WorkingDatabase;
        AcadDb.TransactionManager tm = db.TransactionManager;

        AcadDb.DBObject dbobj;

        using (Transaction trans = tm.StartTransaction())
        {
            BlockTable bt = (BlockTable)tm.GetObject(db.BlockTableId, OpenMode.ForRead, false);
            BlockTableRecord btr = (BlockTableRecord)tm.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead, false);

            foreach (ObjectId id in btr)
            {

                dbobj = tm.GetObject(id, OpenMode.ForRead, false, false);

                System.Collections.ArrayList values = GetValuesFromPropertySetByName(pname, dbobj);
                foreach (AecPropDb.PropertyValueUnitPair value_unit in values)
                {
                    ed.WriteMessage("\nProperty with name = " + pname + " Entity ObjectId = " + id.ToString());
                    ed.WriteMessage("\nUnit Type = " + value_unit.UnitType.InternalName + ", IsImperial = " + value_unit.UnitType.IsImperial.ToString() + ", Type = " + value_unit.UnitType.Type.ToString());
                    
                    Object val = value_unit.Value; 
                    if (val != null)
                    {
                        ed.WriteMessage("\nDataType = " + val.GetType().ToString());
                        ed.WriteMessage("\nValue = " + val.ToString());
                    }

                }
            }
            trans.Commit();
        }
    }
    #endregion Command_GetPropertyDataByName

    #region Command_SetPropertyDataByName
    /// <summary>
    /// Command implementation for SetPropertyDataByName.
    /// </summary>

    [Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertySampleMgd", "SetPropertyDataByName", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Command_SetPropertyDataByName()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        // prompt for the property name
        PromptResult res1 = ed.GetString("\nEnter Property name (must be a floating point type for this sample): ");

        if (res1.Status != PromptStatus.OK)
            return;

        String pname = res1.StringResult;


        PromptDoubleResult res2 = ed.GetDouble("\nEnter new Property value (floating point number): ");

        if (res2.Status != PromptStatus.OK)
            return;

        Double newValue = res2.Value;


        Database db = HostApplicationServices.WorkingDatabase;
        AcadDb.TransactionManager tm = db.TransactionManager;

        AcadDb.DBObject dbobj;

        using (Transaction trans = tm.StartTransaction())
        {
            BlockTable bt = (BlockTable)tm.GetObject(db.BlockTableId, OpenMode.ForRead, false);
            BlockTableRecord btr = (BlockTableRecord)tm.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead, false);

            foreach (ObjectId id in btr)
            {
                dbobj = tm.GetObject(id, OpenMode.ForRead, false, false);
                System.Collections.ArrayList values = GetValuesFromPropertySetByName(pname, dbobj);
                foreach (AecPropDb.PropertyValueUnitPair value_unit in values)
                {
                    Object currentValue = value_unit.Value;
                    if (currentValue != null)
                    {
                        if (currentValue.GetType() == typeof(double))
                        {
                            using (Transaction trans2 = tm.StartTransaction())
                            {
                                dbobj.UpgradeOpen();
                                bool WasChanged = SetValuesFromPropertySetByName(pname, dbobj, newValue);
                                if (WasChanged)
                                {
                                    ed.WriteMessage("\nSuccesfully changed value for objectId = " + id.ToString());
                                    ed.WriteMessage("\nCurrent Value = " + currentValue.ToString() + " New Value = " + newValue.ToString());
                                }
                                else
                                {
                                    ed.WriteMessage("\nFailed to change value for objectId = " + id.ToString());
                                }
                                trans2.Commit();
                            }
                        }
                    }
                }
            }
            trans.Commit();
        }
    }
    #endregion Command_SetPropertyDataByName

    #region Command_CreatePropSetOnDBObject
    /// <summary>
    /// Command implementation for CreatePropSetOnDBObject.
    /// </summary>

    [Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertySampleMgd", "CreatePropSetOnDBObject", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Command_CreatePropSetOnDBObject()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        try
        {
            PromptEntityOptions entopts = new PromptEntityOptions("Select an entity to add a property set to: ");
            PromptEntityResult ent = null;
            try
            {
                ent = ed.GetEntity(entopts);
            }
            catch
            {
                ed.WriteMessage("You did not select a valid entity");
                return;
            }

            if (ent.Status != PromptStatus.OK)
                return;

            // prompt for the property name
            PromptResult res1 = ed.GetString("\nEnter Property set definition name: ");

            if (res1.Status != PromptStatus.OK)
                return;

            String pname = res1.StringResult;


            ObjectId propsetId = GetPropertySetDefinitionIdByName(pname);
            if (propsetId.IsNull)
                ed.WriteMessage("\n There are no property set definitions by that name");
            
            ObjectId entid = ent.ObjectId;
            bool WasCreated = CreatePropSetOnDBObject(entid, propsetId);

            if (WasCreated)
                ed.WriteMessage("\n Successfully created property set named " + pname + " on selected entity");
            else
                ed.WriteMessage("\n Failed to create property set on entity.");
        }
        catch (SystemException)
        {
        }
    }
    #endregion Command_CreatePropSetOnDBObject

    #region Command_CreatePropSetDefintionWithAutomaticData
    /// <summary>
    /// Command implementation for CreatePropSetDefAuto.
    /// </summary>

    [Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertySampleMgd", "CreatePropSetDefAuto", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Command_CreatePropSetDefintionWithAutomaticData()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        try
        {

            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            // prompt for the property name
            PromptResult result = ed.GetString("\nEnter new property set definition name: ");

            if (result.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nError with input...");
                return;
            }

            String pname = result.StringResult;

            DictionaryPropertySetDefinitions dictDefs = new DictionaryPropertySetDefinitions(db);
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                if (dictDefs.Has(pname, trans) == true)
                {
                    ed.WriteMessage("\nProperty set with that name already exists. Try again, please...");
                    return;
                }
            }

            result = ed.GetString("\nEnter class name to work with: ");

            if (result.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nError with input...");
                return;
            }

            String className = result.StringResult;

            Autodesk.AutoCAD.Runtime.Dictionary runtime = Autodesk.AutoCAD.Runtime.SystemObjects.ClassDictionary;
            if (runtime.Contains(className) == false)
            {
                ed.WriteMessage("\nObjects of that class type are not present in the system. For example, for wall it will be AecDbWall.");
                return;
            }

            bool WasCreated = CreatePropertySetDefinitionWithAutoData(pname, className);

            if (WasCreated)
                ed.WriteMessage("\n Successfully created property set definition named " + pname + " with all available automatic data.");
            else
                ed.WriteMessage("\n Failed to create property set definition.");
        }
        catch (SystemException)
        {
        }
    }
    #endregion Command_CreatePropSetDefintionWithAutomaticData

    #region Command_CreatePropSetDefintionWithAutomaticDataMultiClass
    /// <summary>
    /// Command implementation for CreatePropSetDefAutoDup.
    /// </summary>

    [Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertySampleMgd", "CreatePropSetDefAutoDup", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Command_CreatePropSetDefintionWithAutomaticDataMultiClass()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        try
        {

            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            // prompt for the property name
            PromptResult result = ed.GetString("\nEnter new property set definition name: ");

            if (result.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nError with input...");
                return;
            }

            String pname = result.StringResult;

            DictionaryPropertySetDefinitions dictDefs = new DictionaryPropertySetDefinitions(db);
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                if (dictDefs.Has(pname, trans) == true)
                {
                    ed.WriteMessage("\nProperty set with that name already exists. Try again, please...");
                    return;
                }
            }

            
            System.Collections.Specialized.StringCollection classNames = new System.Collections.Specialized.StringCollection();
            Autodesk.AutoCAD.Runtime.Dictionary runtime = Autodesk.AutoCAD.Runtime.SystemObjects.ClassDictionary;

            result = ed.GetString("\nEnter first class name to work with: ");

            if (result.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nError with input...");
                return;
            }

            String className = result.StringResult;
            if (runtime.Contains(className) == true)
            {
                classNames.Add(className);
            } else {
                ed.WriteMessage("\nObjects of that class type are not present in the system. For example, for wall it will be in the format: AecDbWall.");
                return;
            }

            result = ed.GetString("\nEnter second class name to work with: ");

            if (result.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nError with input...");
                return;
            }

            className = result.StringResult;
            if (runtime.Contains(className) == true)
            {
                classNames.Add(result.StringResult);
            }
            else
            {
                ed.WriteMessage("\nObjects of that class type are not present in the system. For example, for wall it will be in the format: AecDbWall.");
                return;
            }


            bool WasCreated = CreatePropertySetDefinitionWithAutoDataDup(pname, classNames);

            if (WasCreated)
                ed.WriteMessage("\n Successfully created property set definition named " + pname + " with all available automatic data.");
            else
                ed.WriteMessage("\n Failed to create property set definition.");
        }
        catch (SystemException)
        {
        }
    }
    #endregion Command_CreatePropSetDefintionWithAutomaticDataMultiClass
 
    #region Command_GetPropertyXRef
    /// <summary>
    /// Command implementation for Get property X ref.
    /// </summary>
    [Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertySampleMgd", "GetPropertyXRef", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Command_GetPropertyXRef()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        DBTransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        try
        {
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = "\nSelect an Object from X reference:";
            pso.SingleOnly = true;
            PromptSelectionResult result = ed.GetSelection(pso);
            if (result.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nError with input...");
                return;
            }

            PromptStringOptions pso2 = new PromptStringOptions("Input the Property Set Name:");
            pso2.DefaultValue = "WallObjects";
            string testSetName = ed.GetString(pso2).StringResult;

            PromptStringOptions pso3 = new PromptStringOptions("Input the Property Name:");
            pso3.DefaultValue = "Width";
            string testPropName = ed.GetString(pso3).StringResult;

            ObjectIdAndBlockReferencePathCollection idRefPaths = new ObjectIdAndBlockReferencePathCollection();
            foreach (SelectedObject selectedObj in result.Value)
            {
                if (selectedObj.ObjectId.ObjectClass == RXObject.GetClass(typeof(Autodesk.AutoCAD.DatabaseServices.BlockReference)))
                {
                    Autodesk.AutoCAD.DatabaseServices.BlockReference br = trans.GetObject(selectedObj.ObjectId, OpenMode.ForWrite) as Autodesk.AutoCAD.DatabaseServices.BlockReference;
                    BlockTableRecord btr = trans.GetObject(br.BlockTableRecord, OpenMode.ForWrite) as BlockTableRecord;
                    foreach (ObjectId id in btr)
                    {
                        int index = idRefPaths.Add(new ObjectIdAndBlockReferencePath(id));
                        ShowPropertyData(id, idRefPaths[index].BlockReferencePath,testSetName,testPropName);
                    }
                }
            }
            trans.Commit();
        }
        catch (System.Exception e)
        {
            ed.WriteMessage(e.Message);
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
    }

    #endregion Command_GetPropertyXRef

    // End of command implementations
    #endregion

    #region ImplementationMethods

    #region GetPropertySets
    /// <summary>
    /// Gets the property sets for the object.
    /// </summary>
    /// <param name="id">The object id.</param>
    /// <param name="setIds">The property set ids.</param>
    private void GetPropertySets(ObjectId id, out ObjectIdCollection setIds)
    {
        setIds = new ObjectIdCollection();

        Database db = HostApplicationServices.WorkingDatabase;
        DBTransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        try
        {
            Object obj = tm.GetObject(id, OpenMode.ForRead, false, false);
                    
            AecDb.Entity ent = obj as AecDb.Entity;
            if (ent == null)
                return;

            // get the property sets on the object
            setIds = AecPropDb.PropertyDataServices.GetPropertySets(ent);
        }
        catch
        {
        }
        finally
        {
            trans.Dispose();
        }
    }
    #endregion GetPropertySets

    #region GetPropertySetsFromStyle
    /// <summary>
    /// Get the property sets for the object's style.
    /// </summary>
    /// <param name="id">The object id.</param>
    /// <param name="setIds">The property set ids.</param>
    private void GetPropertySetsFromStyle(ObjectId id, out ObjectIdCollection setIds)
    {
        setIds = new ObjectIdCollection();

        Database db = HostApplicationServices.WorkingDatabase;
        DBTransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        try
        {
            Object obj = tm.GetObject(id, OpenMode.ForRead, false, false);
                    
            AecDb.Entity ent = obj as AecDb.Entity;
            if (ent == null)
                return;

            // use late binding to see if the entity has a StyleId property
            obj = ent.GetType().InvokeMember("StyleId",System.Reflection.BindingFlags.GetProperty,null,ent,null);
            if (!(obj is ObjectId))
                return;

            ObjectId styleId = (ObjectId)obj;
            if (styleId == ObjectId.Null)
                return;

            obj = tm.GetObject(styleId, OpenMode.ForRead, false, false);
            AecDb.DBObject style = obj as AecDb.DBObject;
            if (style == null)
                return;

            // get the property sets from style
            setIds = PropertyDataServices.GetPropertySets(style);
        }
        catch
        {
        }
        finally
        {
            trans.Dispose();
        }
    }
    #endregion GetPropertySetsFromStyle

    #region FindPropertyValue
    /// <summary>
    /// Finds the property with the given value.
    /// </summary>
    /// <param name="id">The object id.</param>
    /// <param name="setIds">The property set ids.</param>
    /// <param name="propName">The property name.</param>
    /// <param name="propValue">The property value.</param>
    /// <returns>true, if a property with the specified value is found.</returns>
    private bool FindPropertyValue(ObjectId id, ObjectIdCollection setIds, String propName, String propValue)
    {
        bool result = false;

        Database db = HostApplicationServices.WorkingDatabase;
        DBTransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        try
        {
            foreach (ObjectId setId in setIds)
            {
                Object obj = tm.GetObject(setId, OpenMode.ForRead, false, false);
                        
                // open the prop set
                PropertySet propSet = obj as PropertySet;
                if (propSet == null)
                    continue;
                
                int index = -1;
                Object data;
                
                try
                {   
                    index = propSet.PropertyNameToId(propName);
                }
                catch
                {
                }

                // see if we have a manual property
                if (index > -1)
                {
                    data = propSet.GetAt(index);
   
                    // check for a match
                    if (data.ToString() == propValue)
                    {
                        result = true;
                        break;
                    }
                }

                // go through the prop set def for automatic properties
                ObjectId idPropSetDef = propSet.PropertySetDefinition;
                if (idPropSetDef == ObjectId.Null)
                    continue;

                obj = tm.GetObject(idPropSetDef, OpenMode.ForRead, false, false);

                PropertySetDefinition propSetDef = obj as PropertySetDefinition;
                if (propSetDef == null)
                    continue;

                // find the automatic property
                bool bFound = false;
                foreach (PropertyDefinition propDef in propSetDef.Definitions)
                {
                    if (propDef.Name == propName)
                        bFound = true;
                }
                
                if (!bFound)
                    continue;

                // get the data
                ObjectIdCollection idBlockRefPaths = new ObjectIdCollection();
                data = propSetDef.GetValue(index, id, idBlockRefPaths);

                // check for a match
                if (data.ToString() == propValue)
                {
                    result = true;
                    break;
                }
            }
        }
        catch
        {
        }
        finally
        {
            trans.Dispose();
        }

        return result;
    }
    #endregion FindPropertyValue

    #region GetPropertySetDefinitionIdByName
    /// <summary>
    /// Finds the property set definition objectId with the given name.
    /// </summary>
    /// <param name="psdName">The property set definition name.</param>
    /// <returns>true, if a property with the specified value is found.</returns>
    private ObjectId GetPropertySetDefinitionIdByName(string psdName)
    {
        ObjectId psdId = ObjectId.Null;
        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;
        using (Transaction trans = tm.StartTransaction())
        {
            AecPropDb.DictionaryPropertySetDefinitions psdDict = new AecPropDb.DictionaryPropertySetDefinitions(db);
            if (psdDict.Has(psdName, trans))
            {
                psdId = psdDict.GetAt(psdName);
            }

            trans.Commit();
        }

        return psdId;
    }
    #endregion GetPropertySetDefinitionIdByName

    #region CreatePropertySetDefinition
    /// <summary>
    /// Creates a property set definition with given name, applies to filter, and property definitions.
    /// </summary>
    /// <param name="psdName">The property set definition name.</param>
    /// <param name="appliesto">An array of strings that represent the objects this property set will apply to.</param>
    /// <param name="propdefs">An array of property definitions that will be contained by this property set definition.</param>
    /// <returns> The objectId of the eixtsing or newly created property set definiton.</returns>
    private ObjectId CreatePropertySetDefinition(string psdname,
                                                System.Collections.Specialized.StringCollection appliesto,
                                                System.Collections.ArrayList propdefs)
    {
        ObjectId psdId;

        // first let's see if it exists... If so just return its ObjectId.
        psdId = GetPropertySetDefinitionIdByName(psdname);
        if (psdId != ObjectId.Null)
            return psdId;

        Database db = Application.DocumentManager.MdiActiveDocument.Database;

        // Create the new property set definition;
        Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;
        AecPropDb.PropertySetDefinition psd = new AecPropDb.PropertySetDefinition();
        psd.SetToStandard(db);
        psd.SubSetDatabaseDefaults(db);
        psd.AlternateName = psdname;
        psd.IsLocked = false;
        psd.IsVisible = true;
        psd.IsWriteable = true;

        psd.SetAppliesToFilter(appliesto, false);

        // Place the property definitions into the new property set by
        // first getting the collection of property definitions (in this
        // case the collection is empty because it is new), and the new 
        // property defintions to the collection.
        AecPropDb.PropertyDefinitionCollection psdefs = psd.Definitions;
        foreach (AecPropDb.PropertyDefinition propdef in propdefs)
        {
            psdefs.Add(propdef);
        }

        // Add the property set definition to the dictionary
        using (Transaction trans = tm.StartTransaction())
        {

            AecPropDb.DictionaryPropertySetDefinitions psdDict = new AecPropDb.DictionaryPropertySetDefinitions(db);
            psdDict.AddNewRecord(psdname, psd);
            tm.AddNewlyCreatedDBObject(psd, true);

            psdId = psd.ObjectId;
            trans.Commit();
        }

        return psdId;
    }

    #endregion CreatePropertySetDefinition

    #region GetPropSetByNameFromObject
    /// <summary>
    /// Finds the property set by its name on a given object.
    /// </summary>
    /// <param name="dbobj">The property set definition name.</param>
    /// <param name="propname">The property name to find on the object.</param>
    /// <returns> true if the property set with the given name was found, or false otherwise. </returns>
    private bool GetPropSetByNameFromObject(AcadDb.DBObject dbobj, String propname)
    {
        ObjectId definitionId = GetPropertySetDefinitionIdByName(propname);
        ObjectId propSetId = ObjectId.Null;
        if (!definitionId.IsNull) 
        {
            try
            {
                propSetId = AecPropDb.PropertyDataServices.GetPropertySet(dbobj, definitionId);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception)
            {
                // More than likely eKeyNotFound, so this is a more specific
                // place to handle a "failed to find" condition.
            }
        }

        if (!propSetId.IsNull)
            return true; // got an ID so we found it

        return false; // didn't find it by name on this object.
    }
    #endregion GetPropSetByNameFromObject

    #region GetValuesFromPropertySetByName
    /// <summary>
    /// Returns the values (PropertyValueUnitPair) of a property by name on a given object.
    /// </summary>
    /// <param name="pname">The property name to find on the object.</param>
    /// <param name="dbobj">The object to find the property on. </param>
    /// <returns> An array of the values </returns>
    public System.Collections.ArrayList GetValuesFromPropertySetByName(string pname, AcadDb.DBObject dbobj)
    {

        ObjectIdCollection setIds = AecPropDb.PropertyDataServices.GetPropertySets(dbobj);

        System.Collections.ArrayList values = new System.Collections.ArrayList();

        if (setIds.Count == 0)
            return values; // just return emtpy collection...

        
        Database db = HostApplicationServices.WorkingDatabase;
        AcadDb.TransactionManager tm = db.TransactionManager;
        foreach (ObjectId psId in setIds)
        {
            AecPropDb.PropertySet pset = tm.GetObject(psId, OpenMode.ForRead, false, false) as AecPropDb.PropertySet;
            int pid;
            try
            {
                pid = pset.PropertyNameToId(pname);
                values.Add(pset.GetValueAndUnitAt(pid));
            }
            catch (Autodesk.AutoCAD.Runtime.Exception)
            {
                // most likely eKeyNotfound.
                ;
            }
            
        }

        return values;
    }
    #endregion GetValuesFromPropertySetByName

    #region SetValuesFromPropertySetByName
    /// <summary>
    /// Sets the values (PropertyValueUnitPair) of a property by name on a given object.
    /// </summary>
    /// <param name="pname">The property name to find on the object.</param>
    /// <param name="dbobj">The object to set the property on. </param>
    /// <param name="value">The value to set. </param>
    /// <returns> true if succesful, or false otherwise. </returns>
    public bool SetValuesFromPropertySetByName(string pname, AcadDb.DBObject dbobj, Object value)
    {
        bool findany = false;
        ObjectIdCollection setIds = AecPropDb.PropertyDataServices.GetPropertySets(dbobj);

        Database db = HostApplicationServices.WorkingDatabase;
        AcadDb.TransactionManager tm = db.TransactionManager;
        using (Transaction trans = tm.StartTransaction())
        {
            foreach (ObjectId psId in setIds)
            {
                AecPropDb.PropertySet pset = tm.GetObject(psId, OpenMode.ForWrite, false, false) as AecPropDb.PropertySet;
                int pid;
                try
                {
                    pid = pset.PropertyNameToId(pname);
                    if (pset.IsWriteEnabled)
                    {
                        pset.SetAt(pid, value);
                    }

                    findany = true;
                }
                catch (Autodesk.AutoCAD.Runtime.Exception)
                {
                    // most likely eKeyNotfound
                    ;
                }

            }
            trans.Commit();
        }

        return findany;

    }
    #endregion SetValuesFromPropertySetByName

    #region CreatePropSetOnDBObject
    /// <summary>
    /// Creates a new property set on the given DBObject.
    /// </summary>
    /// <param name="pname">The property name to find on the object.</param>
    /// <param name="dbobj">The object to find the property on. </param>
    /// <param name="value">The value to set. </param>
    /// <returns> true is succesful, or false otherwise. </returns>
    public bool CreatePropSetOnDBObject(ObjectId dbobjId, ObjectId propsetdefId)
    {
        Database db = HostApplicationServices.WorkingDatabase;
        AcadDb.TransactionManager tm = db.TransactionManager;
        using (Transaction trans = tm.StartTransaction())
        {
            try
            {
                AcadDb.DBObject dbobj = tm.GetObject(dbobjId, OpenMode.ForWrite, false, false);
                AecPropDb.PropertyDataServices.AddPropertySet(dbobj, propsetdefId);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception)
            {
                return false; // failure
            }

            trans.Commit();
        }

        return true;
    }
    #endregion CreatePropSetOnDBObject

    #region CreatePropertySetDefinitionWithAutoData
    /// <summary>
    /// Creates a new property set and adds all the available automatic source names as automatic data.
    /// </summary>
    /// <param name="propertySetDefName">The property set definition name to create.</param>
    /// <param name="className"> The class name used for the applies-to filter and the available source names. </param>
    /// <returns> true is succesful, or false otherwise. </returns>
    public bool CreatePropertySetDefinitionWithAutoData(String propertySetDefName, String className)
    {
        Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
        AcadDb.TransactionManager tm = db.TransactionManager;
        using (AcadDb.Transaction tx = tm.StartTransaction())
        {
            try
            {
                AecPropDb.PropertySetDefinition setDef = new AecPropDb.PropertySetDefinition();
                setDef.SetToStandard(db);
                setDef.SubSetDatabaseDefaults(db);
                System.Collections.Specialized.StringCollection filters = new System.Collections.Specialized.StringCollection();
                filters.Add(className);
                setDef.SetAppliesToFilter(filters, false);

                AecPropDb.PropertyDefinitionCollection defs = setDef.Definitions;

                System.String[] sourceNames = AecPropDb.PropertyDataServices.FindAutomaticSourceNames(className, db);
                foreach (String sourceName in sourceNames)
                {
                    AecPropDb.PropertyDefinition testAutoDef = new AecPropDb.PropertyDefinition();
                    testAutoDef.SetToStandard(db);
                    testAutoDef.SubSetDatabaseDefaults(db);
                    testAutoDef.Name = "AutoSample " + sourceName;
                    testAutoDef.Automatic = true;
                    testAutoDef.Description = "Automatic data test:" + sourceName;
                    testAutoDef.IsVisible = true;
                    testAutoDef.IsReadOnly = true;
                    testAutoDef.SetAutomaticData(className, sourceName);
                    defs.Add(testAutoDef);
                }

                AecPropDb.DictionaryPropertySetDefinitions propDefs = new AecPropDb.DictionaryPropertySetDefinitions(db);
                propDefs.AddNewRecord(propertySetDefName, setDef);
                tm.AddNewlyCreatedDBObject(setDef, true);
            }
            catch (System.Exception)
            {
                return false;
            }

            tx.Commit();
        }

        return true;
    }
    #endregion CreatePropertySetDefinitionWithAutoData

    #region CreatePropertySetDefinitionWithAutoDataDup
    /// <summary>
    /// Creates a new property set and adds the COMMON automatic source names as automatic data.
    /// </summary>
    /// <param name="propertySetDefName">The property set definition name to create.</param>
    /// <param name="className"> Input two class names which are used for the applies-to filter and the available common source names. </param>
    /// <returns> true is succesful, or false otherwise. </returns>
    public bool CreatePropertySetDefinitionWithAutoDataDup(String propertySetDefName, System.Collections.Specialized.StringCollection classNames)
    {
        Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
        AcadDb.TransactionManager tm = db.TransactionManager;
        using (AcadDb.Transaction tx = tm.StartTransaction())
        {
            try
            {
                AecPropDb.PropertySetDefinition setDef = new AecPropDb.PropertySetDefinition();
                setDef.SetToStandard(db);
                setDef.SubSetDatabaseDefaults(db);
                System.Collections.Specialized.StringCollection filters = new System.Collections.Specialized.StringCollection();
                foreach (String className in classNames)
                {
                    filters.Add(className);
                }
                setDef.SetAppliesToFilter(filters, false);

                AecPropDb.PropertyDefinitionCollection defs = setDef.Definitions;

                System.Collections.Specialized.StringCollection dupes = new System.Collections.Specialized.StringCollection();
                System.Collections.Specialized.StringCollection[] compare = new System.Collections.Specialized.StringCollection[classNames.Count];
                int i = 0;
                bool bFirst = true;
                
                foreach (String className in classNames)
                {
                    System.String[] sourceNames = AecPropDb.PropertyDataServices.FindAutomaticSourceNames(className, db);
                    compare[i] = new System.Collections.Specialized.StringCollection();
                    compare[i].AddRange(sourceNames);
                    if (!bFirst)
                    {
                        foreach (String sourceName in sourceNames)
                        {
                            if (compare[i - 1].Contains(sourceName))
                                dupes.Add(sourceName);
                        }
                    }
                    bFirst = false;
                    i++;
                }

                foreach (String sourceName in dupes)
                {
                    AecPropDb.PropertyDefinition testAutoDef = new AecPropDb.PropertyDefinition();
                    testAutoDef.SetToStandard(db);
                    testAutoDef.SubSetDatabaseDefaults(db);
                    testAutoDef.Name = sourceName;
                    testAutoDef.Automatic = true;
                    testAutoDef.Description = "Automatic data test";
                    testAutoDef.IsVisible = true;
                    testAutoDef.IsReadOnly = true;
                    testAutoDef.SetAutomaticData(classNames[0], sourceName);
                    testAutoDef.SetAutomaticData(classNames[1], sourceName);
                    defs.Add(testAutoDef);

                }

                AecPropDb.DictionaryPropertySetDefinitions propDefs = new AecPropDb.DictionaryPropertySetDefinitions(db);
                propDefs.AddNewRecord(propertySetDefName, setDef);
                tm.AddNewlyCreatedDBObject(setDef, true);
            }
            catch (System.Exception)
            {
                return false;
            }

            tx.Commit();
        }

        return true;
    }
    #endregion CreatePropertySetDefinitionWithAutoDataDup

    #region GetScheduleDataProperty
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj">The object. </param>
    /// <param name="blockReferencePath">The block reference path.</param>
    /// <param name="setName">The set name.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns></returns>
    private Object GetScheduleDataProperty(Autodesk.AutoCAD.DatabaseServices.DBObject obj, ObjectIdCollection blockReferencePath, string setName, string propertyName)
    {
        string fullPropertyName = setName + ":" + propertyName;
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        Database db = obj.Database;

        //Database db = Application.DocumentManager.MdiActiveDocument.Database;
        DBTransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();
        PropertyValueUnitPair result = null;
        try
        {
            DictionaryPropertySetDefinitions propertySetDictionary = new DictionaryPropertySetDefinitions(db);
            ObjectId dictId = propertySetDictionary.DictionaryId;
            Autodesk.AutoCAD.DatabaseServices.
            DBDictionary dictDefinitions = trans.GetObject(dictId, OpenMode.ForRead) as DBDictionary;

            foreach (DBDictionaryEntry entry in dictDefinitions)
            {
                if (setName == entry.Key)
                {
                    ObjectId propId = entry.Value;
                    int nId = 0;
                    ObjectId existId = PropertyDataServices.PropertyExists(db, fullPropertyName, out nId);

                    result = PropertyDataServices.GetPropertyValueUnitExt(obj, blockReferencePath, existId, nId);
                    break;
                }
            }

        }
        catch (System.Exception e)
        {
            ed.WriteMessage(e.Message);
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }

        if (null != result)
            return result.Value;
        else
            return null;
    }
    #endregion 

    #region ShowPropertyData
    /// <summary>
    /// 
    /// </summary>
    /// <param name="objId">The object id.</param>
    /// <param name="blockReferencePath">The block reference path.</param>
    private void ShowPropertyData(ObjectId objId, ObjectIdCollection blockReferencePath, string propertySetName, string propertyName)
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        Database db = Application.DocumentManager.MdiActiveDocument.Database;

        DBTransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        try
        {
            Autodesk.AutoCAD.DatabaseServices.DBObject dbObj = trans.GetObject(objId, OpenMode.ForWrite) as Autodesk.AutoCAD.DatabaseServices.DBObject;

            Object ret = GetScheduleDataProperty(dbObj, blockReferencePath, propertySetName, propertyName);
            string retString = PropertyDataServices.DataToString(ret);
            ed.WriteMessage("Found Property: " + propertyName + " = " + retString + "\n");
        }
        catch (System.Exception e)
        {
            ed.WriteMessage(e.Message);
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
    }
    #endregion

    #endregion ImplementationMethods

}
