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

#region Namespaces
using System;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.Aec.DatabaseServices;
using Autodesk.Aec.Arch.DatabaseServices;

using ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;
using ObjectIdCollection = Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using Database = Autodesk.AutoCAD.DatabaseServices.Database;
using TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
using Transaction = Autodesk.AutoCAD.DatabaseServices.Transaction;
using OpenMode = Autodesk.AutoCAD.DatabaseServices.OpenMode;
using LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight;
using BlockReference = Autodesk.AutoCAD.DatabaseServices.BlockReference;
using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;
using AecModeler = Autodesk.Aec.Modeler;
using Autodesk.Aec.Geometry;
#endregion

[assembly: ExtensionApplication(typeof(AecStreamSampleMgd.StreamSample))]
[assembly: CommandClass(typeof(AecStreamSampleMgd.StreamSample))]

namespace AecStreamSampleMgd
{
    public class StreamSample : IExtensionApplication
    {
        #region IExtensionApplication
        /// <summary>
        /// Initialization.
        /// </summary>
        public void Initialize()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Loading Stream Sample...\r\n");
            ed.WriteMessage("Type \"StreamSample\" to run the sample:\r\n");
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
        [Autodesk.AutoCAD.Runtime.CommandMethod("StreamSample", "StreamSample", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
        public void ShowSample()
        {
            Editor editor = GetEditor();
            editor.WriteMessage("\n====================================================\n");
            editor.WriteMessage("Thank you for using StreamSample\n");

            PromptKeywordOptions options = new PromptKeywordOptions("Please select a stream type "
                + "[collect Bodies/collect Clip bodies/collect Materials/cUrves/Explode/eXtent/Intersect/Slice/Vector]",
                "collectBodies collectClipbodies collectMaterials cUrves Explode eXtent Intersect Slice Vector");

            bool stop = false;
            while (!stop)
            {
                PromptResult pr = editor.GetKeywords(options);
                switch (pr.Status)
                {
                    case PromptStatus.OK:
                    case PromptStatus.Keyword:
                        DispatchCommand(pr.StringResult);
                        break;
                    default:
                        stop = true;
                        break;
                }
            }

            editor.WriteMessage("\n====================================================\n");
        }
        #endregion

        #region HelperFunctions
        private Database GetDatabase()
        {
            return Application.DocumentManager.MdiActiveDocument.Database;
        }
        private Editor GetEditor()
        {
            return Application.DocumentManager.MdiActiveDocument.Editor;
        }
        ObjectId PickObject(Type type, bool exactMatch, string tips)
        {
            Editor editor = GetEditor();
            PromptEntityOptions options = null;
            PromptEntityResult result;
            if (type != null)
            {
                options = new PromptEntityOptions(tips);
                options.SetRejectMessage("This type of object is not allowed.\n");
                options.AddAllowedClass(type, exactMatch);
                result = editor.GetEntity(options);
            }
            else
            {
                result = editor.GetEntity(tips);
            }
            if (result.Status == PromptStatus.OK && result.ObjectId.IsValid)
                return result.ObjectId;
            else
                return ObjectId.Null;
        }

        ObjectId PickObject(Type type, bool exactMatch)
        {
            return PickObject(type, exactMatch, "Please pick an object or return");
        }

        ObjectIdCollection PickObjectSet(string tips)
        {
            Editor editor = GetEditor();
            PromptSelectionOptions options = new PromptSelectionOptions();
            options.MessageForAdding = tips;
            options.MessageForRemoval = "Remove objects from selection";
            options.AllowDuplicates = false;
            options.RejectObjectsFromNonCurrentSpace = true;
            ObjectIdCollection ids = new ObjectIdCollection();

            PromptSelectionResult result = editor.GetSelection(options);
            if (result.Status == PromptStatus.OK)
            {
                SelectionSet set = result.Value;
                foreach (SelectedObject obj in set)
                    ids.Add(obj.ObjectId);
            }

            return ids;
        }

        ObjectIdCollection PickObjectSet()
        {
            return PickObjectSet("Add objects to selection");
        }

        Plane PromptPlane()
        {
            Editor editor = GetEditor();
            Point3d[] pts = new Point3d[3];
            int numPicked = 0;
            while (numPicked < 3)
            {
                // pick
                PromptPointResult result = editor.GetPoint("Please pick point " + (numPicked + 1) + " on the plane");
                if (result.Status != PromptStatus.OK)
                    return null;

                // check for duplicated
                Point3d pt = result.Value;
                bool ok = true;
                for (int i = 0; i < numPicked; ++i)
                {
                    if (pt.DistanceTo(pts[i]) <= Tolerance.Global.EqualPoint)
                    {
                        editor.WriteMessage("Duplicated point, please select again.\n");
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    pts[numPicked] = pt;
                    ++numPicked;
                }
            }
            Vector3d v1 = pts[1] - pts[0];
            Vector3d v2 = pts[2] - pts[0];
            Vector3d normal = v1.CrossProduct(v2);
            Plane plane = new Plane(pts[0], normal);
            return plane;
        }

        void HighlightBody(AecModeler.Body body, int color)
        {
            Editor editor = GetEditor();

            foreach (AecModeler.Edge edge in body.PhysicalEdges)
            {
                editor.DrawVector(edge.Line.StartPoint, edge.Line.EndPoint, color, true);
            }
        }

        void HighlightBody(AecModeler.Body body)
        {
            HighlightBody(body, 1);
        }

        void HighlightGraphics(GraphicsStorage gs)
        {
            Type t = gs.GetType();
            switch (t.Name)
            {
                case "GraphicsStorageArc":
                    HighlightGraphicsArc(gs as GraphicsStorageArc);
                    break;
                case "GraphicsStorageBody":
                    HighlightGraphicsBody(gs as GraphicsStorageBody);
                    break;
                case "GraphicsStorageFace":
                    HighlightGraphicsFace(gs as GraphicsStorageFace);
                    break;
                case "GraphicsStorageHatch":
                    HighlightGraphicsHatch(gs as GraphicsStorageHatch);
                    break;
                case "GraphicsStoragePolyline":
                    HighlightGraphicsPolyline(gs as GraphicsStoragePolyline);
                    break;
                case "GraphicsStorageShell":
                    HighlightGraphicsShell(gs as GraphicsStorageShell);
                    break;
            }
        }

        Point3dCollection ArcToVectors(CircularArc3d arc, int numSegments)
        {
            Interval interval = arc.GetInterval();
            double increment = interval.Length / (double)numSegments;
            double controller = interval.LowerBound;

            Point3dCollection points = new Point3dCollection();

            for (int i = 0; i < numSegments; ++i)
            {
                points.Add(arc.EvaluatePoint(controller));
                controller += increment;
            }

            points.Add(arc.EvaluatePoint(controller));

            return points;
        }

        void HighlightSegments(Point3dCollection points, bool closed)
        {
            Editor editor = GetEditor();
            int maxIndex = points.Count - 1;
            int i = 0;
            while (i < maxIndex)
            {
                editor.DrawVector(points[i], points[i + 1], 1, true);
                ++i;
            }

            if (closed)
                editor.DrawVector(points[maxIndex], points[0], 1, true);
        }

        void HighlightArc(CircularArc3d arc)
        {
            Point3dCollection points = ArcToVectors(arc, 25);
            HighlightSegments(points, false);
        }

        void HighlightGraphicsArc(GraphicsStorageArc gsArc)
        {
            CircularArc3d arc3d = gsArc.Arc;
            HighlightArc(arc3d);
        }

        void HighlightGraphicsBody(GraphicsStorageBody gsBody)
        {
            HighlightBody(gsBody.Body);
        }

        void HighlightGraphicsFace(GraphicsStorageFace gsFace)
        {
            Point3dCollection points = gsFace.Points;
            HighlightSegments(points, true);
        }

        void HighlightGraphicsHatch(GraphicsStorageHatch gsHatch)
        {
            Editor editor = GetEditor();
            LineSegment3d[] vectors = gsHatch.Vectors;
            foreach (LineSegment3d line in vectors)
                editor.DrawVector(line.StartPoint, line.EndPoint, 1, true);
        }

        void HighlightGraphicsPolyline(GraphicsStoragePolyline gsPline)
        {
            Point3dCollection points = gsPline.Points;
            HighlightSegments(points, false);
        }

        void HighlightGraphicsShell(GraphicsStorageShell gsShell)
        {
            // draw all edges including edges of holes
            Editor editor = GetEditor();
            Point3dCollection vertices = gsShell.Vertices;
            int[] faceList = gsShell.FaceList;
            int sum = 0;
            int count = (int)faceList.Length;
            while (sum < count)
            {
                int i = sum + 1;
                int maxIndex = (int)(i + Math.Abs(faceList[sum]) - 1);
                Point3d startPoint = vertices[(int)faceList[i]];
                while (i < maxIndex)
                {
                    editor.DrawVector(vertices[(int)faceList[i]], vertices[(int)faceList[i + 1]], 1, true);
                    ++i;
                }
                editor.DrawVector(vertices[(int)faceList[i]], startPoint, 1, true);

                sum += (int)(Math.Abs(faceList[sum]) + 1);
            }
            Autodesk.AutoCAD.GraphicsInterface.FaceData faceData = gsShell.FaceData;
            Autodesk.AutoCAD.GraphicsInterface.EdgeData edgeData = gsShell.EdgeData;
        }

        void HighlightBoundBox3d(BoundBox3d box)
        {
            if (!box.IsValid)
                return;

            Editor editor = GetEditor();

            double width = box.Width;
            double height = box.Height;
            double depth = box.Depth;
            Point3d minPt = box.MinPoint;

            editor.DrawVector(minPt, minPt + new Vector3d(width, 0, 0), 1, true);
            editor.DrawVector(minPt + new Vector3d(width, 0, 0), minPt + new Vector3d(width, depth, 0), 1, true);
            editor.DrawVector(minPt + new Vector3d(width, depth, 0), minPt + new Vector3d(0, depth, 0), 1, true);
            editor.DrawVector(minPt + new Vector3d(0, depth, 0), minPt, 1, true);

            editor.DrawVector(minPt + new Vector3d(0, 0, height), minPt + new Vector3d(width, 0, height), 2, true);
            editor.DrawVector(minPt + new Vector3d(width, 0, height), minPt + new Vector3d(width, depth, height), 2, true);
            editor.DrawVector(minPt + new Vector3d(width, depth, height), minPt + new Vector3d(0, depth, height), 2, true);
            editor.DrawVector(minPt + new Vector3d(0, depth, height), minPt + new Vector3d(0, 0, height), 2, true);

            editor.DrawVector(minPt, minPt + new Vector3d(0, 0, height), 3, true);
            editor.DrawVector(minPt + new Vector3d(width, 0, 0), minPt + new Vector3d(width, 0, height), 3, true);
            editor.DrawVector(minPt + new Vector3d(width, depth, 0), minPt + new Vector3d(width, depth, height), 3, true);
            editor.DrawVector(minPt + new Vector3d(0, depth, 0), minPt + new Vector3d(0, depth, height), 3, true);
        }

        void HighlightProfile(Profile profile, Plane plane)
        {
            Editor editor = GetEditor();
            foreach (Ring ring in profile.Rings)
            {
                foreach (Segment2d segment in ring.Segments)
                {
                    editor.DrawVector(new Point3d(plane, segment.StartPoint), new Point3d(plane, segment.EndPoint), 1, true);
                }
            }
        }

        void DispatchCommand(string command)
        {
            switch (command.ToLower())
            {
                case "collectbodies":
                    StreamCollectBodiesSample();
                    break;
                case "collectclipbodies":
                    StreamCollectClipBodiesSample();
                    break;
                case "collectmaterials":
                    StreamCollectMaterialsSample();
                    break;
                case "curves":
                    StreamCurvesSample();
                    break;
                case "explode":
                    StreamExplodeSample();
                    break;
                case "extent":
                    StreamExtentSample();
                    break;
                case "intersect":
                    StreamIntersectSample();
                    break;
                case "slice":
                    StreamSliceSample();
                    break;
                case "vector":
                    StreamVectorSample();
                    break;
                default:
                    GetEditor().WriteMessage("\n* Unknown Keyword *\n");
                    break;
            }
        }
        #endregion

        private void StreamVectorSample()
        {
            // intro message
            Editor editor = GetEditor();
            editor.WriteMessage("* StreamVector *\n");
            editor.WriteMessage("StreamVector sample demonstrates the ability of StreamVector that breaks selected entity into primitive lines.\n");
            editor.WriteMessage("Pick an object in the current drawing and the collected lines will be highlighted.\n");

            // pick an object
            ObjectId id = PickObject(typeof(Entity), false);
            if (id.IsNull)
            {
                editor.WriteMessage("No object is picked\n");
                return;
            }

            Database db = GetDatabase();
            StreamVector stream = new StreamVector(db);
            TransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                Entity ent = trans.GetObject(id, OpenMode.ForRead) as Entity;

                // setup the display parameters
                stream.PushDisplayParameters(DictionaryDisplayConfiguration.GetStandardDisplayConfiguration(db), trans);

                // stream the object
                stream.Stream(ent);

                stream.PopDisplayParameters();

                trans.Commit(); // for better performance
            }

            // highlights the collected lines
            foreach (LineSegment3d line in stream.Lines)
                editor.DrawVector(line.StartPoint, line.EndPoint, 1, true);
        }

        private void StreamIntersectSample()
        {
            // intro message
            Editor editor = GetEditor();
            editor.WriteMessage("* StreamIntersect *\n");
            editor.WriteMessage("StreamIntersect detects intersections between two streams.\n");
            editor.WriteMessage("Pick two objects in the current drawing. The intersections will be highlighted and printed out in the output window.\n");

            // pick two objects
            ObjectId id1 = PickObject(typeof(Entity), false, "Please pick the first object");
            if (id1.IsNull)
            {
                editor.WriteMessage("No object is picked\n");
                return;
            }

            ObjectId id2;
            do
            {
                id2 = PickObject(typeof(Entity), false, "Please pick the second object");
                if (id2.IsNull)
                {
                    editor.WriteMessage("No object is picked\n");
                    return;
                }
            } while (id2 == id1);

            Database db = GetDatabase();
            IntPtr ptr = new IntPtr();
            StreamIntersect stream1 = new StreamIntersect(db, ptr);
            StreamIntersect stream2 = new StreamIntersect(db, ptr);
            Point3dCollection points = new Point3dCollection();

            TransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                Entity ent1 = trans.GetObject(id1, OpenMode.ForRead) as Entity;
                Entity ent2 = trans.GetObject(id2, OpenMode.ForRead) as Entity;

                // put the entities in the streams respectively
                stream1.PushDisplayParameters(DictionaryDisplayConfiguration.GetStandardDisplayConfiguration(db), trans);
                stream2.PushDisplayParameters(DictionaryDisplayConfiguration.GetStandardDisplayConfiguration(db), trans);
                stream1.Stream(ent1);
                stream2.Stream(ent2);

                // detect intersections
                int count = stream1.IntersectWith(stream2, points);

                stream1.PopDisplayParameters();
                stream2.PopDisplayParameters();

                trans.Commit();
            }

            // print and highlight the results
            editor.WriteMessage("\nHere are the intersections: " + points.Count + " in total\n");
            foreach (Point3d point in points)
            {
                editor.DrawVector(point, point, 1, true);
                editor.WriteMessage("(" + point.X + ", " + point.Y + ", " + point.Z + ")\n");
            }
        }

        private void StreamExtentSample()
        {
            // intro message
            Editor editor = GetEditor();
            editor.WriteMessage("* StreamExtent *\n");
            editor.WriteMessage("StreamExtent detects the extents of all the objects in the stream.\n");
            editor.WriteMessage("Pick some objects in the current drawing and the extents will be highlighted.\n");

            // pick multiple objects
            ObjectIdCollection ids = PickObjectSet();
            if (ids.Count == 0)
            {
                editor.WriteMessage("No object is picked\n");
                return;
            }

            Database db = GetDatabase();
            StreamExtent stream = new StreamExtent(db);

            TransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                stream.PushDisplayParameters(DictionaryDisplayConfiguration.GetStandardDisplayConfiguration(db), trans);

                foreach (ObjectId id in ids)
                {
                    Entity ent = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    stream.Stream(ent);
                }

                stream.PopDisplayParameters();
                trans.Commit();
            }

            HighlightBoundBox3d(stream.Extents);
        }

        private void StreamExplodeSample()
        {
            // intro message
            Editor editor = GetEditor();
            editor.WriteMessage("* StreamExplode *\n");
            editor.WriteMessage("StreamExplode converts all entities to AutoCAD primitive entities.\n");
            editor.WriteMessage("Pick an object in the current drawing. The object you picked will be packaged to an anonymous block and finally referenced in the current space. The block-referenced object will be overlapping with the original one.\n");

            // pick a object to be exploded
            ObjectId id = PickObject(typeof(Entity), false);
            if (id.IsNull)
            {
                editor.WriteMessage("No object is picked\n");
                return;
            }

            Database db = GetDatabase();
            DBObjectCollection objects = new DBObjectCollection();
            StreamExplode streamExplode = new StreamExplode(db, objects);
            streamExplode.IsVisualExplode = true;
            streamExplode.SetForBoundarySearch(false);

            TransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                Entity ent = trans.GetObject(id, OpenMode.ForRead) as Entity;

                streamExplode.PushDisplayParameters(DictionaryDisplayConfiguration.GetStandardDisplayConfiguration(db), trans);
                streamExplode.PushEntity(ent);
                streamExplode.PushProperties(ent);

                streamExplode.Stream(ent);
                streamExplode.PackageExplodedEntities();

                streamExplode.PopProperties();
                streamExplode.PopEntity();
                streamExplode.PopDisplayParameters();

                trans.Commit();
            }

            BlockReference blockRef = null;
            foreach (DBObject obj in objects)
            {
                if (obj.GetType() == typeof(BlockReference))
                    blockRef = obj as BlockReference;
            }

            if (blockRef == null)
                return;

            blockRef.Position = Point3d.Origin;

            using (Transaction trans = tm.StartTransaction())
            {
                BlockTableRecord btr = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                btr.AppendEntity(blockRef);
                trans.AddNewlyCreatedDBObject(blockRef, true);
                trans.Commit();
            }
        }

        private void StreamCurvesSample()
        {
            Editor editor = GetEditor();
            Database db = GetDatabase();
            editor.WriteMessage("* StreamCurves *\n");
            editor.WriteMessage("StreamCurves collects curves and lines from the objects passed in.\n");
            editor.WriteMessage("Pick some objects in the current drawing. The collected graphics will be highlighted while the numbers of collected lines and curves will be printed in the output window.\n");

            ObjectIdCollection ids = PickObjectSet();
            if (ids.Count == 0)
            {
                editor.WriteMessage("No object is picked\n");
                return;
            }

            StreamCurves stream = new StreamCurves(db);
            // collect arcs instead of breaking arcs into line segments
            stream.CollectArcs = true;
            TransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                stream.PushDisplayParameters(DictionaryDisplayConfiguration.GetStandardDisplayConfiguration(db), trans);

                foreach (ObjectId id in ids)
                {
                    Entity ent = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    stream.Stream(ent);
                }

                stream.PopDisplayParameters();
                trans.Commit();
            }

            LineSegment3d[] lines = stream.Lines;
            CircularArc3d[] arcs = stream.Arcs;
            editor.WriteMessage("Collected " + lines.Length + " lines and " + arcs.Length + " arcs.\n");
            foreach (LineSegment3d line in lines)
                editor.DrawVector(line.StartPoint, line.EndPoint, 1, true);
            foreach (CircularArc3d arc in arcs)
                HighlightArc(arc);
        }

        private void StreamCollectBodiesSample()
        {
            Database db = GetDatabase();
            Editor editor = GetEditor();
            editor.WriteMessage("* StreamCollectBodies *\n");
            editor.WriteMessage("StreamCollectBodies collects bodies from the objects passed in.\n");
            editor.WriteMessage("Pick some objects in the current drawing. The collected bodies will be highlighted in the current view. And the volume of the bodies will be printed out in the output window.\n");

            ObjectIdCollection ids = PickObjectSet();
            if (ids.Count == 0)
            {
                editor.WriteMessage("No object is picked\n");
                return;
            }

            StreamCollectBodies stream = new StreamCollectBodies(db);
            TransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                stream.PushDisplayParameters(DictionaryDisplayConfiguration.GetStandardDisplayConfiguration(db), trans);

                foreach (ObjectId id in ids)
                {
                    Entity ent = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    stream.Stream(ent);
                }

                stream.PopDisplayParameters();
                trans.Commit();
            }

            // draw the collected body and output the volume of the body
            AecModeler.Body body = stream.GetBodyCopy();
            HighlightBody(body);

            editor.WriteMessage("Body volume = " + body.Volume.ToString() + "\n");
        }

        void StreamSliceSample()
        {
            Database db = GetDatabase();
            Editor editor = GetEditor();
            editor.WriteMessage("* StreamSlice *\n");
            editor.WriteMessage("StreamSlice slices bodies with a plane.\n");
            editor.WriteMessage("Pick some objects in the current drawing to be sliced and then pick three points to define the plane. The result profile will be highlighted.\n");
            ObjectIdCollection ids = PickObjectSet("Please pick the bodies need to be sliced");
            if (ids.Count == 0)
            {
                editor.WriteMessage("No object is picked\n");
                return;
            }

            editor.WriteMessage("Now you need to pick 3 points from the screen to define the slicing plane.\n");
            Plane plane = PromptPlane();
            if (plane == null)
                return;

            StreamSlice stream = new StreamSlice(db, plane);

            TransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                stream.PushDisplayParameters(DictionaryDisplayConfiguration.GetStandardDisplayConfiguration(db), trans);

                foreach (ObjectId id in ids)
                {
                    Entity ent = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    stream.Stream(ent);
                }

                stream.PopDisplayParameters();
                trans.Commit();
            }

            HighlightProfile(stream.GetProfile(), plane);
        }

        void StreamCollectClipBodiesSample()
        {
            Database db = GetDatabase();
            Editor editor = GetEditor();
            editor.WriteMessage("* StreamCollectClipBodies *\n");
            editor.WriteMessage("StreamCollectClipBodies clips all geometry pushed in against the supplied body.\n");
            editor.WriteMessage("Pick some objects in the current drawing and then pick an mass element to define the clipping boundary. The collected graphics will be highlighted in the current view.\n");

            ObjectIdCollection ids = PickObjectSet("Please pick the objects to be clipped");
            if (ids.Count == 0)
            {
                editor.WriteMessage("No object is picked\n");
                return;
            }

            ObjectId massElemId = PickObject(typeof(MassElement), true, "Please pick a mass element to define the clipping boundary");
            if (massElemId.IsNull)
            {
                editor.WriteMessage("A mass element is needed to define the clipping boundary.\n");
                return;
            }

            StreamCollectClipBodies stream = new StreamCollectClipBodies(db);
            // You may tell the stream to retain bodies instead of turning bodies into shells.
            // stream.SetRetainBodies(true);
            // But now we use the default setting, which uses shell as output.
            TransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                MassElement masselem = trans.GetObject(massElemId, OpenMode.ForRead) as MassElement;
                AecModeler.Body body = masselem.Body.Transform(masselem.Ecs);
                stream.SetBodyClipVolume(body);

                stream.PushDisplayParameters(DictionaryDisplayConfiguration.GetStandardDisplayConfiguration(db), trans);

                foreach (ObjectId id in ids)
                {
                    Entity entity = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    stream.Stream(entity);
                }

                stream.PopDisplayParameters();
                trans.Commit();
            }

            GraphicsStorage[] gsCollection = stream.GetCollectedGraphics();
            foreach (GraphicsStorage gs in gsCollection)
                HighlightGraphics(gs);
        }

        void StreamCollectMaterialsSample()
        {
            Database db = GetDatabase();
            Editor editor = GetEditor();
            editor.WriteMessage("* StreamCollectMaterials *\n");
            editor.WriteMessage("StreamCollectMaterials collects bodies of the specified materials from the entities pushed into the stream.\n");
            editor.WriteMessage("Pick some objects in the current drawing. The bodies with materials specifed will be highlighted. And bodies with the same material are displayed in one color.\n");

            StreamCollectMaterials stream = new StreamCollectMaterials(db);
            // StreamCollectMaterials needs the users to specify the materials they're interested in by setting the MaterialFilter property.
            // Here we add all the material ids in the current drawing to the filter collection.
            DictionaryMaterialDefinition dictMaterials = new DictionaryMaterialDefinition(db);
            stream.MaterialFilter = dictMaterials.Records;
            // To combine the bodies with the same material instead of traversing the body chain
            stream.CombineBodies = true;

            ObjectIdCollection ids = PickObjectSet();
            if (ids.Count == 0)
            {
                editor.WriteMessage("No object is picked\n");
                return;
            }

            TransactionManager tm = db.TransactionManager;
            using (Transaction trans = tm.StartTransaction())
            {
                stream.PushDisplayParameters(DictionaryDisplayConfiguration.GetStandardDisplayConfiguration(db), trans);

                foreach (ObjectId id in ids)
                {
                    Entity entity = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    stream.Stream(entity);
                }

                stream.PopDisplayParameters();
                trans.Commit();
            }

            ids = stream.MaterialIds;
            int color = 1;
            foreach (ObjectId id in ids)
            {
                AecModeler.Body body = stream.GetBody(id);
                HighlightBody(body, color++);
            }
        }
    }
}
