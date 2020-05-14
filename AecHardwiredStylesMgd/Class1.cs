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

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;

using Autodesk.Aec.DatabaseServices;
using Autodesk.Aec.Arch.DatabaseServices;
using Autodesk.Aec.Structural.DatabaseServices;
using Autodesk.Aec.Geometry;

using TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;
using ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;
using ObjectIdCollection = Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection;

#endregion

[assembly: ExtensionApplication(typeof(Class1))]
[assembly: CommandClass(typeof(Class1))]

public class Class1 : IExtensionApplication
{
	#region IExtensionApplication
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Initialize()
	{
		Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        ed.WriteMessage("Loading AecHardwiredStylesMgd...\r\n");
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
	[Autodesk.AutoCAD.Runtime.CommandMethod("AecHardwiredStylesMgd", "Hardwired", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
    public void Hardwired()
	{
		Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        PromptKeywordOptions options = new PromptKeywordOptions("Option [Door/Member/Roofslab/SLab/STair/WAll/WIndow/All]",
                                                                "Door Member Roofslab SLab STair WAll WIndow All");
        PromptResult res1 = ed.GetKeywords(options);
		String opt = res1.StringResult;

        if (opt == "Door" || opt == "All")
            CreateDoorStyles();
        if (opt == "Member" || opt == "All")
            CreateMemberStyles();
        if (opt == "Roofslab" || opt == "All")
            CreateRoofSlabStyles();
        if (opt == "SLab" || opt == "All")
            CreateSlabStyles();
        if (opt == "STair" || opt == "All")
            CreateStairStyles();
        if (opt == "WAll" || opt == "All")
            CreateWallStyles();
        if (opt == "WIndow" || opt == "All")
            CreateWindowStyles();
    }
	#endregion

    #region CreateDoorStyles
    /// <summary>
    /// Creates the hardwired door styles.
    /// </summary>
    private void CreateDoorStyles()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        Database db = HostApplicationServices.WorkingDatabase;
        TransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        DictionaryDoorStyle dict = new DictionaryDoorStyle(db);

        try
        {
            String stylename = "InteriorDouble";
            String description = "Interior Double, 2\" Thick, Wood Door Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                DoorStyle style = new DoorStyle();
                style.Description = description;

                style.StopWidth = 2.0;
                style.StopDepth = 2.5;
                style.FrameWidth = 2.0;
                style.FrameDepth = 7.0;
                style.DoorThickness = 2.0;
                style.DoorType = DoorType.Double;
                style.DoorShape = OpenShapeType.Rectangular;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                StandardSizeOpeningList list = new StandardSizeOpeningList();
                StandardSizeOpening opening = new StandardSizeOpening();
                opening.Set(32.0, 80.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(34.0, 80.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(36.0, 80.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(32.0, 84.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(34.0, 84.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(36.0, 84.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(32.0, 88.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(34.0, 88.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(36.0, 88.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(42.0, 80.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(50.0, 88.0, 0.0, 0.0);
                list.Openings.Add(opening);

                // create the extension dictionary
                style.CreateExtensionDictionary();
                ObjectId extDictId = style.ExtensionDictionary;
                DBObject obj = tm.GetObject(extDictId, OpenMode.ForWrite);
                DBDictionary dbDict = obj as DBDictionary;

                DBDictionary standardSizesDict = new DBDictionary();
                dbDict.SetAt(StandardSizeOpeningList.ExtensionDictionaryName, standardSizesDict);
                ObjectId idStandardSizesDict = dbDict.GetAt(StandardSizeOpeningList.ExtensionDictionaryName);

                tm.AddNewlyCreatedDBObject(standardSizesDict, true);

                // get the extension dictionary and add the list
                obj = tm.GetObject(idStandardSizesDict, OpenMode.ForWrite);
                dbDict = obj as DBDictionary;
                dbDict.SetAt(StandardSizeOpeningList.ExtensionDictionaryKeyName, list);

                tm.AddNewlyCreatedDBObject(list, true);
            }

            stylename = "ClosetBifold";
            description = "Closet Bifold, 2\" Thick, Masonite Door Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                DoorStyle style = new DoorStyle();
                style.Description = description;

                style.FrameWidth = 3.0;
                style.FrameDepth = 7.0;
                style.DoorThickness = 2.0;
                style.DoorType = DoorType.Bifold;
                style.DoorShape = OpenShapeType.Rectangular;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                StandardSizeOpeningList list = new StandardSizeOpeningList();
                StandardSizeOpening opening = new StandardSizeOpening();
                opening.Set(28.0, 80.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(36.0, 80.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(48.0, 84.0, 0.0, 0.0);
                list.Openings.Add(opening);

                // create the extension dictionary
                style.CreateExtensionDictionary();
                ObjectId extDictId = style.ExtensionDictionary;
                DBObject obj = tm.GetObject(extDictId, OpenMode.ForWrite);
                DBDictionary dbDict = obj as DBDictionary;

                DBDictionary standardSizesDict = new DBDictionary();
                dbDict.SetAt(StandardSizeOpeningList.ExtensionDictionaryName, standardSizesDict);
                ObjectId idStandardSizesDict = dbDict.GetAt(StandardSizeOpeningList.ExtensionDictionaryName);

                tm.AddNewlyCreatedDBObject(standardSizesDict, true);

                // get the extension dictionary and add the list
                obj = tm.GetObject(idStandardSizesDict, OpenMode.ForWrite);
                dbDict = obj as DBDictionary;
                dbDict.SetAt(StandardSizeOpeningList.ExtensionDictionaryKeyName, list);

                tm.AddNewlyCreatedDBObject(list, true);
            } 
            
            trans.Commit();
        }
        catch
        {
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
    }
    #endregion

    #region CreateMemberStyles
    /// <summary>
    /// Creates the hardwired member styles.
    /// </summary>
    private void CreateMemberStyles()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        Database db = HostApplicationServices.WorkingDatabase;
        TransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        DictionaryMemberStyle dict = new DictionaryMemberStyle(db);
        DictionaryMemberNodeShape shapeDict = new DictionaryMemberNodeShape(db);

        try
        {
            // create the shape definitions

            String shapename = "W12x20";
            ObjectId shapeIdW12x20 = ObjectId.Null;
            
            if (shapeDict.Has(shapename, trans))
                shapeIdW12x20 = shapeDict.GetAt(shapename);
            else
            {
                MemberNodeShape shapeDef = new MemberNodeShape();
                for (int i = 0; i < 3; i++)
                {
                    Curve2d[] curves;
                    Profile profile;
                    IShapeForDesign((DetailLevel)i, out curves, out profile, 12, 0.1, 0.1, 4);

                    if (curves != null)
                    {
                        for (int j = 0; j < curves.Length; j++)
                        {
                            Curve2d curve = curves[j];
                            shapeDef.AddCurve((DetailLevel)i, curve);
                        }
                    }

                    if (profile != null)
                        shapeDef.SetProfile((DetailLevel)i, profile);
                }
                shapeDict.AddNewRecord(shapename, shapeDef);
                tm.AddNewlyCreatedDBObject(shapeDef, true);
                shapeIdW12x20 = shapeDef.ObjectId;
            }

            shapename = "W36x100";
            ObjectId shapeIdW36x100 = ObjectId.Null;

            if (shapeDict.Has(shapename, trans))
                shapeIdW36x100 = shapeDict.GetAt(shapename);
            else
            {
                MemberNodeShape shapeDef = new MemberNodeShape();
                for (int i = 0; i < 3; i++)
                {
                    Curve2d[] curves;
                    Profile profile;
                    IShapeForDesign((DetailLevel)i, out curves, out profile, 36, 0.1, 0.1, 12);

                    if (curves != null)
                    {
                        for (int j = 0; j < curves.Length; j++)
                        {
                            Curve2d curve = curves[j];
                            shapeDef.AddCurve((DetailLevel)i, curve);
                        }
                    }

                    if (profile != null)
                        shapeDef.SetProfile((DetailLevel)i, profile);
                }
                shapeDict.AddNewRecord(shapename, shapeDef);
                tm.AddNewlyCreatedDBObject(shapeDef, true);
                shapeIdW36x100 = shapeDef.ObjectId;
            }

            shapename = "W36x210";
            ObjectId shapeIdW36x210 = ObjectId.Null;

            if (shapeDict.Has(shapename, trans))
                shapeIdW36x100 = shapeDict.GetAt(shapename);
            else
            {
                MemberNodeShape shapeDef = new MemberNodeShape();
                for (int i = 0; i < 3; i++)
                {
                    Curve2d[] curves;
                    Profile profile;
                    IShapeForDesign((DetailLevel)i, out curves, out profile, 36.75, 0.8125, 1.375, 12.125);

                    if (curves != null)
                    {
                        for (int j = 0; j < curves.Length; j++)
                        {
                            Curve2d curve = curves[j];
                            shapeDef.AddCurve((DetailLevel)i, curve);
                        }
                    }

                    if (profile != null)
                        shapeDef.SetProfile((DetailLevel)i, profile);
                }
                shapeDict.AddNewRecord(shapename, shapeDef);
                tm.AddNewlyCreatedDBObject(shapeDef, true);
                shapeIdW36x210 = shapeDef.ObjectId;
            }

            shapename = "W6x15";
            ObjectId shapeIdW6x15 = ObjectId.Null;

            if (shapeDict.Has(shapename, trans))
                shapeIdW6x15 = shapeDict.GetAt(shapename);
            else
            {
                MemberNodeShape shapeDef = new MemberNodeShape();
                for (int i = 0; i < 3; i++)
                {
                    Curve2d[] curves;
                    Profile profile;
                    IShapeForDesign((DetailLevel)i, out curves, out profile, 6, 0.25, 0.25, 6);

                    if (curves != null)
                    {
                        for (int j = 0; j < curves.Length; j++)
                        {
                            Curve2d curve = curves[j];
                            shapeDef.AddCurve((DetailLevel)i, curve);
                        }
                    }

                    if (profile != null)
                        shapeDef.SetProfile((DetailLevel)i, profile);
                }
                shapeDict.AddNewRecord(shapename, shapeDef);
                tm.AddNewlyCreatedDBObject(shapeDef, true);
                shapeIdW6x15 = shapeDef.ObjectId;
            }

            shapename = "WT12x31";
            ObjectId shapeIdWT12x31 = ObjectId.Null;

            if (shapeDict.Has(shapename, trans))
                shapeIdWT12x31 = shapeDict.GetAt(shapename);
            else
            {
                MemberNodeShape shapeDef = new MemberNodeShape();
                for (int i = 0; i < 3; i++)
                {
                    Curve2d[] curves;
                    Profile profile;
                    TShapeForDesign((DetailLevel)i, out curves, out profile, 11.87, 0.43, 0.59, 7.04);

                    if (curves != null)
                    {
                        for (int j = 0; j < curves.Length; j++)
                        {
                            Curve2d curve = curves[j];
                            shapeDef.AddCurve((DetailLevel)i, curve);
                        }
                    }

                    if (profile != null)
                        shapeDef.SetProfile((DetailLevel)i, profile);
                }
                shapeDict.AddNewRecord(shapename, shapeDef);
                tm.AddNewlyCreatedDBObject(shapeDef, true);
                shapeIdWT12x31 = shapeDef.ObjectId;
            }

            shapename = "C6x13";
            ObjectId shapeIdC6x13 = ObjectId.Null;

            if (shapeDict.Has(shapename, trans))
                shapeIdC6x13 = shapeDict.GetAt(shapename);
            else
            {
                MemberNodeShape shapeDef = new MemberNodeShape();
                for (int i = 0; i < 3; i++)
                {
                    Curve2d[] curves;
                    Profile profile;
                    AmericanStandardChannelsForDesign((DetailLevel)i, out curves, out profile, 6, 0.4375, 0.3125, 2.18);

                    if (curves != null)
                    {
                        for (int j = 0; j < curves.Length; j++)
                        {
                            Curve2d curve = curves[j];
                            shapeDef.AddCurve((DetailLevel)i, curve);
                        }
                    }

                    if (profile != null)
                        shapeDef.SetProfile((DetailLevel)i, profile);
                }
                shapeDict.AddNewRecord(shapename, shapeDef);
                tm.AddNewlyCreatedDBObject(shapeDef, true);
                shapeIdC6x13 = shapeDef.ObjectId;
            }

            shapename = "ROD2";
            ObjectId shapeIdROD2 = ObjectId.Null;

            if (shapeDict.Has(shapename, trans))
                shapeIdROD2 = shapeDict.GetAt(shapename);
            else
            {
                MemberNodeShape shapeDef = new MemberNodeShape();
                for (int i = 0; i < 3; i++)
                {
                    Profile profile = Profile.CreateCircle(2, new Vector2d(0, 0));
                    shapeDef.SetProfile((DetailLevel)i, profile);
                }
                shapeDict.AddNewRecord(shapename, shapeDef);
                tm.AddNewlyCreatedDBObject(shapeDef, true);
                shapeIdROD2 = shapeDef.ObjectId;
            }

            shapename = "Tube";
            ObjectId shapeIdTube = ObjectId.Null;

            if (shapeDict.Has(shapename, trans))
                shapeIdTube = shapeDict.GetAt(shapename);
            else
            {
                MemberNodeShape shapeDef = new MemberNodeShape();
                for (int i = 0; i < 3; i++)
                {
                    Profile profile = new Profile();
                    Profile circleShape = Profile.CreateCircle(10.0, new Vector2d(0, 0));
                    profile.AddRingsFrom(circleShape);
                    circleShape = Profile.CreateCircle(9.0, new Vector2d(0, 0));
                    profile.AddRingsFrom(circleShape);
                    profile.Rings[0].IsVoid = false;
                    profile.Rings[1].IsVoid = true;

                    shapeDef.SetProfile((DetailLevel)i, profile);
                }
                shapeDict.AddNewRecord(shapename, shapeDef);
                tm.AddNewlyCreatedDBObject(shapeDef, true);
                shapeIdTube = shapeDef.ObjectId;
            }

            // create the member styles

            String stylename = "Taper";
            String description = "Tapered Member Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");
                
                MemberStyle style = new MemberStyle();
                style.Description = description;

                MemberNodeComponent startNode = new MemberNodeComponent();
                MemberNodeComponent endNode = new MemberNodeComponent();
                startNode.ShapeId = shapeIdW12x20;
                startNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.Start);
                startNode.Rotation = Math.PI / 2; 
                endNode.ShapeId = shapeIdW36x100;
                endNode.ReferenceNodeId = new MemberNodeId(1, RelativeEnd.Start);
                endNode.Rotation = Math.PI / 2;
                MemberComponent comp = new MemberComponent();
                comp.StartNode = startNode;
                comp.EndNode = endNode;
                style.Components.Add(comp);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "W12x20";
            description = "W12x20 Member Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                MemberStyle style = new MemberStyle();
                style.Description = description;

                MemberNodeComponent startNode = new MemberNodeComponent();
                MemberNodeComponent endNode = new MemberNodeComponent();
                startNode.ShapeId = shapeIdW12x20;
                startNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.Start);
                endNode.ShapeId = shapeIdW12x20;
                endNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.End);
                MemberComponent comp = new MemberComponent();
                comp.StartNode = startNode;
                comp.EndNode = endNode;
                style.Components.Add(comp);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "Rod";
            description = "Rebar Member Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                MemberStyle style = new MemberStyle();
                style.Description = description;

                MemberNodeComponent startNode = new MemberNodeComponent();
                startNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.Start);
                startNode.ShapeId = shapeIdROD2;
                MemberComponent comp = new MemberComponent();
                comp.StartNode = startNode;
                style.Components.Add(comp);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "W36x210";
            description = "W36x210 Member Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                MemberStyle style = new MemberStyle();
                style.Description = description;

                MemberNodeComponent startNode = new MemberNodeComponent();
                MemberNodeComponent endNode = new MemberNodeComponent();
                startNode.ShapeId = shapeIdW36x210;
                startNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.Start);
                endNode.ShapeId = shapeIdW36x210;
                endNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.End);
                MemberComponent comp = new MemberComponent();
                comp.StartNode = startNode;
                comp.EndNode = endNode;
                style.Components.Add(comp);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "W6x15";
            description = "W6x15 Member Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                MemberStyle style = new MemberStyle();
                style.Description = description;

                MemberNodeComponent startNode = new MemberNodeComponent();
                MemberNodeComponent endNode = new MemberNodeComponent();
                startNode.ShapeId = shapeIdW6x15;
                startNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.Start);
                endNode.ShapeId = shapeIdW6x15;
                endNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.End);
                MemberComponent comp = new MemberComponent();
                comp.StartNode = startNode;
                comp.EndNode = endNode;
                style.Components.Add(comp);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "WT12x31";
            description = "WT12x31 Member Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                MemberStyle style = new MemberStyle();
                style.Description = description;

                MemberNodeComponent startNode = new MemberNodeComponent();
                MemberNodeComponent endNode = new MemberNodeComponent();
                startNode.ShapeId = shapeIdWT12x31;
                startNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.Start);
                endNode.ShapeId = shapeIdWT12x31;
                endNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.End);
                MemberComponent comp = new MemberComponent();
                comp.StartNode = startNode;
                comp.EndNode = endNode;
                style.Components.Add(comp);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "C6x13";
            description = "C6x13 Member Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                MemberStyle style = new MemberStyle();
                style.Description = description;

                MemberNodeComponent startNode = new MemberNodeComponent();
                MemberNodeComponent endNode = new MemberNodeComponent();
                startNode.ShapeId = shapeIdC6x13;
                startNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.Start);
                endNode.ShapeId = shapeIdC6x13;
                endNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.End);
                MemberComponent comp = new MemberComponent();
                comp.StartNode = startNode;
                comp.EndNode = endNode;
                style.Components.Add(comp);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "Tube10x9";
            description = "Tube Member Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                MemberStyle style = new MemberStyle();
                style.Description = description;

                MemberNodeComponent startNode = new MemberNodeComponent();
                MemberNodeComponent endNode = new MemberNodeComponent();
                startNode.ShapeId = shapeIdTube;
                startNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.Start);
                endNode.ShapeId = shapeIdTube;
                endNode.ReferenceNodeId = new MemberNodeId(0, RelativeEnd.End);
                MemberComponent comp = new MemberComponent();
                comp.StartNode = startNode;
                comp.EndNode = endNode;
                style.Components.Add(comp);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            trans.Commit();
        }
        catch
        {
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
    }
    #endregion

    #region CreateRoofSlabStyles
    /// <summary>
    /// Creates the hardwired roof slab styles.
    /// </summary>
    private void CreateRoofSlabStyles()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        Database db = HostApplicationServices.WorkingDatabase;
        TransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        DictionaryRoofSlabStyle dict = new DictionaryRoofSlabStyle(db);

        try
        {
            String stylename = "Flat Roof";
            String description = "Flat Roof Slab Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                RoofSlabStyle style = new RoofSlabStyle();
                style.Description = description;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                // a. Gravel Ballast
                SlabStyleComponent comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Gravel Ballast";
                comp.Position.SetFixedThickness(2.0);
                comp.Position.Offset.BaseValue = 6.0;
                comp.Position.Offset.OperatorType = InstanceBasedValueOperatorType.Plus;
                comp.Position.Offset.Operand = 0.0;
                comp.Position.Offset.UseInstanceValue = true;
                
                style.Components.Add(comp);
                
                // b. Roofing membrane
                comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Roofing Membrane";
                comp.Position.SetFixedThickness(0.0);
                comp.Position.Offset.BaseValue = 6.0;
                comp.Position.Offset.OperatorType = InstanceBasedValueOperatorType.Plus;
                comp.Position.Offset.Operand = 0.0;
                comp.Position.Offset.UseInstanceValue = true;

                style.Components.Add(comp);

                // c. Insulation
                comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Rigid Insulation";
                comp.Position.SetFixedOffset(6.0);
                comp.Position.Thickness.BaseValue = 0.0;
                comp.Position.Thickness.OperatorType = InstanceBasedValueOperatorType.Plus;
                comp.Position.Thickness.Operand = 0.0;
                comp.Position.Thickness.UseInstanceValue = true;

                style.Components.Add(comp);

                // d. Concrete
                comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Concrete";
                comp.Position.SetFixedOffset(2.0);
                comp.Position.SetFixedThickness(4.0);

                style.Components.Add(comp);

                // e. Metal decking
                comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Metal Decking";
                comp.Position.SetFixedOffset(0.0);
                comp.Position.SetFixedThickness(2.0);

                style.Components.Add(comp);
            }

            stylename = "Wood Frame Floor";
            description = "Sloped Standing Seam Metal Roof Slab Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                RoofSlabStyle style = new RoofSlabStyle();
                style.Description = description;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                // a. Standing Seam Metal Roof Panel
                SlabStyleComponent comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Standing Seam Metal Roof Panel";
                comp.Position.SetFixedOffset(6.0);
                comp.Position.SetFixedThickness(2.0);

                style.Components.Add(comp);

                // b. Insulation
                comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Insulation";
                comp.Position.SetFixedOffset(4.0);
                comp.Position.SetFixedThickness(2.0);

                style.Components.Add(comp);

                //c. Structural Roof Frame
                comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Structural Roof Frame";
                comp.Position.SetFixedOffset(0.0);
                comp.Position.SetFixedThickness(4.0);

                style.Components.Add(comp);
            }
             
            trans.Commit();

        }
        catch
        {
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
    }
    #endregion

    #region CreateSlabStyles
    /// <summary>
    /// Creates the hardwired slab styles.
    /// </summary>
    private void CreateSlabStyles()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        Database db = HostApplicationServices.WorkingDatabase;
        TransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        DictionarySlabStyle dict = new DictionarySlabStyle(db);

        try
        {
            String stylename = "6in Concrete on Grade";
            String description = "6in Concrete on Grade Slab Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                SlabStyle style = new SlabStyle();
                style.Description = description;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                // a. Concrete
                SlabStyleComponent comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Concrete";
                comp.Position.SetFixedOffset(0.0);
                comp.Position.SetFixedThickness(6.0);
                style.Components.Add(comp);

                // b. Polythene sheet
                comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Polyethylene Sheet";
                comp.Position.SetFixedOffset(-0.125);
                comp.Position.SetFixedThickness(0.125);
                style.Components.Add(comp);

                // c. Gravel base
                comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Gravel";
                comp.Position.SetFixedOffset(-4.125);
                comp.Position.SetFixedThickness(4.0);
                style.Components.Add(comp);
            }

            stylename = "Wood Frame Floor";
            description = "Wood Frame Floor Slab Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                SlabStyle style = new SlabStyle();
                style.Description = description;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                // a. Finish Floor
                SlabStyleComponent comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Finish Floor";
                comp.Position.SetFixedThickness(0.5);
                comp.Position.Offset.BaseValue = 3.0/4.0;
                comp.Position.Offset.OperatorType = InstanceBasedValueOperatorType.Plus;
                comp.Position.Offset.Operand = 0.0;
                comp.Position.Offset.UseInstanceValue = true;
                style.Components.Add(comp);

                // b. Plywood subfloor
                comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Plywood Subfloor";
                comp.Position.SetFixedThickness(3.0 / 4.0);
                comp.Position.Offset.BaseValue = 0.0;
                comp.Position.Offset.OperatorType = InstanceBasedValueOperatorType.Plus;
                comp.Position.Offset.Operand = 0.0;
                comp.Position.Offset.UseInstanceValue = true;
                style.Components.Add(comp);

                // c. Floor Joists
                comp = new SlabStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "Floor Joists";
                comp.Position.SetFixedOffset(0.0);
                comp.Position.Thickness.BaseValue = 0.0;
                comp.Position.Thickness.OperatorType = InstanceBasedValueOperatorType.Plus;
                comp.Position.Thickness.Operand = 0.0;
                comp.Position.Thickness.UseInstanceValue = true;
                style.Components.Add(comp);
            }

            trans.Commit();
        }
        catch
        {
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
    }
    #endregion

    #region CreateStairStyles
    /// <summary>
	/// Creates the hardwired stair styles.
	/// </summary>
	private void CreateStairStyles()
	{
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        
        Database db = HostApplicationServices.WorkingDatabase;
		TransactionManager tm = db.TransactionManager;
		Transaction trans = tm.StartTransaction();

        DictionaryStairStyle dict = new DictionaryStairStyle(db);
        
        try
		{
            String stylename = "Wood1";
            String description = "Wood (Saddle) Stair Style";
            
            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                StairStyle style = new StairStyle();
                style.SetToStandard(db);
                style.Description = description;
                style.Flags = style.Flags | StairStyleFlags.HideRiser; // true
                style.Nosing = 2.0;

                StairStringerComponentDefinition stringer = new StairStringerComponentDefinition();
                stringer.SubSetDatabaseDefaults(db);
                stringer.Type = StairStringerType.Saddled;
                stringer.AnchorType = StairStringerAnchorType.AlignLeft;
                stringer.Width = 5.0;
                stringer.WaistAtFlight = 7.0;
                stringer.WaistAtLanding = 7.0;
                stringer.Offset = 2.0;
                style.ComponentDefinitions.Add(stringer);

                stringer = new StairStringerComponentDefinition();
                stringer.SubSetDatabaseDefaults(db);
                stringer.Type = StairStringerType.Saddled;
                stringer.AnchorType = StairStringerAnchorType.AlignRight;
                stringer.Width = 5.0;
                stringer.WaistAtFlight = 7.0;
                stringer.WaistAtLanding = 7.0;
                stringer.Offset = 2.0;
                style.ComponentDefinitions.Add(stringer);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "Wood2";
            description = "Wood (Housed) Stair Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                StairStyle style = new StairStyle();
                style.SetToStandard(db);
                style.Description = description;
                style.Flags = style.Flags | StairStyleFlags.HideRiser; // true

                StairStringerComponentDefinition stringer = new StairStringerComponentDefinition();
                stringer.SubSetDatabaseDefaults(db);
                stringer.Type = StairStringerType.Housed;
                stringer.AnchorType = StairStringerAnchorType.AlignLeft;
                stringer.Width = 5.0;
                stringer.WaistAtFlight = 7.0;
                stringer.WaistAtLanding = 7.0;
                stringer.ThicknessTotalAtFlight = 12.0;
                stringer.ThicknessTotalAtLanding = 12.0;
                style.ComponentDefinitions.Add(stringer);

                stringer = new StairStringerComponentDefinition();
                stringer.SubSetDatabaseDefaults(db);
                stringer.Type = StairStringerType.Housed;
                stringer.AnchorType = StairStringerAnchorType.AlignRight;
                stringer.Width = 5.0;
                stringer.WaistAtFlight = 7.0;
                stringer.WaistAtLanding = 7.0;
                stringer.ThicknessTotalAtFlight = 12.0;
                stringer.ThicknessTotalAtLanding = 12.0;
                style.ComponentDefinitions.Add(stringer);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "Steel1";
            description = "Steel Pan Stair Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                StairStyle style = new StairStyle();
                style.SetToStandard(db);
                style.Description = description;
                style.Flags = style.Flags | StairStyleFlags.HideRiser; // true

                StairStringerComponentDefinition stringer = new StairStringerComponentDefinition();
                stringer.SubSetDatabaseDefaults(db);
                stringer.Type = StairStringerType.Saddled;
                stringer.AnchorType = StairStringerAnchorType.AlignLeft;
                stringer.Width = 5.0;
                stringer.WaistAtFlight = 6.0;
                stringer.WaistAtLanding = 6.0;
                style.ComponentDefinitions.Add(stringer);

                stringer = new StairStringerComponentDefinition();
                stringer.SubSetDatabaseDefaults(db);
                stringer.Type = StairStringerType.Saddled;
                stringer.AnchorType = StairStringerAnchorType.AlignRight;
                stringer.Width = 5.0;
                stringer.WaistAtFlight = 6.0;
                stringer.WaistAtLanding = 6.0;
                style.ComponentDefinitions.Add(stringer);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "Steel2";
            description = "Steel Stair Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                StairStyle style = new StairStyle();
                style.SetToStandard(db);
                style.Description = description;
                style.Flags = style.Flags & ~StairStyleFlags.HideRiser; //false

                StairStringerComponentDefinition stringer = new StairStringerComponentDefinition();
                stringer.SubSetDatabaseDefaults(db);
                stringer.Type = StairStringerType.Saddled;
                stringer.AnchorType = StairStringerAnchorType.AlignLeft;
                stringer.Width = 5.0;
                stringer.WaistAtFlight = 6.0;
                stringer.WaistAtLanding = 6.0;
                style.ComponentDefinitions.Add(stringer);

                stringer = new StairStringerComponentDefinition();
                stringer.SubSetDatabaseDefaults(db);
                stringer.Type = StairStringerType.Saddled;
                stringer.AnchorType = StairStringerAnchorType.AlignRight;
                stringer.Width = 5.0;
                stringer.WaistAtFlight = 6.0;
                stringer.WaistAtLanding = 6.0;
                style.ComponentDefinitions.Add(stringer);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "Concrete";
            description = "Concrete Slab Stair Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                StairStyle style = new StairStyle();
                style.SetToStandard(db);
                style.Description = description;
                style.Flags = style.Flags & ~StairStyleFlags.HideRiser; //false

                StairStringerComponentDefinition stringer = new StairStringerComponentDefinition();
                stringer.SubSetDatabaseDefaults(db);
                stringer.Type = StairStringerType.Slab;
                stringer.AnchorType = StairStringerAnchorType.FullWidth;
                stringer.Width = 1.0;
                stringer.WaistAtFlight = 7.0;
                stringer.WaistAtLanding = 7.0;
                style.ComponentDefinitions.Add(stringer);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            stylename = "Cantilever";
            description = "Double Cantilever Stair Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                StairStyle style = new StairStyle();
                style.SetToStandard(db);
                style.Description = description;
                style.Flags = style.Flags & ~StairStyleFlags.HideRiser; //false

                StairStringerComponentDefinition stringer = new StairStringerComponentDefinition();
                stringer.SubSetDatabaseDefaults(db);
                stringer.Type = StairStringerType.Saddled;
                stringer.AnchorType = StairStringerAnchorType.Center;
                stringer.Width = 10.0;
                stringer.WaistAtFlight = 8.0;
                stringer.WaistAtLanding = 8.0;
                style.ComponentDefinitions.Add(stringer);

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);
            }

            trans.Commit();
		}
		catch
		{
            trans.Abort();
		}
		finally
		{
			trans.Dispose();
		}
	}
	#endregion

    #region CreateWallStyles
    /// <summary>
    /// Creates the hardwired wall styles.
    /// </summary>
    private void CreateWallStyles()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        Database db = HostApplicationServices.WorkingDatabase;
        TransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        DictionaryWallStyle dict = new DictionaryWallStyle(db);

        try
        {
            String stylename = "Cavity";
            String description = "Cavity Wall Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n"); 
                
                WallStyle style = new WallStyle();
                style.Description = description;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                WallStyleComponent comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "facer";
                comp.Position.SetFixedEdgeOffset(-6);
                comp.Position.SetFixedWidth(4.0);
                comp.Priority = 1;
                style.Components.Add(comp);

                comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "facel";
                comp.Position.SetFixedEdgeOffset(2);
                comp.Position.SetFixedWidth(4.0);
                comp.Priority = 8;
                style.Components.Add(comp);
            }

            stylename = "Mason";
            description = "Mason Wall Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");
                
                WallStyle style = new WallStyle();
                style.Description = description;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                WallStyleComponent comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "mainwall";
                comp.Position.SetFixedEdgeOffset(-3);
                comp.Position.SetFixedWidth(3.0);
                comp.Priority = 7;
                style.Components.Add(comp);

                comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "someline";
                comp.Position.SetFixedEdgeOffset(0);
                comp.Position.SetFixedWidth(3.0);
                comp.Priority = 8;
                style.Components.Add(comp);
            }

            stylename = "ExtMason";
            description = "ExtMason Wall Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                WallStyle style = new WallStyle();
                style.Description = description;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                WallStyleComponent comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "mainwall";
                comp.Position.SetFixedEdgeOffset(-3);
                comp.Position.SetFixedWidth(3.0);
                comp.Position.StartElevation.ElevationType = WallElevationType.FromWallBottom;
                comp.Position.EndElevation.ElevationType = WallElevationType.FromWallTop;
                comp.Position.StartElevation.ElevationOffset = 0.0;
                comp.Position.EndElevation.ElevationOffset = 0.0;
                style.Components.Add(comp);

                comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "someline";
                comp.Position.SetFixedEdgeOffset( 0 );
                comp.Position.SetFixedWidth( 3.0 );
                comp.Position.StartElevation.ElevationType = WallElevationType.FromBaseline;
                comp.Position.EndElevation.ElevationType = WallElevationType.FromBaseHeight;
                comp.Position.StartElevation.ElevationOffset = 0.0;
                comp.Position.EndElevation.ElevationOffset = 0.0;
                style.Components.Add(comp);
            }

            stylename = "Connor";
            description = "Connor Wall Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                WallStyle style = new WallStyle();
                style.Description = description;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                WallStyleComponent comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "comp2";
                comp.Position.SetFixedEdgeOffset( -3.5 );
                comp.Position.SetFixedWidth( 7.0 );
                comp.Priority = 3;
                comp.Position.StartElevation.ElevationType = WallElevationType.FromWallBottom;
                comp.Position.EndElevation.ElevationType = WallElevationType.FromBaseline;
                comp.Position.StartElevation.ElevationOffset = 0.0;
                comp.Position.EndElevation.ElevationOffset = 36.0;
                style.Components.Add(comp);

                comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "comp1";
                comp.Position.SetFixedEdgeOffset( -2 );
                comp.Position.SetFixedWidth( 4.0 );
                comp.Priority = 3;
                comp.Position.StartElevation.ElevationType = WallElevationType.FromBaseline;
                comp.Position.EndElevation.ElevationType = WallElevationType.FromWallTop;
                comp.Position.StartElevation.ElevationOffset = 36.0;
                comp.Position.EndElevation.ElevationOffset = 0.0;
                style.Components.Add(comp);
            }

            stylename = "CementBottom";
            description = "CementBottom Wall Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                WallStyle style = new WallStyle();
                style.Description = description;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                WallStyleComponent comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "comp1";
                comp.Position.SetFixedEdgeOffset(-4);
                comp.Position.SetFixedWidth(8.0);
                comp.Position.StartElevation.ElevationType = WallElevationType.FromWallBottom;
                comp.Position.EndElevation.ElevationType = WallElevationType.FromBaseline;
                comp.Position.StartElevation.ElevationOffset = 0.0;
                comp.Position.EndElevation.ElevationOffset = 36.0;
                comp.Priority = 1;
                style.Components.Add(comp);

                comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "comp2";
                comp.Position.SetFixedEdgeOffset(-4);
                comp.Position.SetFixedWidth(2.0);
                comp.Position.StartElevation.ElevationType = WallElevationType.FromBaseline;
                comp.Position.EndElevation.ElevationType = WallElevationType.FromWallTop;
                comp.Position.StartElevation.ElevationOffset = 36.0;
                comp.Position.EndElevation.ElevationOffset = 0.0;
                comp.Priority = 12;
                style.Components.Add(comp);

                comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "comp3";
                comp.Position.SetFixedEdgeOffset(1);
                comp.Position.SetFixedWidth(3.0);
                comp.Position.StartElevation.ElevationType = WallElevationType.FromBaseline;
                comp.Position.EndElevation.ElevationType = WallElevationType.FromBaseHeight;
                comp.Position.StartElevation.ElevationOffset = 36.0;
                comp.Position.EndElevation.ElevationOffset = 0.0;
                comp.Priority = 15;
                style.Components.Add(comp);
            }

            stylename = "SimpleVariable";
            description = "SimpleVariable Wall Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                WallStyle style = new WallStyle();
                style.Description = description;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                WallStyleComponent comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "comp2";
                comp.Position.SetFixedEdgeOffset(-3);
                comp.Position.SetFixedWidth(6.0);
                comp.Priority = 3;
                comp.Position.InstanceWidth.OperatorType = InstanceBasedValueOperatorType.Plus;
                comp.Position.InstanceWidth.Operand = 0.0;
                comp.Position.InstanceWidth.UseInstanceValue = true;
                style.Components.Add(comp);
            }

            stylename = "ComplexVariable";
            description = "ComplexVariable Wall Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                WallStyle style = new WallStyle();
                style.Description = description;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                WallStyleComponent comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "comp1";
                comp.Position.SetFixedEdgeOffset(-3);
                comp.Position.SetFixedWidth(1.0);
                comp.Priority = 3;
                style.Components.Add(comp);

                comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "comp2";
                comp.Position.SetFixedEdgeOffset(-2);
                comp.Position.SetFixedWidth(0);
                comp.Priority = 4;
                comp.Position.InstanceWidth.OperatorType = InstanceBasedValueOperatorType.Plus;
                comp.Position.InstanceWidth.Operand = 0.0;
                comp.Position.InstanceWidth.UseInstanceValue = true;
                style.Components.Add(comp);

                comp = new WallStyleComponent();
                comp.SubSetDatabaseDefaults(db);
                comp.Name = "comp3";
                comp.Position.SetFixedEdgeOffset(-2);
                comp.Position.SetFixedWidth(1.0);
                comp.Priority = 3;
                comp.Position.InstanceWidth.OperatorType = InstanceBasedValueOperatorType.Plus;
                comp.Position.InstanceWidth.Operand = 0.0;
                comp.Position.InstanceWidth.UseInstanceValue = true;
                style.Components.Add(comp);
            }

            trans.Commit();
        }
        catch
        {
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
    }
    #endregion

    #region CreateWindowStyles
    /// <summary>
    /// Creates the hardwired window styles.
    /// </summary>
    private void CreateWindowStyles()
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        Database db = HostApplicationServices.WorkingDatabase;
        TransactionManager tm = db.TransactionManager;
        Transaction trans = tm.StartTransaction();

        DictionaryWindowStyle dict = new DictionaryWindowStyle(db);

        try
        {
            String stylename = "Sophie";
            String description = "Sophie Window Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                WindowStyle style = new WindowStyle();
                style.Description = description;

                style.SashWidth = 1.5;
                style.SashDepth = 2.5;
                style.FrameWidth = 3.2;
                style.FrameDepth = 6.0;
                style.WindowType = WindowType.Picture;
                style.WindowShape = OpenShapeType.Oval;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                StandardSizeOpeningList list = new StandardSizeOpeningList();
                StandardSizeOpening opening = new StandardSizeOpening();
                opening.Set(12.0, 20.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(18.0, 24.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(24.0, 36.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(24.0, 12.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(36.0, 18.0, 0.0, 0.0);
                list.Openings.Add(opening);

                // create the extension dictionary
                style.CreateExtensionDictionary();
                ObjectId extDictId = style.ExtensionDictionary;
                DBObject obj = tm.GetObject(extDictId, OpenMode.ForWrite);
                DBDictionary dbDict = obj as DBDictionary;

                DBDictionary standardSizesDict = new DBDictionary();
                dbDict.SetAt(StandardSizeOpeningList.ExtensionDictionaryName, standardSizesDict);
                ObjectId idStandardSizesDict = dbDict.GetAt(StandardSizeOpeningList.ExtensionDictionaryName);

                tm.AddNewlyCreatedDBObject(standardSizesDict, true);

                // get the extension dictionary and add the list
                obj = tm.GetObject(idStandardSizesDict, OpenMode.ForWrite);
                dbDict = obj as DBDictionary;
                dbDict.SetAt(StandardSizeOpeningList.ExtensionDictionaryKeyName, list);

                tm.AddNewlyCreatedDBObject(list, true);
            }

            stylename = "Kori";
            description = "Kori Window Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                WindowStyle style = new WindowStyle();
                style.Description = description;
                style.FrameWidth = 2.0;
                style.FrameDepth = 4.0;
                style.WindowType = WindowType.DoubleHung;
                style.WindowShape = OpenShapeType.Rectangular;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                StandardSizeOpeningList list = new StandardSizeOpeningList();
                StandardSizeOpening opening = new StandardSizeOpening();
                opening.Set(24.0, 24.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(28.0, 30.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(30.0, 32.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(30.0, 34.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(32.0, 36.0, 0.0, 0.0);
                list.Openings.Add(opening);

                // create the extension dictionary
                style.CreateExtensionDictionary();
                ObjectId extDictId = style.ExtensionDictionary;
                DBObject obj = tm.GetObject(extDictId, OpenMode.ForWrite);
                DBDictionary dbDict = obj as DBDictionary;

                DBDictionary standardSizesDict = new DBDictionary();
                dbDict.SetAt(StandardSizeOpeningList.ExtensionDictionaryName, standardSizesDict);
                ObjectId idStandardSizesDict = dbDict.GetAt(StandardSizeOpeningList.ExtensionDictionaryName);

                tm.AddNewlyCreatedDBObject(standardSizesDict, true);

                // get the extension dictionary and add the list
                obj = tm.GetObject(idStandardSizesDict, OpenMode.ForWrite);
                dbDict = obj as DBDictionary;
                dbDict.SetAt(StandardSizeOpeningList.ExtensionDictionaryKeyName, list);

                tm.AddNewlyCreatedDBObject(list, true);
            }

            stylename = "Susanne";
            description = "Susanne Window Style";

            if (!(dict.Has(stylename, trans)))
            {
                ed.WriteMessage("Creating " + description + "...\r\n");

                WindowStyle style = new WindowStyle();
                style.Description = description;
                style.FrameWidth = 2.0;
                style.FrameDepth = 4.0;
                style.WindowType = WindowType.Casement;
                style.WindowShape = OpenShapeType.Hexagon;

                dict.AddNewRecord(stylename, style);
                tm.AddNewlyCreatedDBObject(style, true);

                StandardSizeOpeningList list = new StandardSizeOpeningList();
                StandardSizeOpening opening = new StandardSizeOpening();
                opening.Set(24.0, 24.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(28.0, 30.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(30.0, 32.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(30.0, 34.0, 0.0, 0.0);
                list.Openings.Add(opening);

                opening = new StandardSizeOpening();
                opening.Set(32.0, 36.0, 0.0, 0.0);
                list.Openings.Add(opening);

                // create the extension dictionary
                style.CreateExtensionDictionary();
                ObjectId extDictId = style.ExtensionDictionary;
                DBObject obj = tm.GetObject(extDictId, OpenMode.ForWrite);
                DBDictionary dbDict = obj as DBDictionary;

                DBDictionary standardSizesDict = new DBDictionary();
                dbDict.SetAt(StandardSizeOpeningList.ExtensionDictionaryName, standardSizesDict);
                ObjectId idStandardSizesDict = dbDict.GetAt(StandardSizeOpeningList.ExtensionDictionaryName);

                tm.AddNewlyCreatedDBObject(standardSizesDict, true);

                // get the extension dictionary and add the list
                obj = tm.GetObject(idStandardSizesDict, OpenMode.ForWrite);
                dbDict = obj as DBDictionary;
                dbDict.SetAt(StandardSizeOpeningList.ExtensionDictionaryKeyName, list);

                tm.AddNewlyCreatedDBObject(list, true);
            }

            trans.Commit();
        }
        catch
        {
            trans.Abort();
        }
        finally
        {
            trans.Dispose();
        }
    }
    #endregion

    #region IShapeForDesign
    void IShapeForDesign(Autodesk.Aec.Structural.DatabaseServices.DetailLevel detailLevel, out Curve2d[] curves, out Profile profile,
                         double d, double tw, double tf, double bf)
    {
        curves = null;
        profile = null;
        
        double hd = d * 0.5;
        double htw = tw * 0.5;
        double hbf = bf * 0.5;

        if (detailLevel == Autodesk.Aec.Structural.DatabaseServices.DetailLevel.Low) // Sketch
        {
            curves = new Curve2d[3]; 
            
            LineSegment2d line = new LineSegment2d(new Point2d(-hbf, hd), new Point2d(hbf, hd));
            curves[0] = line;

            line = new LineSegment2d(new Point2d (0.0, hd), new Point2d(0.0, -hd));
            curves[1] = line;

            line = new LineSegment2d (new Point2d (-hbf, -hd), new Point2d (hbf, -hd));
            curves[2] = line;
        }
        else if (detailLevel == Autodesk.Aec.Structural.DatabaseServices.DetailLevel.Medium) // Design
        {
            Point2dCollection pnts = new Point2dCollection();

            pnts.Add(new Point2d(-hbf, -hd        ));
            pnts.Add(new Point2d( hbf, -hd        ));
            pnts.Add(new Point2d( hbf, -hd + tf    ));
            pnts.Add(new Point2d( htw, -hd + tf    ));
            pnts.Add(new Point2d( htw,  hd - tf    ));
            pnts.Add(new Point2d( hbf,  hd - tf    ));
            pnts.Add(new Point2d( hbf,  hd        ));
            pnts.Add(new Point2d(-hbf,  hd        ));
            pnts.Add(new Point2d(-hbf,  hd - tf    ));
            pnts.Add(new Point2d(-htw,  hd - tf    ));
            pnts.Add(new Point2d(-htw, -hd + tf    ));
            pnts.Add(new Point2d(-hbf, -hd + tf    ));

            Ring ring = new Ring(pnts);

            profile = new Profile();
            profile.AddRing(ring);
        }
        else // Autodesk.Aec.Structural.DatabaseServices.DetailLevel.Medium (Detail)
        {
            // this is a quarter arc
            double qBulge = 0.41421356;
            double startWidth = -1;
            double endWidth = -1;

            Polyline pline = new Polyline();
            pline.AddVertexAt(0, new Point2d(-hbf, -hd), 0, startWidth, endWidth);
            pline.AddVertexAt(1, new Point2d(hbf, -hd), qBulge, startWidth, endWidth);
            pline.AddVertexAt(2, new Point2d(hbf - tf, -hd + tf), 0, startWidth, endWidth);
            pline.AddVertexAt(3, new Point2d(htw + tf, -hd + tf), -qBulge, startWidth, endWidth);
            pline.AddVertexAt(4, new Point2d(htw, -hd + 2 * tf), 0, startWidth, endWidth);
            pline.AddVertexAt(5, new Point2d(htw, hd - 2 * tf), -qBulge, startWidth, endWidth);
            pline.AddVertexAt(6, new Point2d(htw + tf, hd - tf), 0, startWidth, endWidth);
            pline.AddVertexAt(7, new Point2d(hbf - tf, hd - tf), qBulge, startWidth, endWidth);
            pline.AddVertexAt(8, new Point2d(hbf, hd), 0, startWidth, endWidth);
            pline.AddVertexAt(9, new Point2d(-hbf, hd), qBulge, startWidth, endWidth);
            pline.AddVertexAt(10, new Point2d(-hbf + tf, hd - tf), 0, startWidth, endWidth);
            pline.AddVertexAt(11, new Point2d(-htw - tf, hd - tf), -qBulge, startWidth, endWidth);
            pline.AddVertexAt(12, new Point2d(-htw, hd - 2 * tf), 0, startWidth, endWidth);
            pline.AddVertexAt(13, new Point2d(-htw, -hd + 2 * tf), -qBulge, startWidth, endWidth);
            pline.AddVertexAt(14, new Point2d(-htw - tf, -hd + tf), 0, startWidth, endWidth);
            pline.AddVertexAt(15, new Point2d(-hbf + tf, -hd + tf), qBulge, startWidth, endWidth);
            pline.AddVertexAt(16, new Point2d(-hbf, -hd), 0, startWidth, endWidth);

            Ring ring = new Ring();
            ring.AddSegmentsFrom(pline, true);

            profile = new Profile();
            profile.AddRing(ring);
        }
    }
    #endregion

    #region TShapeForDesign
    void TShapeForDesign(Autodesk.Aec.Structural.DatabaseServices.DetailLevel detailLevel, out Curve2d[] curves, out Profile profile,
                         double d, double tw, double tf, double bf)
    {
        curves = null;
        profile = null;

        double hd = d * 0.5;
        double htw = tw * 0.5;
        double hbf = bf * 0.5;

        if (detailLevel == Autodesk.Aec.Structural.DatabaseServices.DetailLevel.Low) // Sketch
        {
            curves = new Curve2d[2];

            LineSegment2d line = new LineSegment2d(new Point2d(-hbf, hd), new Point2d(hbf, hd));
            curves[0] = line;

            line = new LineSegment2d(new Point2d(0.0, -hd), new Point2d(0.0, hd));
            curves[1] = line;
        }
        else if (detailLevel == Autodesk.Aec.Structural.DatabaseServices.DetailLevel.Medium) // Design
        {
            Point2dCollection pnts = new Point2dCollection();

            pnts.Add(new Point2d(-htw, -hd));
            pnts.Add(new Point2d( htw, -hd));
            pnts.Add(new Point2d( htw,  hd - tf));
            pnts.Add(new Point2d( hbf,  hd - tf));
            pnts.Add(new Point2d( hbf,  hd));
            pnts.Add(new Point2d(-hbf,  hd));
            pnts.Add(new Point2d(-hbf,  hd - tf));
            pnts.Add(new Point2d(-htw,  hd - tf));

            Ring ring = new Ring(pnts);

            profile = new Profile();
            profile.AddRing(ring);
        }
        else // Autodesk.Aec.Structural.DatabaseServices.DetailLevel.Medium (Detail)
        {
            // this is a quarter arc
            double qBulge = 0.41421356;
            double startWidth = -1;
            double endWidth = -1;

            Polyline pline = new Polyline();
            pline.AddVertexAt( 0, new Point2d(-htw     , -hd)       ,       0, startWidth, endWidth);
            pline.AddVertexAt( 1, new Point2d( htw     , -hd)       ,       0, startWidth, endWidth);
            pline.AddVertexAt( 2, new Point2d( htw     ,  hd - 2*tf), -qBulge, startWidth, endWidth);
            pline.AddVertexAt( 3, new Point2d( htw + tf,  hd - tf)  ,       0, startWidth, endWidth);
            pline.AddVertexAt( 4, new Point2d( hbf - tf,  hd - tf)  ,  qBulge, startWidth, endWidth);
            pline.AddVertexAt( 5, new Point2d( hbf     ,  hd)       ,       0, startWidth, endWidth);
            pline.AddVertexAt( 6, new Point2d(-hbf     ,  hd)       ,  qBulge, startWidth, endWidth);
            pline.AddVertexAt( 7, new Point2d(-hbf + tf,  hd - tf)  ,       0, startWidth, endWidth);
            pline.AddVertexAt( 8, new Point2d(-htw - tf,  hd - tf)  , -qBulge, startWidth, endWidth);
            pline.AddVertexAt( 9, new Point2d(-htw     ,  hd - 2*tf),       0, startWidth, endWidth);
            pline.AddVertexAt(10, new Point2d(-htw     , -hd)       ,       0, startWidth, endWidth);

            Ring ring = new Ring();
            ring.AddSegmentsFrom(pline, true);

            profile = new Profile();
            profile.AddRing(ring);
        }
    }
    #endregion

    #region ASCForDesign
    void AmericanStandardChannelsForDesign(Autodesk.Aec.Structural.DatabaseServices.DetailLevel detailLevel, out Curve2d[] curves, out Profile profile,
                                           double d, double tw, double tf, double bf)
    {
        curves = null;
        profile = null;

        double hd = d * 0.5;
        double htw = tw * 0.5;
        double hbf = bf * 0.5;
        
        if (detailLevel == Autodesk.Aec.Structural.DatabaseServices.DetailLevel.Low) // Sketch
        {
            curves = new Curve2d[3]; 
            
            LineSegment2d line = new LineSegment2d(new Point2d(0.0, hd), new Point2d(hbf, hd));
            curves[0] = line;

            line = new LineSegment2d(new Point2d(0.0, hd), new Point2d(0.0, -hd));
            curves[1] = line;

            line = new LineSegment2d(new Point2d(0.0, -hd), new Point2d(hbf, -hd));
            curves[2] = line;
        }
        else if (detailLevel == Autodesk.Aec.Structural.DatabaseServices.DetailLevel.Medium) // Design
        {
            Point2dCollection pnts = new Point2dCollection();

            pnts.Add(new Point2d( 0.0, -hd));
            pnts.Add(new Point2d( hbf, -hd));
            pnts.Add(new Point2d( hbf, -hd + tf));
            pnts.Add(new Point2d( htw, -hd + tf));
            pnts.Add(new Point2d( htw,  hd - tf));
            pnts.Add(new Point2d( hbf,  hd - tf));
            pnts.Add(new Point2d( hbf,  hd));
            pnts.Add(new Point2d( 0.0,  hd));

            Ring ring = new Ring(pnts);

            profile = new Profile();
            profile.AddRing(ring);
        }
        else // Autodesk.Aec.Structural.DatabaseServices.DetailLevel.Medium (Detail)
        {
            // this is a quarter arc
            double qBulge = 0.41421356;
            double startWidth = -1;
            double endWidth = -1;

            Polyline pline = new Polyline();
            pline.AddVertexAt( 0, new Point2d(0.0      , -hd       ),0, startWidth, endWidth);
            pline.AddVertexAt( 1, new Point2d(hbf      , -hd       ), qBulge, startWidth, endWidth);
            pline.AddVertexAt( 2, new Point2d(hbf - tf , -hd + tf  ),0, startWidth, endWidth);
            pline.AddVertexAt( 3, new Point2d(htw + tf , -hd + tf     ),-qBulge, startWidth, endWidth);
            pline.AddVertexAt( 4, new Point2d(htw      , -hd + 2*tf),0, startWidth, endWidth);
            pline.AddVertexAt( 5, new Point2d(htw      ,  hd - 2*tf),-qBulge, startWidth, endWidth);
            pline.AddVertexAt( 6, new Point2d(htw + tf ,  hd - tf  ),0, startWidth, endWidth);
            pline.AddVertexAt( 7, new Point2d(hbf - tf ,  hd - tf     ), qBulge, startWidth, endWidth);
            pline.AddVertexAt( 8, new Point2d(hbf      ,  hd       ),0, startWidth, endWidth);
            pline.AddVertexAt( 9, new Point2d(0.0      ,  hd       ),0, startWidth, endWidth);
            pline.AddVertexAt(10, new Point2d(0.0      , -hd       ),0, startWidth, endWidth);

            Ring ring = new Ring();
            ring.AddSegmentsFrom(pline, true);

            profile = new Profile();
            profile.AddRing(ring);
        }
    }
    #endregion
}