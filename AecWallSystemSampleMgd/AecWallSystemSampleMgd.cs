#region Copyright
//      .NET Stream Sample
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

#region Namespace
using System;

using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.Aec.DatabaseServices;
using Autodesk.Aec.Arch.DatabaseServices;
using Autodesk.Aec.Arch.DatabaseServices.WallSystem;

using ObjectIdCollection = Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection;
using ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;
using Graph = Autodesk.Aec.Arch.DatabaseServices.WallSystem.Graph;
using Section = Autodesk.Aec.Arch.DatabaseServices.WallSystem.Section;
using TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
#endregion

#region ACASample
[assembly: ExtensionApplication(typeof(ACASample.WallSystemSample))]
[assembly: CommandClass(typeof(ACASample.WallSystemSample))]

namespace ACASample
{
    #region WallSystem Sample
    public class WallSystemSample : IExtensionApplication
    {
        static bool m_WallsDrawn = false;

        #region IExtensionApplication Implementations
        /// <summary>
        /// Initialization.
        /// </summary>
        public void Initialize()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Loading Wall System Sample...\r\n");
            ed.WriteMessage("Type \"WallSystemSample\" to run the sample:\r\n");
        }

        /// <summary>
        /// Terminate.
        /// </summary>
        public void Terminate()
        {

        }
        #endregion

        #region Command
        /// <summary>
        /// Commands support.
        /// </summary>
        [Autodesk.AutoCAD.Runtime.CommandMethod("WallSystemSample", "WallSystemSample", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
        public void Sample()
        {
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage("\n====================================================\n");
            editor.WriteMessage("Welcome to WallSystem Sample!\n");

            if (!m_WallsDrawn)
                DrawSomeWalls();

            PromptKeywordOptions options = new PromptKeywordOptions("Please select a sub command, escape to exit:");
            options.Keywords.Add("wallJointsShow");
            options.Keywords.Add("wallConnectionsList");
            options.Keywords.Add("wallsPaceDraw");
            options.Keywords.Add("wallSectionsShow");

            bool stop = false;
            while (!stop)
            {
                PromptResult pr = editor.GetKeywords(options);
                switch (pr.Status)
                {
                    case PromptStatus.OK:
                    case PromptStatus.Keyword:
                        CommandHandler(pr.StringResult);
                        break;
                    default:
                        stop = true;
                        break;
                }
            }

            editor.WriteMessage("\n====================================================\n");
        }
        #endregion

        #region Helper Functions
        void DrawSomeWalls()
        {

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                PromptPointOptions opts = new PromptPointOptions("\nWe will draw some walls to demonstrate Wall System APIs.\nPick a point for the center of the walls: ");
                PromptPointResult results = null;
                try
                {
                    results = ed.GetPoint(opts);
                }
                catch
                {
                    ed.WriteMessage("You did not select a valid point");
                    return;
                }

                if (results.Status == PromptStatus.Error)
                {
                    ed.WriteMessage("You did not select a valid point");
                    return;
                }

                Database db = Application.DocumentManager.MdiActiveDocument.Database;
                Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;

                Point3d centerPt = results.Value;

                using (Transaction trans1 = tm.StartTransaction())
                {
                    BlockTable bt = (BlockTable)(trans1.GetObject(db.BlockTableId, OpenMode.ForWrite));
                    BlockTableRecord btr = (BlockTableRecord)trans1.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    double distX = 4800;
                    double distY = 2400;

                    Point3d[] pts = new Point3d[5];
                    pts[0] = centerPt + new Vector3d(-(distX / 2), -(distY / 2), 0);
                    pts[1] = centerPt + new Vector3d((distX / 2), -(distY / 2), 0);
                    pts[2] = centerPt + new Vector3d((distX / 2), (distY / 2), 0);
                    pts[3] = centerPt + new Vector3d(-(distX / 2), (distY / 2), 0);
                    pts[4] = pts[0];


                    for (int i = 0; i < 4; i++)
                    {
                        Wall wall = new Wall();
                        wall.SetDatabaseDefaults(db);
                        wall.SetToStandard(db);
                        wall.SetDefaultLayer();
                        wall.Normal = new Vector3d(0, 0, 1);
                        wall.Set(pts[i], pts[i + 1], Vector3d.ZAxis);

                        btr.AppendEntity(wall);
                        trans1.AddNewlyCreatedDBObject(wall, true);
                    }
                    trans1.Commit();
                }

            }

            catch (System.Exception e)
            {
                ed.WriteMessage("\nException caught: " + e.Message);
            }
            finally
            {

            }

            m_WallsDrawn = true;

        }
        void CommandHandler(string cmd)
        {
            switch (cmd)
            {
                case "wallJointsShow":
                    getJointsWithSelection();
                    break;

                case "wallConnectionsList":
                    getConnectionsWithSelection();
                    break;

                case "wallSectionsShow":
                    getSectionsWithSelection();
                    break;
                    
                case "wallsPaceDraw":
                    wallSpaceDraw();
                    break;
                default:
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n* Unknown Keyword *\n");
                    break;
            }
        }

        void wallSpaceDraw()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TransactionManager tm = db.TransactionManager;
            Transaction trans = tm.StartTransaction();

            try
            {
                PromptEntityOptions entopts = new PromptEntityOptions("Select a space ");
                entopts.SetRejectMessage("Must select a space, please!");
                entopts.AddAllowedClass(typeof(Space), true);
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

                if (ent.Status == PromptStatus.OK)
                {
                    ObjectId entId = ent.ObjectId;

                    Space space = trans.GetObject(entId, OpenMode.ForRead, false) as Space;
                    if (space != null)
                    {
                        Manager mgr = new Manager(db);
                    }
                    else
                    {
                        ed.WriteMessage("\nSomething bad has happened...");
                    }
                }

                trans.Commit();
                Point3d centerPt = new Point3d(0.0,0.0,0.0);

                using (Transaction trans1 = tm.StartTransaction())
                {
                    BlockTable bt = (BlockTable)(trans1.GetObject(db.BlockTableId, OpenMode.ForWrite));
                    BlockTableRecord btr = (BlockTableRecord)trans1.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    double distX = 4800;
                    double distY = 2400;

                    Point3d[] pts = new Point3d[5];
                    pts[0] = centerPt + new Vector3d(-(distX / 2), -(distY / 2), 0);
                    pts[1] = centerPt + new Vector3d((distX / 2), -(distY / 2), 0);
                    pts[2] = centerPt + new Vector3d((distX / 2), (distY / 2), 0);
                    pts[3] = centerPt + new Vector3d(-(distX / 2), (distY / 2), 0);
                    pts[4] = pts[0];


                    for (int i = 0; i < 4; i++)
                    {
                        Wall wall = new Wall();
                        wall.SetDatabaseDefaults(db);
                        wall.SetToStandard(db);
                        wall.SetDefaultLayer();
                        wall.Normal = new Vector3d(0, 0, 1);
                        wall.Set(pts[i], pts[i + 1], Vector3d.ZAxis);

                        btr.AppendEntity(wall);
                        trans1.AddNewlyCreatedDBObject(wall, true);
                    }
                    trans1.Commit();

                }
            }
            catch (System.Exception e)
            {
                trans.Abort();
                ed.WriteMessage(e.Message);
            }
            finally
            {
                trans.Dispose();
            }
        }

        void DrawBlip(Point3d pt, double size, int color, bool highlight)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Vector3d vecLL = new Vector3d(-size / 2, -size / 2, 0);
            Vector3d vecUL = new Vector3d(-size / 2, size / 2, 0);
            Vector3d vecLR = new Vector3d(size / 2, -size / 2, 0);
            Vector3d vecUR = new Vector3d(size / 2, size / 2, 0);
            ed.DrawVector((pt + vecLL), (pt + vecUR), color, highlight);
            ed.DrawVector((pt + vecUL), (pt + vecLR), color, highlight);
        }

        void getJointsWithSelection()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TransactionManager tm = db.TransactionManager;
            Transaction trans = tm.StartTransaction();

            try
            {
                PromptEntityOptions entopts = new PromptEntityOptions("Select a wall ");
                entopts.SetRejectMessage("Must select a wall, please!");
                entopts.AddAllowedClass(typeof(Wall), true);
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

                if (ent.Status == PromptStatus.OK)
                {
                    ObjectId entId = ent.ObjectId;

                    Wall wall = trans.GetObject(entId, OpenMode.ForRead, false) as Wall;
                    if (wall != null)
                    {
                        Manager mgr = new Manager(db);
                        Graph theGraph = mgr.FindGraph(wall);
                        Matrix3d mat = theGraph.WcsToEcsMatrix.Inverse();
                        for (int i = 0; i < theGraph.WallJointCount; i++)
                        {
                            Joint joint = theGraph.GetWallJoint(i);
                            Point3d jointloc = new Point3d(joint.Location.X, joint.Location.Y, 0);
                            Point3d jointlocTrans = jointloc.TransformBy(mat);
                            DrawBlip(jointloc.TransformBy(mat), 3, 1, false);
                            ed.WriteMessage("\n Joint at (ECS): " + joint.Location.ToString() + " (WCS): " + jointlocTrans.ToString());
                        }
                    }
                    else
                    {
                        ed.WriteMessage("\nSomething bad has happened...");
                    }
                }

                trans.Commit();
            }
            catch (System.Exception e)
            {
                trans.Abort();
                ed.WriteMessage(e.Message);
            }
            finally
            {
                trans.Dispose();
            }
        }


        void getConnectionsWithSelection()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TransactionManager tm = db.TransactionManager;
            Transaction trans = tm.StartTransaction();

            try
            {
                PromptEntityOptions entopts = new PromptEntityOptions("Select a wall ");
                entopts.SetRejectMessage("Must select a wall, please!");
                entopts.AddAllowedClass(typeof(Wall), true);
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

                if (ent.Status == PromptStatus.OK)
                {
                    ObjectId entId = ent.ObjectId;

                    Wall wall = trans.GetObject(entId, OpenMode.ForRead, false) as Wall;
                    Manager mgr = new Manager(db);
                    Graph theGraph = mgr.FindGraph(wall);
                    Matrix3d mat = theGraph.WcsToEcsMatrix.Inverse();
                    for (int i = 0; i < theGraph.WallJointCount; i++)
                    {
                        Joint joint = theGraph.GetWallJoint(i);
                        ConnectionCollection connections = joint.Connections;
                        foreach (Connection connection in connections)
                        {
                            ed.WriteMessage("\nConnection in wall system for wall id: " + connection.Section.WallId.ToString());
                            ed.WriteMessage("\n  Direction from here: " + connection.DirectionFromHere.ToString());
                            foreach (double elevation in connection.ElevationVariations)
                            {
                                ed.WriteMessage("\n  Elevation Variations: " + elevation.ToString());
                            }
                            ed.WriteMessage("\n  Start Elevation: " + connection.StartElevation.ToString());
                            ed.WriteMessage("\n  End Elevation: " + connection.EndElevation.ToString());
                            ed.WriteMessage("\n  Section Starts Here: " + connection.SectionStartsHere.ToString());

                        }
                    }
                }

                trans.Commit();
            }
            catch (System.Exception e)
            {
                trans.Abort();
                ed.WriteMessage(e.Message);
            }
            finally
            {
                trans.Dispose();
            }
        }

         void getSectionsWithSelection()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TransactionManager tm = db.TransactionManager;
            Transaction trans = tm.StartTransaction();

            try
            {

                PromptEntityOptions entopts = new PromptEntityOptions("Select a wall ");
                entopts.SetRejectMessage("Must select a wall, please!");
                entopts.AddAllowedClass(typeof(Wall), true);
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

                if (ent.Status == PromptStatus.OK)
                {
                    ObjectId entId = ent.ObjectId;

                    Wall wall = trans.GetObject(entId, OpenMode.ForRead, false) as Wall;
                    Manager mgr = new Manager(db);
                    Graph theGraph = mgr.FindGraph(wall);
                    Matrix3d mat = theGraph.WcsToEcsMatrix.Inverse();
                    for (int i = 0; i < theGraph.WallJointCount; i++)
                    {
                        Joint joint = theGraph.GetWallJoint(i);
                        ConnectionCollection connections = joint.Connections;
                        foreach (Connection connection in connections)
                        {
                            Section section = connection.Section;
                            int count = section.ComponentSetCount;
                            ed.WriteMessage("\n  Section in wall system for wall id: " + section.WallId + "\n    component set count = " + count);
                            ed.DrawVector(new Point3d(section.StartPoint.X, section.StartPoint.Y, 0).TransformBy(mat),
                                          new Point3d(section.EndPoint.X, section.EndPoint.Y, 0).TransformBy(mat), 4, true);
                        }
                    }
                }

                trans.Commit();
            }
            catch (System.Exception e)
            {
                trans.Abort();
                ed.WriteMessage(e.Message);
            }
            finally
            {
                trans.Dispose();
            }
        }
        #endregion
    }
    #endregion
}
#endregion