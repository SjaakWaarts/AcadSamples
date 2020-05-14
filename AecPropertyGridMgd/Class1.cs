#region Copyright
//      .NET Sample
//
//      Copyright (c) 2005 by Autodesk, Inc.
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
//
#endregion

#region Namepaces
using System;
using System.ComponentModel;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

using DBTransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
#endregion

[assembly: ExtensionApplication(typeof(Class1))]
[assembly: CommandClass(typeof(Class1))]

public class Class1 : IExtensionApplication
{
	#region IExtensionApplication
	public void Initialize()
	{
		Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        ed.WriteMessage("Loading AecPropertyGridMgd...\r\n");
	}

	public void Terminate()
	{
	}
	#endregion

	#region Command
	[Autodesk.AutoCAD.Runtime.CommandMethod("AecPropertyGridMgd", "PropertyGrid", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
	public void PropertyGrid()
	{
		Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        PromptSelectionOptions options = new PromptSelectionOptions();
        options.SingleOnly = true;
        options.MessageForAdding = "Pick an entity: ";

        PromptSelectionResult res1 = ed.GetSelection(options);

		if (res1.Status != PromptStatus.OK)
			return;

		ObjectId[] selSet = res1.Value.GetObjectIds();

		Database db = Application.DocumentManager.MdiActiveDocument.Database;
		DBTransactionManager tm = db.TransactionManager;
		Transaction trans = tm.StartTransaction();

		System.Windows.Forms.DialogResult res2 = System.Windows.Forms.DialogResult.None;

		try
		{
			Object obj = tm.GetObject(selSet[0], OpenMode.ForWrite, false, false);

			ObjectId styleId;
			try
			{
				Object obj2 = obj.GetType().InvokeMember("StyleId",System.Reflection.BindingFlags.GetProperty,null,obj,null);
				if (!(obj2 is ObjectId))
					return;
				
				styleId = (ObjectId)obj2;
			}
			catch
			{
				styleId = ObjectId.Null;
			}
            
			if (!styleId.IsNull)
			{
				PromptResult res3 = ed.GetString("Option [Object/Style]: ");
				if (res3.StringResult.StartsWith("S") || res3.StringResult.StartsWith("s"))
					obj = tm.GetObject(styleId, OpenMode.ForWrite, false, false);
			}

#if DEBUG_CM 
			// check for Aec properties that are not categorized
			CheckCategories(obj);
#endif

			Form1 form = new Form1();
			form.SetObjects(obj);

			res2 = form.ShowDialog();

            // free up the object owned by the property grid
            form.ResetObjects();
		}
		catch
		{
			trans.Abort();
			trans.Dispose();
		}
		finally
		{
			if (res2 == System.Windows.Forms.DialogResult.OK)
				trans.Commit();

			trans.Dispose();

		}
	}
	#endregion

	#region CheckCategories
	private void CheckCategories(Object obj)
	{	
		String results = "";
				
		PropertyDescriptorCollection props = TypeDescriptor.GetProperties(obj);
		foreach (PropertyDescriptor prop in props)
		{
			if (prop.Category == "Misc")
			{
				if (prop.ComponentType.Namespace.StartsWith("Autodesk.Aec"))
					results = results + prop.Name + " (" + prop.ComponentType.Name + ")" + "\r\n";
			}
		}

		if (results != "")
			System.Windows.Forms.MessageBox.Show(results, "Category missing!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
	}
	#endregion
}
