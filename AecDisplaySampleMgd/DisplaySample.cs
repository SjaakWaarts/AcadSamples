#region Header
//      .NET Sample
//
//      Copyright (c) 2006 by Autodesk, Inc.
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


using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;


using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

using Autodesk.Aec.DatabaseServices;
using Autodesk.Aec.Arch.DatabaseServices;
using Autodesk.Aec.Geometry;

using TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;
using ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;
using ObjectIdCollection = Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection;

#endregion

#region DisplaySample

[assembly: ExtensionApplication(typeof(DisplaySample))]
[assembly: CommandClass(typeof(DisplaySample))]

public class DisplaySample : IExtensionApplication
{
    static int m_i = 0;

    #region IExtensionApplication
    /// <summary>
    /// Initialization.
    /// </summary>
    public void Initialize()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        ed.WriteMessage("Loading Display System Sample...\r\n");
        ed.WriteMessage("Input \"DisplaySample\" to run the sample..." +"\n");
    }

    /// <summary>
    /// Termination.
    /// </summary>
    public void Terminate()
    {
    }
    #endregion

    #region CommandMethod
    /// <summary>
    /// Command implementation.
    /// </summary>
    [Autodesk.AutoCAD.Runtime.CommandMethod("AecDisplaySampleMgd", "DisplaySample", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void ShowSample()
    {
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("====================================================");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nDisplay System Sample:\n"); 
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("====================================================\n");

        DisplayConfigs();

        ListDisplaySets();

        DisplayReps();

        DisplayProps();

        DisplayPropsMaterial();
    }
    #endregion

    ////////////////////////////
    //// Display Configuration
    ////////////////////////////

    private void DisplayConfigs()
    {

        //' access the display system from the database object

        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        Transaction trans = db.TransactionManager.StartTransaction();

        try
        {
            // Access the display system from the document object
            DictionaryDisplayConfiguration dictDisplayConfigs = new DictionaryDisplayConfiguration(db);

            // The total number of display configurations registered. 
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("There are " + dictDisplayConfigs.Records.Count + " display configs:\n");

            // List them
            foreach (ObjectId dcId in dictDisplayConfigs.Records)
            {
                DisplayConfiguration dc = trans.GetObject(dcId, OpenMode.ForWrite) as DisplayConfiguration;
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Name : " + dc.Name + "\n" +
                    "Description :  " + dc.Description + "\n");
            }

            // Add a new config with the name "MyConfig" with a description in it. 
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nLet's add a new DisplayConfiguration:\n");
            DisplayConfiguration displayconfig = new DisplayConfiguration();
            displayconfig.Description = "This is MyConfig";
            if (!dictDisplayConfigs.Has("MyConfig", trans))
            {
                dictDisplayConfigs.AddNewRecord("MyConfig", displayconfig);
                trans.AddNewlyCreatedDBObject(displayconfig, true);
            }

            // Add another one "FORUSE1"
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(displayconfig.Name + " : " + displayconfig.Description + "\n");
            DisplayConfiguration displayconfig1 = new DisplayConfiguration();
            displayconfig1.Description = "This is for use";
            if (!dictDisplayConfigs.Has("FORUSE1", trans))
            {
                dictDisplayConfigs.AddNewRecord("FORUSE1", displayconfig1);
                trans.AddNewlyCreatedDBObject(displayconfig1, true);
            }

            //    rename MyConfig to a new name MyNewConfigureXxx
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nLet's rename this new DisplayConfiguration:\n");
            string newName = GetNewName();
            dictDisplayConfigs.Rename("MyConfig", newName, trans);
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("After Renamed:\nName: " + displayconfig.Name + "\nDescription: " + displayconfig.Description + "\n");


            //    report the total number of display config again.
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("There are " + dictDisplayConfigs.Records.Count + " display configurations now.\n");

            //    the ObjectId of the new disp config? 
            ObjectId nid = dictDisplayConfigs.GetAt(newName);
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("The current ObjectId for new display configuration is:" + nid.ToString() + "\n");

            //    the current active config? 
            DisplayRepresentationManager drm = new DisplayRepresentationManager(db);
            ObjectId currentDisConfId = drm.DisplayConfigurationIdForCurrentViewport;
            DisplayConfiguration currentDisConf = trans.GetObject(currentDisConfId, OpenMode.ForRead) as DisplayConfiguration;
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("The Current DisplayConfiguration is " + currentDisConf.Name + "\n");
            ListDisplayConfig(currentDisConf);

            //    create a new disp config with more detailed information. 
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Let's create a new DisplayConfiguration for detail:\n");
            DisplayConfiguration newDc = CreateDisplayConfig();

            //    print it out
            ListDisplayConfig(newDc);

            trans.Commit();
        }
        catch (System.Exception e)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Exception: " + e.Message + "\n");
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
    }

    //  create a display config with a little more information filled. 
    private DisplayConfiguration CreateDisplayConfig()
    {
        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        Transaction trans = db.TransactionManager.StartTransaction();

        DisplayRepresentationManager drm = new DisplayRepresentationManager(db);
        ObjectId currentDisConfId = drm.DisplayConfigurationIdForCurrentViewport;
        DisplayConfiguration currentDisConf = trans.GetObject(currentDisConfId, OpenMode.ForRead) as DisplayConfiguration;

        DisplayConfiguration dc = new DisplayConfiguration();
        try
        {

            DisplaySet ds = new DisplaySet();
            ds.SetToStandard(db);
            ds.SubSetDatabaseDefaults(db);

            //  top
            dc.ViewDependentCombinations.Add(new ViewDependentCombination(new Vector3d(0, 0, 1), DictionaryDisplaySet.GetStandardPlanId(db)));
            //  for bottom. No DictionaryDisplaySet.GetStandardXxxId for Plan Diagnostic, so get it from the name. 
            ObjectId id = DictionaryDisplaySet.GetStandardSet("Plan Diagnostic", db);
            if (id != ObjectId.Null ) 
                dc.ViewDependentCombinations.Add(new ViewDependentCombination(new Vector3d(0, 0, -1), id));
            else  // if not found, let's use Reflected. 
                dc.ViewDependentCombinations.Add(new ViewDependentCombination(new Vector3d(0, 0, -1), DictionaryDisplaySet.GetStandardReflectedId(db)));  //buttom

            dc.ViewDependentCombinations.Add(new ViewDependentCombination(new Vector3d(-1, 0, 0), DictionaryDisplaySet.GetStandardSectionElevId(db)));// left
            dc.ViewDependentCombinations.Add(new ViewDependentCombination(new Vector3d(1, 0, 0), DictionaryDisplaySet.GetStandardSectionElevId(db)));// right
            dc.ViewDependentCombinations.Add(new ViewDependentCombination(new Vector3d(0, -1, 0), DictionaryDisplaySet.GetStandardSectionElevId(db)));//  front
            dc.ViewDependentCombinations.Add(new ViewDependentCombination(new Vector3d(0, 1, 0), DictionaryDisplaySet.GetStandardSectionElevId(db)));// back

            //  try adding DefaultViewDependentViewSet here. 
            dc.DefaultViewDependentViewSet = DictionaryDisplaySet.GetStandardModelId(db);

            DictionaryDisplayConfiguration dictDisplayConfigs = new DictionaryDisplayConfiguration(db);
            string newName = GetNewName();
            dictDisplayConfigs.AddNewRecord(newName, dc);
            trans.AddNewlyCreatedDBObject(dc, true);
            trans.Commit();
        }
        catch (System.Exception)
        {
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
        return dc;
    }

    //  show the information of the given display configuration. 
    private void ListDisplayConfig(DisplayConfiguration dc)
    {
        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        Transaction trans = db.TransactionManager.StartTransaction();

        try
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("=======================================\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Name: " + dc.Name + "\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Description: " + dc.Description + "\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Cut Plane Display Above: " + dc.CutPlaneAboveRange + "\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Cut Plane Display Below: " + dc.CutPlaneBelowRange + "\n");

            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Use View Port View Direction: " + dc.UseViewportViewDirection.ToString() + "\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Is View Dependent: " + dc.IsViewDependent.ToString() + "\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Hard View Direction: " + dc.HardViewDirection.ToString() + "\n");


            Vector3d v;
            DisplaySet ds;
            foreach (ViewDependentCombination vd in dc.ViewDependentCombinations)
            {
                v = vd.Direction;

                ds = trans.GetObject(vd.SetId, OpenMode.ForRead) as DisplaySet;
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(v.ToString() + " " + ds.Name + "\n");
            }

            // added.  Default view set  
            ds = trans.GetObject(dc.DefaultViewDependentViewSet, OpenMode.ForRead) as DisplaySet;
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Default View Dependent View Set: " + ds.Name +"\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("=======================================\n");
            trans.Commit();
        }
        catch (System.Exception )
        {
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
    }

    ////////////////////////////
    //// Display Rep
    ////
    //// list all the display reps for MassElem and MassGroup for the current display config. 
    ////////////////////////////

    private void DisplayReps()
    {
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n====================================================\n");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Get All the AecDbMassElem/Massgroup Display Reps for the current display config\n");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("====================================================\n");

        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        Transaction trans = db.TransactionManager.StartTransaction();
        Autodesk.AutoCAD.EditorInput.Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        DisplaySet ds = new DisplaySet();
        ObjectIdCollection ids = DisplayRepresentationManager.GetActiveDisplayRepresentationSets(db);
        ed.WriteMessage("# of active display sets = " + ids.Count+"\n");

        DisplaySet acds = new DisplaySet();
        try
        {
            //  loop through the active disp sets (i.e., disp sets of active dis confid.) 
            foreach (ObjectId id in ids)
            {
                acds = trans.GetObject(id, OpenMode.ForRead) as DisplaySet;
                foreach (ObjectId drid in acds.DisplayRepresentationIds)
                {
                    DisplayRepresentation dr = trans.GetObject(drid, OpenMode.ForRead) as DisplayRepresentation;
                    dr = trans.GetObject(drid, OpenMode.ForRead) as DisplayRepresentation;
                    RXClass rc = dr.WorksWith;
                    if (rc.Name == "AecDbMassElem" || rc.Name == "AecDbMassGroup")
                    {
                        if (ds.DisplayRepresentationIds.Contains(drid))
                            continue;
                        else
                        {
                            ds.DisplayRepresentationIds.Add(drid);
                            ListDisplayRep(dr);
                        }
                    }

                }
            }
            trans.Commit();
        }
        catch (System.Exception)
        {
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
        return;
    }

    //  added. list all the disp set registered. 
    private void ListDisplaySets()
    {
        Autodesk.AutoCAD.EditorInput.Editor ed  = Application.DocumentManager.MdiActiveDocument.Editor;
        ed.WriteMessage("===================================\n");
        ed.WriteMessage("  Currently Registerd Display Set  \n");
        ed.WriteMessage("===================================\n");

        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        Transaction trans = db.TransactionManager.StartTransaction();

        // get the dictionary. 
        DictionaryDisplaySet dictDispSet = new DictionaryDisplaySet(db, false);
        ed.WriteMessage("Current # of display set = " + dictDispSet.Records.Count.ToString()+"\n");

        try
        {
            // loop throught it and print out the name. 
            foreach (ObjectId id in dictDispSet.Records)
            {
                DisplaySet dispSet = trans.GetObject(id, OpenMode.ForRead) as DisplaySet;
                ListDisplaySet(dispSet);
            }
        }
        catch (System.Exception)
        { trans.Abort(); }
        finally
        { trans.Dispose(); }
    }

    private void ListDisplaySet(DisplaySet ds)
    {
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("DisplaySet Name:" + ds.Name + "\n");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("DisplaySet Description:" + ds.Description + "\n");

        return;
    }

    private void ListDisplayRep(DisplayRepresentation dr)
    {
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("DisplayRep Name: " + dr.Name + "\n");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("DisplayRep Work With: " + dr.WorksWith.Name + "\n");

        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("DisplayRep Display Name: " + dr.DisplayName+"\n");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("DisplayRep Display Representation Name: " + dr.DisplayRepresentationName + "\n");

        return;
    }

    ////////////////////////////
    //// Display Props
    //// list all the disp reps for the door for the current view port 
    ////////////////////////////

    private void DisplayProps()
    {
        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        TransactionManager tm = db.TransactionManager;
        Transaction trans = db.TransactionManager.StartTransaction();
        BlockTable bt = (BlockTable)tm.GetObject(db.BlockTableId, OpenMode.ForRead, false);
        BlockTableRecord btr = (BlockTableRecord)tm.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false);
        DictionaryDisplaySet dds = new DictionaryDisplaySet(db, false);
        try
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityOptions optEnt = new PromptEntityOptions("Select an AEC door entity");
            optEnt.SetRejectMessage("Selected entity is NOT an AEC door entity, try again...");
            optEnt.AddAllowedClass(typeof(Autodesk.Aec.Arch.DatabaseServices.Door), false);  // Geo is the base class of AEC entities. 

            PromptEntityResult resEnt = ed.GetEntity(optEnt);
            if (resEnt.Status != PromptStatus.OK)
            {
                ed.WriteMessage("Selection error - aborting" + "\n");
                return;
            }

            Autodesk.Aec.Arch.DatabaseServices.Door door = trans.GetObject(resEnt.ObjectId, OpenMode.ForRead) as Door;
            ed.WriteMessage("You have selected an " + door.GetRXClass().Name + "\n");

            //  get its style
            Autodesk.Aec.Arch.DatabaseServices.DoorStyle doorstyle = trans.GetObject(door.StyleId, OpenMode.ForRead) as DoorStyle;

            ObjectId doorId = door.ObjectId;
            DisplayRepresentationManager drm = new DisplayRepresentationManager(db);

            ObjectIdCollection ids = drm.GetDisplayRepresentationIdsFromCurrentViewport(door.GetRXClass());
            DisplayRepresentation dr = null;
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("=======================================\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Get all the Display Representations work with the door for current view port\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("=======================================\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("door disp rep count = " + ids.Count + "\n");

            foreach (ObjectId id in ids)
            {
                dr = trans.GetObject(id, OpenMode.ForRead) as DisplayRepresentation;

                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Display Representation Name: " + dr.Name + " Display Name: " + dr.DisplayName + "\n");

                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Display Representation Name: " + dr.Name + "Display Name" + dr.DisplayName + "\n");
                DisplayProperties dps = null;
                RXClass rcProps = dr.DisplayPropertiesClass;
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Display Props Class: " + rcProps.Name + "\n");

                //  find override at instance level. 
                ObjectId overId = dr.FindDisplayPropertiesOverride(door);
                if (overId != ObjectId.Null)
                {
                    dps = trans.GetObject(overId, OpenMode.ForRead) as DisplayProperties;
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Element override Display Props Name: " + dps.Name + "Display Name" + dps.DisplayName + "\n");
                    continue;
                }
                ObjectId overstyleId = dr.FindDisplayPropertiesOverride(doorstyle);
                if (overstyleId != ObjectId.Null)
                {
                    dps = trans.GetObject(overstyleId, OpenMode.ForRead) as DisplayProperties;
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Style override Style Display Props Name: " + dps.Name + "Display Name" + dps.DisplayName + "\n");
                    continue;
                }
                ObjectId defaultDpsId = dr.DefaultDisplayPropertiesId;
                if (!defaultDpsId.IsNull)
                {
                    dps = trans.GetObject(defaultDpsId, OpenMode.ForRead) as DisplayProperties;
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Default Display Props Name: " + dps.Name + "Display Name" + dps.DisplayName + "\n");
                }
            }
            trans.Commit();
        }
        catch (System.Exception)
        {
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
        return;
    }

    ////////////////////////////
    //// MATERIALS
    //////////////////////////// 

    private void DisplayPropsMaterial()
    {
        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        TransactionManager tm = db.TransactionManager;
        Transaction trans = db.TransactionManager.StartTransaction();

        try
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("=======================================\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Get all the Display Properties and the Display components works with AecDbMaterialDef in ZAxis view\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("=======================================\n");

            DisplayRepresentationManager drm = new DisplayRepresentationManager(db);
            Viewport vport = new Viewport();
            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false);
            btr.AppendEntity(vport);
            trans.AddNewlyCreatedDBObject(vport, true);
            ObjectId viewportId = vport.ObjectId;

            Vector3d viewDirection = Vector3d.ZAxis;
            ObjectId setId = drm.DisplayRepresentationSet(viewportId, viewDirection);
            DisplaySet ds = trans.GetObject(setId, OpenMode.ForWrite) as DisplaySet;
            DisplayRepresentationIdCollection repIds = ds.DisplayRepresentationIds;
            DisplayRepresentation dr = null;
            foreach (ObjectId repId in repIds)
            {
                dr = trans.GetObject(repId, OpenMode.ForRead) as DisplayRepresentation;
                if (dr.WorksWith.Name == "AecDbMaterialDef")
                {
                    ObjectId dpId = dr.DefaultDisplayPropertiesId;
                    DisplayProperties dp = trans.GetObject(dpId, OpenMode.ForWrite) as DisplayProperties;
                    ListDisplayProperties(dp);
                }
            }
        }
        catch (System.Exception e)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.Message + "\n");
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
        return;
    }

    private void ListDisplayProperties(DisplayProperties dp)
    {
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("List Display Properties: \nName:" + dp.Name + "\n");


        System.Collections.Specialized.StringCollection sc = new System.Collections.Specialized.StringCollection();

        DisplayComponent[] components = dp.GetDisplayComponents(out sc);
        if (components == null || components.Length == 0)
            return;
        //foreach (DisplayComponent obj in components)
        for (int i = 0; i < components.Length; i++)
        {
            DisplayComponent obj = components[i];
            string name = sc[i];
            DisplayComponent dc = (DisplayComponent)obj;
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("List Display Components: " + name + "\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Is Applicable? : " + dc.IsApplicable.ToString() + "\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Is Inherited?: " + dc.IsInherited.ToString() + "\n");

            if (dc is DisplayComponentEntity)
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("It is a Display Component Entity. \n");
            if (dc is DisplayComponentHatch)
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("It is a Display Component Hatch. \n");
        }
    }

    private static string GetNewName()
    {
        string newString = "MyNewConfigure" + m_i.ToString();
        m_i++;
        return newString;
    }

}

#endregion
