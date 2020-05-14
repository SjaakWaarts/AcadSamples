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
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Colors;
using BlockReference = Autodesk.AutoCAD.DatabaseServices.BlockReference;

#endregion

#region DisplaySample

[assembly: ExtensionApplication(typeof(DisplayPropertiesSample))]
[assembly: CommandClass(typeof(DisplayPropertiesSample))]

public class DisplayPropertiesSample : IExtensionApplication
{
    #region IExtensionApplication
    /// <summary>
    /// Initialization.
    /// </summary>
    public void Initialize()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        ed.WriteMessage("Loading Display Properties Sample...\r\n");
        ed.WriteMessage("Input \"DisplayPropertiesSample\" to run the sample..." + "\n");
    }

    /// <summary>
    /// Termination.
    /// </summary>
    public void Terminate()
    {
    }
    #endregion

    #region CommandMethod
    [Autodesk.AutoCAD.Runtime.CommandMethod("AecDisplayPropertiesSampleMgd", "DisplayPropertiesSample", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void SampleEntry()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        PromptKeywordOptions pso = new PromptKeywordOptions("Select the sample for:");
        pso.Keywords.Add("override display properties for Material");
        pso.Keywords.Add("override display properties for a Wall");
        pso.Keywords.Add("display block Attributes");
        PromptResult pr = ed.GetKeywords(pso);

        if (pr.Status == PromptStatus.OK)
        {
            if (pr.StringResult == "Material")
                DisplayPropertiesMaterialSample();
            if (pr.StringResult == "Wall")
                DisplayPropertiesWallSample();
            if (pr.StringResult == "Attributes")
                DisplayBlockAttributesSample();
        }
    }

    /// <summary>
    /// Create a material style and override the display properties of this style.
    /// Use Stylemanager to see the overrided display properties.
    /// </summary>
    public void DisplayPropertiesMaterialSample()
    {
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("====================================================");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nDisplay Properties for Material Sample:\n");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("====================================================\n");

        Database db = Application.DocumentManager.MdiActiveDocument.Database;

        //MaterialUtility.
        TransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        Autodesk.Aec.DatabaseServices.Dictionary dict = new DictionaryMaterialDefinition(db);

        MaterialDefinition materialDefinition = null;
        try
        {
            string name = "ACADotNetMaterialDefination";
            string description = "Material def created through .NET API";

            if (!dict.Has(name, trans))
            {
                materialDefinition = dict.NewEntry() as MaterialDefinition;
                materialDefinition.Description = description;
                dict.AddNewRecord(name, materialDefinition);
                trans.AddNewlyCreatedDBObject(materialDefinition, true);
            }
            else
                materialDefinition = trans.GetObject(dict.GetAt(name), OpenMode.ForWrite) as MaterialDefinition;




            DisplayPropertiesMaterial materialProperties = new DisplayPropertiesMaterial();

            materialProperties.SubSetDatabaseDefaults(db);
            materialProperties.SetToStandard(db);

            ObjectId materialId = AddMaterial("wood");
            materialProperties.SurfaceRenderingMaterialId = materialId;
            materialProperties.SectionRenderingMaterialId = materialId;
            materialProperties.SectionedBodyRenderingMaterialId = materialId;
            DisplayComponentHatch displayComponentHatch = new DisplayComponentHatch();

            materialProperties.SurfaceHatch.HatchType = HatchType.UserDefined;
            materialProperties.SurfaceHatch.PatternName = "NETAPITESTPatternName";
            materialProperties.SurfaceHatch.Angle = 90.0;

            ObjectId dispMaterialId = Override.AddToOverrideExtensionDictionaryAndClose(materialDefinition, materialProperties);

            DisplayRepresentationManager displayManager = new DisplayRepresentationManager(db);
            ObjectIdCollection ids = displayManager.GetDisplayRepresentationIdsFromCurrentViewport(RXObject.GetClass(typeof(MaterialDefinition)));

            ObjectId idActiveDispRep = ids[0];

            OverrideDisplayProperties overrideProperties = new OverrideDisplayProperties();
            overrideProperties.ViewId = idActiveDispRep;
            overrideProperties.DisplayPropertyId = dispMaterialId;
            materialDefinition.Overrides.Add(overrideProperties);

            trans.Commit();
            ed.WriteMessage("\n" +
            "One new style for material \"ACADotNetMaterialDefination\"is created and the display properties are overrided.\n" +
            "Use StyleManager to see the settings."
            + "\n");

        }
        catch (System.Exception e)
        {
            trans.Abort();
            ed.WriteMessage("\n" + e.Message + "\n");
            return;
        }
    }

    /// <summary>
    /// Let user select a wall and override one display properties of this wall.
    /// User can see the different display of this wall.
    /// </summary>
    public void DisplayPropertiesWallSample()
    {
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("====================================================");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nDisplay Properties Wall Sample:\n");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("====================================================\n");

        Database db = Application.DocumentManager.MdiActiveDocument.Database;

        //MaterialUtility.
        TransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        try
        {
            PromptEntityOptions optEnt = new PromptEntityOptions("Select an AEC Wall entity");
            optEnt.SetRejectMessage("Selected entity is NOT an AEC Wall entity, try again...");
            optEnt.AddAllowedClass(typeof(Autodesk.Aec.Arch.DatabaseServices.Wall), false);  // Geo is the base class of AEC entities. 

            PromptEntityResult resEnt = ed.GetEntity(optEnt);
            if (resEnt.Status != PromptStatus.OK)
            {
                throw new System.Exception("Selection error - aborting");
            }

            Wall pickedWall = trans.GetObject(resEnt.ObjectId, OpenMode.ForWrite) as Wall;

            DisplayRepresentationManager displayManager = new DisplayRepresentationManager(db);
            ObjectIdCollection ids = displayManager.GetDisplayRepresentationIdsFromCurrentViewport(RXClass.GetClass(typeof(Wall)));

            ObjectId activeDisplayRepId = ids[0];

            DisplayPropertiesWallPlan wallProperties = new DisplayPropertiesWallPlan();

            wallProperties.SubSetDatabaseDefaults(db);
            wallProperties.SetToStandard(db);
            System.Collections.Specialized.StringCollection sc = new System.Collections.Specialized.StringCollection();

            DisplayComponent[] components = wallProperties.GetDisplayComponents(out sc);
            int index =-1;
            for (int i = 0; i < sc.Count; i++)
            {
                string componentName = sc[i];
                //Here, we override the display properties for "shrink wrap" and display it on the screen.
                if (componentName == "Shrink Wrap")
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
                throw new System.Exception("Lack of display component.");

            DisplayComponentEntity component = components[index] as DisplayComponentEntity;
            component.IsApplicable = true;
            component.IsVisible = true;                
            component.ByMaterial = false;
            component.ColorIndex = 30;
            component.LinetypeScale = 2;
            component.LineWeight = LineWeight.LineWeight070;
             
            ObjectId dispWallId = Override.AddToOverrideExtensionDictionaryAndClose(pickedWall, wallProperties);
            OverrideDisplayProperties overrideProperties = new OverrideDisplayProperties();
            overrideProperties.ViewId = activeDisplayRepId;
            overrideProperties.DisplayPropertyId = dispWallId;
            pickedWall.Overrides.Add(overrideProperties);

            trans.Commit();

        }
        catch (System.Exception e)
        {
            trans.Abort();
            ed.WriteMessage("\n" + e.Message + "\n");
        }
    }

    public void DisplayBlockAttributesSample()
    {
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("====================================================");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nDisplay Block Attributes:\n");
        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("====================================================\n");

        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        Database db = HostApplicationServices.WorkingDatabase;
        Transaction tr = db.TransactionManager.StartTransaction();

        // Start the transaction
        try
        {
            // Build a filter list so that only
            // block references are selected

            TypedValue[] filList = new TypedValue[1] { new TypedValue((int)DxfCode.Start, "INSERT") };

            SelectionFilter filter = new SelectionFilter(filList);
            PromptSelectionOptions opts = new PromptSelectionOptions();
            opts.MessageForAdding = "Select block references: ";

            PromptSelectionResult res = ed.GetSelection(opts, filter);
            if (res.Status != PromptStatus.OK)
                return;

            SelectionSet selSet = res.Value;
            ObjectId[] idArray = selSet.GetObjectIds();

            foreach (ObjectId blkId in idArray) {
                BlockReference blkRef = (BlockReference)tr.GetObject(blkId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);
                btr.Dispose();

                AttributeCollection attCol = blkRef.AttributeCollection;
                foreach (ObjectId attId in attCol) {
                    AttributeReference attRef = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);
                }
            }
            tr.Commit();
        }
        catch (Autodesk.AutoCAD.Runtime.Exception ex)
        {
            ed.WriteMessage(("Exception: " + ex.Message));
        }
        finally
        {
            tr.Dispose();
        }
    }



    #endregion


    #region privite Utilities

    private static ObjectId AddMaterial(string nameWood)
    {

        //
        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        TransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        ObjectId materialId = ObjectId.Null;
        try
        {

            DBDictionary materialDict;

            ObjectId materialDictId = db.MaterialDictionaryId;

            materialDict = trans.GetObject(materialDictId, OpenMode.ForWrite) as DBDictionary;

            Material material = new Material();
            material.Name = nameWood;

            WoodTexture woodTexture = new WoodTexture();
            MaterialColor matColor, woodColor1, woodColor2;

            EntityColor entColor = new EntityColor(128, 0, 128);
            matColor = new MaterialColor(Method.Override, 1.0, entColor);
            EntityColor entColor2 = new EntityColor(128, 128, 0);
            woodColor1 = new MaterialColor(Method.Override, 1.0, entColor2);
            EntityColor entColor3 = new EntityColor(0, 128, 128);
            woodColor2 = new MaterialColor(Method.Override, 1.0, entColor3);

            woodTexture.Color1 = woodColor1;
            woodTexture.Color2 = woodColor2;
            MaterialMap materialMap = new MaterialMap(Source.Procedural, woodTexture, 0, null);

            material.Diffuse = new MaterialDiffuseComponent(matColor, materialMap);

            MaterialColor inheritColor = new MaterialColor(Method.Inherit, 1.0, entColor);
            material.Ambient = matColor;
            material.Specular = new MaterialSpecularComponent(matColor, new MaterialMap(), 0.5);
            material.Mode = Mode.Realistic;

            materialDict.SetAt(nameWood, material);
            materialId = material.ObjectId;
            trans.Commit();
        }
        catch (System.Exception e)
        {
            ed.WriteMessage(e.Message);
            trans.Abort();
        }
        return materialId;
    }
    
    #endregion
}


#endregion