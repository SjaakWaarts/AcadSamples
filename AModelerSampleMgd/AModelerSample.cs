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
using System.Collections.Generic;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Aec.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Aec.Modeler;
using Autodesk.Aec.Arch.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
#endregion

[assembly: ExtensionApplication(typeof(AModelerSampleMgd.AModelerSampleMgd))]
[assembly: CommandClass(typeof(AModelerSampleMgd.AModelerSampleMgd))]

namespace AModelerSampleMgd
{
    public class AModelerSampleMgd : IExtensionApplication
    {
        #region IExtensionApplication
        /// <summary>
        /// Initialization.
        /// </summary>
        public void Initialize()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Loading AModeler Sample...\r\nInput AModelerSample to start:\r\n");
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
        /// 
        [Autodesk.AutoCAD.Runtime.CommandMethod("AModelerSampleMgd", "AModelerSample", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
        public void AModelerSample()
        {
            string cmd = GetInputCommand();
            if(commandExec(cmd) == false)
                return;
            ZoomSample();
            Application.DocumentManager.MdiActiveDocument.Editor.Document.SendStringToExecute("AModelerSample\n", false, true, true);
        }
        #endregion

        private void InitSample()
        {
            Clear();
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("====================================================");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nInitializing AModeler .NET API sample by creating a base mass element entity at 0,0.\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("====================================================\n");
            Init();
        }
        
        private void Clear()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;
            Transaction t = tm.StartTransaction();
            BlockTable bt = tm.GetObject(db.BlockTableId, OpenMode.ForRead, false) as BlockTable;
            BlockTableRecord btr = tm.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;

            foreach (Autodesk.AutoCAD.DatabaseServices.ObjectId id in btr)
            {
                MassElement me = tm.GetObject(id, OpenMode.ForWrite) as MassElement;
                if(null!=me)
                    me.Erase();
            }

            t.Commit();
            t.Dispose();
        }

        private MassElement[] GetSelectedMassElement()
        {

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionOptions Opts = new PromptSelectionOptions();
            PromptSelectionResult psr = ed.GetSelection(Opts);

            if (psr.Status == PromptStatus.OK)
            {
                Autodesk.AutoCAD.EditorInput.SelectionSet ss = psr.Value;
                Autodesk.AutoCAD.DatabaseServices.ObjectId[] oids = ss.GetObjectIds();
                int count = oids.Length;
                MassElement[] result = new MassElement[count];
                Database db = Application.DocumentManager.MdiActiveDocument.Database;
                Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;
                Transaction trans = tm.StartTransaction();

                MassElement me = new MassElement();
                for (int i = 0 ; i < count; i++)
                {
                    Autodesk.AutoCAD.DatabaseServices.Entity ety = tm.GetObject(oids[i], OpenMode.ForWrite, true) as Autodesk.AutoCAD.DatabaseServices.Entity;
                    result[i] = ety as MassElement;
                }

                trans.Commit();
                trans.Dispose();
                return result;
            }
            return null;
        }

        private string GetInputCommand()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptKeywordOptions pko = new PromptKeywordOptions("Input the command:");
            pko.Keywords.Add("InitSample");
            pko.Keywords.Add("StretchBody");
            pko.Keywords.Add("AddBody");
            pko.Keywords.Add("ListBody");
            pko.Keywords.Add("SectionBody");
            pko.Keywords.Add("MultiplyBody");
            pko.Keywords.Add("SubBody");
            pko.Keywords.Add("Clear");
            pko.Keywords.Add("BooleanOp");
            pko.Keywords.Add("CombineBody");
            pko.Keywords.Add("TranslateBody");
            pko.Keywords.Add("RotateBody");
            pko.Keywords.Add("ScaleBody");
            pko.Keywords.Add("ListPickBody");
            pko.Keywords.Add("Quit");
            PromptResult result = ed.GetKeywords(pko);

            return result.StringResult;
        }

        private bool commandExec(string cmdString)
        {

            if (cmdString.ToLower() == "initsample")
            {
                InitSample();
                return true;
            }
            if (cmdString.ToLower() == "clear")
            {
                Clear();
                return true;
            }
            if (cmdString.ToLower() == "quit")
            {
                return false;
            }

            bool result = true;
            Database db = HostApplicationServices.WorkingDatabase;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;
            Transaction t = tm.StartTransaction();
            BlockTable bt = tm.GetObject(db.BlockTableId, OpenMode.ForRead, false) as BlockTable;
            BlockTableRecord btr = tm.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;
            
            foreach ( Autodesk.AutoCAD.DatabaseServices.ObjectId id in btr)
            {
                MassElement me = tm.GetObject(id, OpenMode.ForWrite) as MassElement;

                if (me == null)
                    continue;

                Autodesk.Aec.Modeler.Body b = me.Body;
                Autodesk.Aec.Modeler.Body b2 = null;

                if (cmdString.ToLower() == "subbody")
                {
                    Autodesk.Aec.Modeler.Body subB = Autodesk.Aec.Modeler.Body.Cone(new LineSegment3d(new Point3d(0, 0, 0), new Point3d(0, 0, 5)), 1, 1.5, 10);
                    b2 = b - subB;
                    me.SetBody(b2, true);
                }
                else if (cmdString.ToLower() == "multiplybody")
                {
                    Autodesk.Aec.Modeler.Body bb = Autodesk.Aec.Modeler.Body.Cone(new LineSegment3d(new Point3d(0, 0, 0), new Point3d(0, 0, 5)), 1, 1.5, 10);
                    b2 = b * bb;
                    me.SetBody(b2, true);
                }
                else if (cmdString.ToLower() == "sectionbody")
                {
                    b.Section(new Plane(new Point3d(0, 0, 0), new Vector3d(0, 1, 1)), true);
                    me.SetBody(b, false);
                }
                else if (cmdString.ToLower() == "listbody")
                {
                    ListBody(me);
                }
                else if (cmdString.ToLower() == "addbody")
                {
                    Autodesk.Aec.Modeler.Body bb = Autodesk.Aec.Modeler.Body.Cylinder(new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0)), 5, 20);
                    b2 = b + bb;
                    me.SetBody(b2, true);
                }
                else if (cmdString.ToLower() == "stretchbody")
                {
                    b.Stretch(new LineSegment3d(new Point3d(0, 0, 0), new Point3d(0, 10, 0)));
                    me.SetBody(b, false);
                }
                else if (cmdString.ToLower() == "booleanop")
                {
                    Autodesk.Aec.Modeler.Body bb = Autodesk.Aec.Modeler.Body.Cylinder(new LineSegment3d(new Point3d(0, 0, 0), new Point3d(10, 0, 0)), 5, 20);
                    b2 = b.BooleanOperation(bb, BooleanOperatorType.Intersect);
                    me.SetBody(b2, false);
                }
                else if (cmdString.ToLower() == "combinebody")
                {
                    Autodesk.Aec.Modeler.Body bb = Autodesk.Aec.Modeler.Body.Box(new Point3d(0, 0, 0), new Vector3d(11, 11, 11));
                    b2 = bb.Combine(b);
                    me.SetBody(b2, false);

                }
                else if (cmdString.ToLower() == "translatebody")
                {
                    b2 = b.Translate(new Vector3d(0, 10, 0));
                    me.SetBody(b2, false);
                }
                else if (cmdString.ToLower() == "rotatebody")
                {
                    b2 = b.Rotate(new LineSegment3d(new Point3d(0, 0, 0), new Point3d(1, 1, 1)), 30);
                    me.SetBody(b2, false);
                }
                else if (cmdString.ToLower() == "scalebody")
                {
                    b2 = b.Scale(new Point3d(0, 0, 0), 2);
                    me.SetBody(b2, true);
                }
                else if (cmdString.ToLower() == "listpickbody")
                {
                    MassElement[] ms = GetSelectedMassElement();
                    if (null != ms)
                    {
                        foreach(MassElement m in ms)
                            ListBody(m);
                    }
                }
                else
                {
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Wrong command!\n");
                    result = false;
                }
            }
            t.Commit();
            t.Dispose();
            return result;
        }

        private void Init()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;
            Transaction t = tm.StartTransaction();
            BlockTable bt = tm.GetObject(db.BlockTableId, OpenMode.ForRead, false) as BlockTable;
            BlockTableRecord btr = tm.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false) as BlockTableRecord;

            Autodesk.Aec.DatabaseServices.MassElement temp = Autodesk.Aec.DatabaseServices.MassElement.Create(ShapeType.BoundaryRepresentation);
            Autodesk.Aec.Modeler.Body body = Autodesk.Aec.Modeler.Body.Cone(new LineSegment3d(new Point3d(0, 0, 0), new Point3d(0, 0, 10)), 3, 3, 10);
            temp.SetBody(body, false);
            btr.AppendEntity(temp);
            tm.AddNewlyCreatedDBObject(temp, true);
            
            t.Commit();
            t.Dispose();

        }

        public static void ZoomSample()
        {
            try
            {
                Application.DocumentManager.MdiActiveDocument.Editor.Document.SendStringToExecute("zoom\n", false, true, true);
                Application.DocumentManager.MdiActiveDocument.Editor.Document.SendStringToExecute("e\n", false, true, true);
                Application.DocumentManager.MdiActiveDocument.Editor.Document.SendStringToExecute("-view\n", false, true, true);
                Application.DocumentManager.MdiActiveDocument.Editor.Document.SendStringToExecute("_seiso\n", false, true, true);
            }
            catch
            { }

        }

        private void ListBody(MassElement me)
        {
            Autodesk.Aec.Modeler.Body b = me.Body;
            int faceNumber = b.Faces.Count;
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("There are " + faceNumber + " faces.\n");
            int index = 0;
            foreach (Autodesk.Aec.Modeler.Face f in b.Faces)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("There are " + f.Edges.Count + " edges in face" + index + "\n");
                index++;
            }
            int surfaceNumber = b.Surfaces.Count;
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("There are " + surfaceNumber + " surfaces.\n");
            index = 0;
            foreach (Autodesk.Aec.Modeler.Surface s in b.Surfaces)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Surface " + index + " is a " + s.Type + "\n");
                index++;
            }
            int curveNumber = b.Curves.Count;
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("There are " + curveNumber + " curves.\n");
            index = 0;
            foreach (Autodesk.Aec.Modeler.Curve c in b.Curves)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Curve " + index + " is a " + c.Type + "\n");
                index++;
            }
            int vertexNumber = b.Vertices.Count;
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("There are " + vertexNumber + " vertices.\n");
            index = 0;
            foreach (Autodesk.Aec.Modeler.Vertex v in b.Vertices)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Vertices " + index + " : " + v.Point + "\n");
                index++;
            }

            Autodesk.Aec.Modeler.Vertex vMin = b.GetVertexMin(new Vector3d(0, 0, 1));
            Autodesk.Aec.Modeler.Vertex vMax = b.GetVertexMax(new Vector3d(0, 0, 1));
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("The Lowest Vertex is " + vMin.Point + "\n");
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("The Hightest Vertex is " + vMax.Point + "\n");
        }
    }
}


 

