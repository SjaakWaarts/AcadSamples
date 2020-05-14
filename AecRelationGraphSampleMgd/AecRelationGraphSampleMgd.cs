/////////////////////////////////////////////////////////////////////////////
//
// $Header:  $
// $Author:  $
//
// (C) Copyright 2005 Autodesk, Inc. All rights reserved.
//
//                     ****  CONFIDENTIAL MATERIAL  ****
//
// The information contained herein is confidential, proprietary to
// Autodesk, Inc., and considered a trade secret.  Use of this information
// by anyone other than authorized employees of Autodesk, Inc. is granted
// only under a written nondisclosure agreement, expressly prescribing
// the scope and manner of such use.
//
/////////////////////////////////////////////////////////////////////////////

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

[assembly: ExtensionApplication(typeof(ACASample.RelationGraphSample))]
[assembly: CommandClass(typeof(ACASample.RelationGraphSample))]

namespace ACASample
{
    #region RelationGraph Sample
    public class RelationGraphSample : IExtensionApplication
    {

        #region IExtensionApplication Implementations
        /// <summary>
        /// Initialization.
        /// </summary>
        public void Initialize()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Loading AEC Object Relationship Sample...\r\n");
            ed.WriteMessage("Type \"RelationshipSample\" to run the sample:\r\n");

        }

        /// <summary>
        /// Terminate.
        /// </summary>
        public void Terminate()
        {
        }
        #endregion

        #region Commands Support
        /// <summary>
        /// Commands support.
        /// </summary>
        [Autodesk.AutoCAD.Runtime.CommandMethod("RelationshipSample", "RelationshipSample", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
        public void Sample()
        {
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage("\n====================================================\n");
            editor.WriteMessage("Welcome to Relationship Sample!\n");
            editor.WriteMessage("These tools allow you to see AEC relationships from a global perspective.\n");
            editor.WriteMessage("This sample uses Aec entity to determine relationships."); 
            
            PromptKeywordOptions options = new PromptKeywordOptions("Please select a sub command or escape to quit:");
            options.Keywords.Add("FwdRefsWithSelection");
            options.Keywords.Add("BwdRefsWithSelection");
            options.Keywords.Add("AllRefsWithSelection");
            options.Keywords.Add("RecursiveBwdRefsWithSeletion");

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
        void CommandHandler(string cmd)
        {
            switch (cmd)
            {
                case "FwdRefsWithSelection":
                    ReferencesFromOfSelectedObject();
                    break;

                case "BwdRefsWithSelection":
                    ReferencesToOfSelectedObject();
                    break;

                case "AllRefsWithSelection":
                    AllReferencesOfSelectedObject();
                    break;

                case "RecursiveBwdRefsWithSeletion":
                    RecursiveReferencesToOfSeletedObject();
                    break;

                default:
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n* Unknown Keyword *\n");
                    break;
            }
        }



        void ReferencesFromOfSelectedObject()
        {
            //
            // Transaction processing
            //
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TransactionManager tm = db.TransactionManager;
            Transaction trans = tm.StartTransaction();

            try
            {
                PromptEntityOptions entopts = new PromptEntityOptions("Select an Aec Entity ");
                entopts.SetRejectMessage("Must select an Aec Entity, please!");
                entopts.AddAllowedClass(typeof(Autodesk.Aec.DatabaseServices.Entity), false);
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
                    Autodesk.Aec.DatabaseServices.Entity aecent = trans.GetObject(entId, OpenMode.ForRead, false) as Autodesk.Aec.DatabaseServices.Entity;

                    DBObjectRelationshipManager mgr = new DBObjectRelationshipManager();
                    DBObjectRelationshipCollection refsFrom = mgr.GetReferencesFromThisObject(aecent);

                    ed.WriteMessage("\n\n");
                    foreach (DBObjectRelationship relFrom in refsFrom)
                    {
                        ed.WriteMessage("Entity ID = " + entId.ToString() + "  " + entId.ObjectClass.Name.ToString() + "\n references: " + relFrom.Id.ToString() + "("
                            + relFrom.Id.ObjectClass.Name + ")" + "\n whose relationship type is "
                            + relFrom.RelationshipType.ToString() + "\n");
                    }
                }

                trans.Commit();
            }
            catch (System.Exception e)
            {
                trans.Abort();
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.Message);
            }
            finally
            {
                trans.Dispose();
            }
        }

        void ReferencesToOfSelectedObject()
        {
            //
            // Transaction processing
            //
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TransactionManager tm = db.TransactionManager;
            Transaction trans = tm.StartTransaction();

            try
            {
                PromptEntityOptions entopts = new PromptEntityOptions("Select an Aec Entity ");
                entopts.SetRejectMessage("Must select an Aec Entity, please!");
                entopts.AddAllowedClass(typeof(Autodesk.Aec.DatabaseServices.Entity), false);
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

                    Autodesk.Aec.DatabaseServices.Entity aecEnt = tm.GetObject(entId, OpenMode.ForRead, false) as Autodesk.Aec.DatabaseServices.Entity;

                    DBObjectRelationshipManager mgr = new DBObjectRelationshipManager();
                    DBObjectRelationshipCollection refsTo = mgr.GetReferencesToThisObject(aecEnt);

                    ed.WriteMessage("\n\n");
                    foreach (DBObjectRelationship relTo in refsTo)
                    {
                        ed.WriteMessage("Entity ID = " + entId.ToString() + "  " + entId.ObjectClass.Name.ToString() + "\n is referenced by: " + relTo.Id.ToString() + "("
                            + relTo.Id.ObjectClass.Name + ")" + "\n whose relationship type is "
                            + relTo.RelationshipType.ToString() + "\n");
                    }
                }

                trans.Commit();
            }
            catch (System.Exception e)
            {
                trans.Abort();
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.Message);
            }
            finally
            {
                trans.Dispose();
            }
        }

        void AllReferencesOfSelectedObject()
        {
            //
            // Transaction processing
            //
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TransactionManager tm = db.TransactionManager;
            Transaction trans = tm.StartTransaction();

            try
            {
                PromptEntityOptions entopts = new PromptEntityOptions("Select an Aec Entity ");
                entopts.SetRejectMessage("Must select an Aec Entity, please!");
                entopts.AddAllowedClass(typeof(Autodesk.Aec.DatabaseServices.Entity), false);
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

                    Autodesk.Aec.DatabaseServices.Entity aecent = trans.GetObject(entId, OpenMode.ForRead, false) as Autodesk.Aec.DatabaseServices.Entity;
                    DBObjectRelationshipManager mgr = new DBObjectRelationshipManager();
                    DBObjectRelationshipCollection allRefs = mgr.GetAllReferences(aecent);

                    ed.WriteMessage("\n\n Entity ID = " + entId.ToString() + "  " + entId.ObjectClass.Name.ToString() + "\n all references (from and to) are:\n");
                    foreach (DBObjectRelationship rel in allRefs)
                    {

                        ed.WriteMessage("\n " + rel.Id.ToString() + "(" + rel.Id.ObjectClass.Name + ")" + "\n whose relationship type is "
                            + rel.RelationshipType.ToString() + "\n");
                    }

                }

                trans.Commit();
            }
            catch (System.Exception e)
            {
                trans.Abort();
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.Message);
            }
            finally
            {
                trans.Dispose();
            }
        }

        void RecursiveReferencesToOfSeletedObject()
        {
            //
            // Transaction processing
            //
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TransactionManager tm = db.TransactionManager;
            Transaction trans = tm.StartTransaction();

            try
            {
                PromptEntityOptions entopts = new PromptEntityOptions("Select an Aec Entity ");
                entopts.SetRejectMessage("Must select an Aec Entity, please!");
                entopts.AddAllowedClass(typeof(Autodesk.Aec.DatabaseServices.Entity), false);
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

                    Autodesk.Aec.DatabaseServices.Entity aecent = tm.GetObject(entId, OpenMode.ForRead, false) as Autodesk.Aec.DatabaseServices.Entity;

                    DBObjectRelationshipManager mgr = new DBObjectRelationshipManager();
                    DBObjectRelationshipCollection refsTo = mgr.GetReferencesToThisObjectRecursive(aecent, 0, false);

                    ed.WriteMessage("\n\n");
                    foreach (DBObjectRelationship relTo in refsTo)
                    {
                        ed.WriteMessage("Ent ID = " + entId.ToString() + "  " + entId.ObjectClass.Name.ToString() + " is referenced recursively by: " + relTo.Id.ToString() + "("
                            + relTo.Id.ObjectClass.Name + ")" + ", whose relationship type is "
                            + relTo.RelationshipType.ToString() + "\n");
                    }
                }

                trans.Commit();
            }
            catch (System.Exception e)
            {
                trans.Abort();
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.Message);
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