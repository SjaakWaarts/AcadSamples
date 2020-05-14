using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.Aec.Geometry;
using Autodesk.Aec.DatabaseServices;
using Autodesk.Aec.Arch.DatabaseServices;

using Autodesk.Aec.PropertyData;
using Autodesk.Aec.PropertyData.DatabaseServices;

using DBTransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
using Dictionary = Autodesk.Aec.DatabaseServices.Dictionary;

using ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;
using ObjectIdCollection = Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection;

using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using AecEntity = Autodesk.Aec.DatabaseServices.Entity;
using AecDbObject = Autodesk.Aec.DatabaseServices.DBObject;
using AMEntity = Autodesk.Aec.Modeler.Entity;

[assembly: ExtensionApplication(typeof(AecScheduleSampleMgd.ScheduleSample))]
[assembly: CommandClass(typeof(AecScheduleSampleMgd.ScheduleSample))]

namespace AecScheduleSampleMgd
{
    public class ScheduleSample : IExtensionApplication
    {
        #region IExtensionApplication Members

        /// <summary>
        /// Initialization.
        /// </summary>
        public void Initialize()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Type \"ScheduleSample\" to run the sample:\r\n");
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
        [Autodesk.AutoCAD.Runtime.CommandMethod("ScheduleSample", "ScheduleSample", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
        public void ShowSample()
        {
            ObjectIdCollection ids = PickObjectSet("Please pick the objects to be scheduled:");
            if (ids.Count == 0)
                return;

            Dictionary<RXClass, List<ObjectId>> classDictionary = new Dictionary<RXClass, List<ObjectId>>();
            Dictionary<RXClass, List<ObjectId>> ineligibleClassDictionary = new Dictionary<RXClass, List<ObjectId>>();
            StringCollection eligibleClassNames = new StringCollection();
            eligibleClassNames.AddRange(PropertyDataServices.FindEligibleClassNames());
            foreach (ObjectId id in ids)
            {
                if (!eligibleClassNames.Contains(id.ObjectClass.Name))
                {
                    if (!ineligibleClassDictionary.ContainsKey(id.ObjectClass))
                        ineligibleClassDictionary[id.ObjectClass] = new List<ObjectId>();

                    ineligibleClassDictionary[id.ObjectClass].Add(id);
                }
                else
                {
                    if (!classDictionary.ContainsKey(id.ObjectClass))
                        classDictionary[id.ObjectClass] = new List<ObjectId>();

                    classDictionary[id.ObjectClass].Add(id);
                }
            }

            if (classDictionary.Keys.Count == 0)
            {
                GetEditor().WriteMessage("No eligible object is selected. Schedule table sample will now quit.");
                return;
            }

            UiData runtimeData = new UiData();
            runtimeData.classObjectIdsMap = classDictionary;
            runtimeData.ineligibleClassObjectIdsMap = ineligibleClassDictionary;
            WizardSheetPropertySetDefinition sheetPsd = new WizardSheetPropertySetDefinition();
            WizardSheetScheduleTableStyle sheetSts = new WizardSheetScheduleTableStyle();
            WizardSheetSummary sheetSummary = new WizardSheetSummary();
            WizardManager wizard = new WizardManager();
            wizard.AddSheet(sheetPsd);
            wizard.AddSheet(sheetSts);
            wizard.AddSheet(sheetSummary);
            wizard.RuntimeData = runtimeData;
            if (wizard.ShowWizard() == System.Windows.Forms.DialogResult.OK)
            {
                ScheduleTableCreateResult result = ScheduleTableCreateEx.CreateScheduleTable(runtimeData);
            }
        }

        #endregion

        #region HelperFunction
        static public Database GetDatabase()
        {
            return Application.DocumentManager.MdiActiveDocument.Database;
        }
        static public Editor GetEditor()
        {
            return Application.DocumentManager.MdiActiveDocument.Editor;
        }
        static public ObjectIdCollection PickObjectSet(string tips)
        {
            Editor editor = GetEditor();
            PromptSelectionOptions options = new PromptSelectionOptions();
            options.MessageForAdding = tips;
            options.MessageForRemoval = "Remove objects from selection";
            options.AllowDuplicates = false;
            options.RejectObjectsFromNonCurrentSpace = true;
            options.RejectObjectsOnLockedLayers = true;
            options.RejectPaperspaceViewport = true;
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

        // Gets the display name of an object type.
        public static string GetDisplayName(RXClass classObject)
        {
            using (RXObject rawObject = classObject.Create())
            {
                if (classObject.IsDerivedFrom(RXObject.GetClass(typeof(AecDbObject))))
                {
                    AecDbObject dbObject = (AecDbObject)rawObject;
                    return dbObject.DisplayName;
                }
                else if (classObject.IsDerivedFrom(RXObject.GetClass(typeof(AecEntity))))
                {
                    AecEntity entity = (AecEntity)rawObject;
                    return entity.DisplayName;
                }
                else if (classObject.IsDerivedFrom(RXObject.GetClass(typeof(DBObject))))
                {
                    string dxfName = classObject.DxfName;
                    if (dxfName == null)
                        return classObject.Name;
                    else
                        return dxfName;
                }
            }
            throw new ArgumentException("wrong class type");
        }
        #endregion
    }
}
