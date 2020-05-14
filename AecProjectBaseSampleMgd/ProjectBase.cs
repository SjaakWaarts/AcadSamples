#region Copyright
//      .NET ProjectBase Sample
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
using System.Collections.Specialized;
using System.Text;
using System.IO;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.Aec.DatabaseServices;
using Autodesk.Aec.Project;

using ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;
#endregion

[assembly: ExtensionApplication(typeof(AecProjectBaseSampleMgd.VFS))]
[assembly: CommandClass(typeof(AecProjectBaseSampleMgd.VFS))]

namespace AecProjectBaseSampleMgd
{
    public class VFS: IExtensionApplication
    {
        #region Data Members
        private ProjectBaseManager mgr = ProjectBaseServices.Service.ProjectManager;
        private static string testproject = @"C:\temp\Sample Project\";
        private static string name = "Sample Project";
        private static Project proj = null;
        private static Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        private static PromptKeywordOptions topOpts = new PromptKeywordOptions("Commands");
        private string currentPath = testproject;
        #endregion

        #region IExtensionApplication
        public void Initialize()
        {
            Print("Initializing Sample Project...");

            string sourceDir = Environment.GetEnvironmentVariable("USERPROFILE");
            sourceDir += @"\My Documents\Autodesk\My Projects\Sample Project 2009\";
            if (!Directory.Exists(testproject))
                CopyDir(sourceDir, testproject);

            if (proj == null)
                proj = mgr.OpenProject(OpenMode.ForRead, testproject, name);

            // top commands
            topOpts.Keywords.Add("ls");
            topOpts.Keywords.Add("cd");
            topOpts.Keywords.Add("open");
            topOpts.Keywords.Add("pwd");
            topOpts.Keywords.Add("check");
            topOpts.Keywords.Add("mode");
            topOpts.Keywords.Add("upgrade");
            topOpts.Keywords.Add("downgrade");
        }
        public void Terminate()
        {
            Print("Terminating Sample Project...");

            mgr.CloseProject(proj, false);
        }
        #endregion

        #region Print
        private void Print(string msg)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage( msg + "\n" );
        }
        #endregion

        #region CopyDir
        private void CopyDir(string src, string dest)
        {
            if (dest[dest.Length - 1] != Path.DirectorySeparatorChar)
                dest += Path.DirectorySeparatorChar;

            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);

            string[] fileList = Directory.GetFileSystemEntries(src);

            foreach (string file in fileList)
            {
                // directory, recursively copy
                if (Directory.Exists(file))
                    CopyDir(file, dest + Path.GetFileName(file));
                else // file
                {
                    File.Copy(file, dest + Path.GetFileName(file), true);
                    FileAttributes attri = File.GetAttributes(file);
                    attri = attri & (~FileAttributes.ReadOnly);
                    File.SetAttributes(dest + Path.GetFileName(file), attri);
                }
            }
        }
        #endregion

        #region Command Methods
        [Autodesk.AutoCAD.Runtime.CommandMethod("ProjectBaseSample", "ProjectSample", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
        public void Project()
        {
            while (true)
            {
                PromptResult result = ed.GetKeywords(topOpts);

                if (result.Status == PromptStatus.OK && result.StringResult != "exit")
                {
                    string cmd = result.StringResult;

                    Handler(cmd);
                }
                else
                    break;
            }

        }
        #endregion

        #region Help Functions
        private  void ListAll( )
        {

            string[] fileList = Directory.GetFileSystemEntries(currentPath);
            FileCategory category = proj.GetCategory(currentPath);
            bool isCategory = (category != FileCategory.Unknown);

            Print("1. <CATEGORY>       .");
            Print("2. <CATEGORY>       ..");

            int i = 2;
            foreach (string file in fileList)
            {
                //find category or file in category 
                FileCategory belongToCategory = proj.GetFileCategory(file);
                bool isViewed = false;
                string tag = null;

                // in category directory
                if (belongToCategory != FileCategory.Unknown)
                {
                    isViewed = true;
                    if (!Directory.Exists(file))
                    {
                        tag = ". <DWGFILE>        ";
                        ProjectFile projFile = proj.GetFile(file);
                        ProjectFileName projFileName = new ProjectFileName(file);
                        if (projFile == null || !projFile.DwgExists || projFileName.Extension.ToLower() != ".xml")
                            isViewed = false;
                    }
                    else
                    {
                        tag = ". <CATEGORY>       ";
                    }
                }

                if (isViewed)
                {
                    ++i;
                    string fileName = file.Substring(currentPath.Length);

                    if (fileName.ToLower().Contains(".xml"))
                        fileName = fileName.Substring(0, fileName.Length - 4);

                    Print(i + tag + fileName);
                }
            }
        }

        private bool IsValidFolder(string name)
        {
            string newPath = currentPath + name;

            if (newPath[newPath.Length - 1] != Path.DirectorySeparatorChar)
                newPath += Path.DirectorySeparatorChar;

            if (Directory.Exists(newPath))
                return true;
            else
                return false;
        }

        private void Update(string name)
        {
            if (currentPath[currentPath.Length - 1] != Path.DirectorySeparatorChar)
                currentPath += Path.DirectorySeparatorChar;

            currentPath += name;

            if (currentPath[currentPath.Length - 1] != Path.DirectorySeparatorChar)
                currentPath += Path.DirectorySeparatorChar;
        }

        private string GetFullPath(int index)
        {
            string returnPath = currentPath;

            // parent path
            if (index == 2)
            {
                if (returnPath != testproject)
                {
                    returnPath = returnPath.Remove(returnPath.Length - 1);
                    int pos = returnPath.LastIndexOf(Path.DirectorySeparatorChar);
                    returnPath = returnPath.Substring(0, pos + 1);
                }
            }
            // specified path
            else if (index > 2)
            {
                string[] fileList = Directory.GetFileSystemEntries(returnPath);
                int i = 2;
                foreach (string file in fileList)
                {
                    FileCategory belongToCategory = proj.GetFileCategory(file);
                    bool isViewed = false;
                    if (belongToCategory != FileCategory.Unknown)
                    {
                        isViewed = true;
                        if (!Directory.Exists(file))
                        {
                            ProjectFile projFile = proj.GetFile(file);
                            ProjectFileName projFileName = new ProjectFileName(file);
                            if (projFile == null || !projFile.DwgExists || projFileName.Extension.ToLower() != ".xml")
                                isViewed = false;
                        }
                    }

                    if (isViewed)
                    {
                        ++i;
                    }

                    if (i == index)
                    {
                        returnPath = file;
                        break;
                    }
                }
            }
            // else: current path

            if ( Directory.Exists(returnPath) && 
                 returnPath[returnPath.Length - 1] != Path.DirectorySeparatorChar)
                    returnPath += Path.DirectorySeparatorChar;

            return returnPath;
        }

        private void Open(string path)
        {

            ProjectFile file = proj.GetFile(path);

            if (file.DwgExists)
            {
                Application.DocumentManager.Open(file.DrawingFullPath, true);
            }
            else
                Print(path + " does not exist!");
        }

        private bool IsSamePath(string path1, string path2)
        {
            return (path1.ToLower() == path2.ToLower());
        }

        private Database GetDbForFile(string dwgFullPath)
        {
            DocumentCollection docs = Application.DocumentManager;
            Document doc = null;

            foreach (Document elem in docs)
            {
                if (IsSamePath(elem.Database.Filename, dwgFullPath))
                {
                    doc = elem;
                    break;
                }
            }

            if (doc != null)
            {
                return doc.Database;
            }
            else
            {
                Database db = new Database(false, true);
                db.ReadDwgFile(dwgFullPath, FileShare.Read, false, null);
                db.ResolveXrefs( false, true);

                return db;
            }
        }

        private void Check(string path)
        {
            ProjectFile file = proj.GetFile(path);

            if (file.DwgExists)
            {
                // Gets the database for the specified dwg file
                string dwgName = file.DrawingFullPath;

                Database dwgDb = GetDbForFile(dwgName);

                if (dwgDb != null)
                {
                    ObjectId symTbId = dwgDb.BlockTableId;
                    Transaction trans = dwgDb.TransactionManager.StartTransaction();

                    try
                    {
                        BlockTable bt = trans.GetObject(symTbId, OpenMode.ForRead, false) as BlockTable;
                        StringCollection refs = new StringCollection();

                        foreach (ObjectId recId in bt)
                        {
                            BlockTableRecord btr = trans.GetObject(recId, OpenMode.ForRead) as BlockTableRecord;

                            if (btr.IsFromExternalReference)
                            {
                                refs.Add(btr.Name);
                            }
                        }

                        // output referenced files
                        if (refs.Count == 0)
                        {
                            Print("<" + file.FileName + "> references no other dwg files.");
                        }
                        else
                        {
                            Print("<" + file.FileName + @"> references the following dwg files:");
                            int i = 0;
                            foreach (string refName in refs)
                            {
                                Print( ++i + ". " +refName);
                            }
                        }
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
            }
        }
        #endregion

        #region Handler
        private void Handler( string cmd )
        {
            switch (cmd)
            {
                case "ls":
                    ListAll( );
                    break;

                case "cd":
                    PromptIntegerOptions iOpt = new PromptIntegerOptions("Input Folder Number");
                    PromptIntegerResult result = ed.GetInteger(iOpt);
                    if (result.Status == PromptStatus.OK)
                    {
                        string chosenPath = GetFullPath(result.Value);
                        if (Directory.Exists(chosenPath))
                        {
                            currentPath = chosenPath;
                            break;
                        }
                    }
                    
                    Print("You have chosen an invalid folder");
                    break;

                case "open":
                    PromptIntegerOptions iOpt2 = new PromptIntegerOptions("Input File Number");
                    PromptIntegerResult result2 = ed.GetInteger(iOpt2);
                    if (result2.Status == PromptStatus.OK)
                    {
                        string chosenPath = GetFullPath(result2.Value);
                        if (!Directory.Exists(chosenPath))
                        {
                            Open(chosenPath);
                            break;
                        }
                    }

                    Print("You have chosen an invalid file");
                    break;

                case "check":
                    PromptIntegerOptions iOpt3 = new PromptIntegerOptions("Input File Number");
                    PromptIntegerResult result3 = ed.GetInteger(iOpt3);
                    if (result3.Status == PromptStatus.OK)
                    {
                        string chosenPath = GetFullPath(result3.Value);
                        if (!Directory.Exists(chosenPath))
                        {
                            Check(chosenPath);
                            break;
                        }
                    }

                    Print("You have chosen an invalid file");
                    break;

                case "pwd":
                    string wd = @"\Sample\" + currentPath.Substring(testproject.Length);
                    Print(wd);
                    break;

                case "mode":
                    if (mgr.IsProjectLockedForWrite(testproject + name + ".apj"))
                        Print("This project is opened in write mode.");
                    else
                        Print("This project is opened in read-only mode.");
                    break;

                case "upgrade":
                    if (!mgr.IsProjectLockedForWrite(testproject + name + ".apj"))
                    {
                        mgr.UpgradeOpen(proj);
                        Print("Upgraded open succesfully.");
                    }
                    else
                        Print("This project is already opened in write mode.");
                    break;

                case "downgrade":
                    if (mgr.IsProjectLockedForWrite(testproject + name + ".apj"))
                    {
                        mgr.DowngradeOpen(proj);
                        Print("Downgrade open succesfully.");
                    }
                    else
                        Print("This project is already opened in read mode.");
                    break;

                default:
                    break;
            }
        }
        #endregion
    }
}
