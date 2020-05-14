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

[assembly: ExtensionApplication(typeof(AecPropertyDefinitionFormulaSampleMgd.PropertyDefinitionFormulaSample))]
[assembly: CommandClass(typeof(AecPropertyDefinitionFormulaSampleMgd.PropertyDefinitionFormulaSample))]

namespace AecPropertyDefinitionFormulaSampleMgd
{
    public class PropertyDefinitionFormulaSample : IExtensionApplication
    {
        #region IExtensionApplication Members

        /// <summary>
        /// Initialization.
        /// </summary>
        public void Initialize()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Type \"PropertyDefinitionFormulaSample\" to run the sample:\r\n");
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
        [Autodesk.AutoCAD.Runtime.CommandMethod("PropertyDefinitionFormulaSample", "PropertyDefinitionFormulaSample", Autodesk.AutoCAD.Runtime.CommandFlags.Modal)]
        public void ShowSample()
        {
            Database db = GetDatabase();
            Editor ed = GetEditor();
            ed.WriteMessage("Adding a new property set definition contains a formula property to calculate wall volume.\n");
            // we need to add all the automatic properties prior to the formula property
            PropertySetDefinition psd = CreateWallPropertySetDefinition();
            // then we add the property set definition to the dictionary to make formula property work properly
            DictionaryPropertySetDefinitions dict = new DictionaryPropertySetDefinitions(db);
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                dict.AddNewRecord("SampleWallPropertySetDefinition", psd);
                trans.AddNewlyCreatedDBObject(psd, true);
                // now we can create the formula property
                PropertyDefinitionFormula formula = new PropertyDefinitionFormula();
                formula.SetToStandard(db);
                formula.SubSetDatabaseDefaults(db);
                formula.Name = "Wall Volume";
                formula.UseFormulaForDescription = true;
                // before setting formula string to the formula property, we need to make sure
                // that the property definition is added to the property set definition (which has an object id)
                psd.Definitions.Add(formula);
                // so we can set the formula string now
                formula.SetFormulaString("[Length]*[Height]*[Width]");
                // and here we change the sample values of the referenced properties
                formula.DataItems[0].Sample = 1;
                formula.DataItems[1].Sample = 2;
                formula.DataItems[2].Sample = 3;
                trans.Commit();
            }
            ed.WriteMessage("A new property set definition \"SampleWallPropertySetDefinition\" is created.\n");
            ed.WriteMessage("It contains a formula definition named \"Wall Volume\".\n");
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

        public static PropertySetDefinition CreateWallPropertySetDefinition()
        {
            Database db = GetDatabase();
            PropertySetDefinition def = new PropertySetDefinition();
            // we want the property set to apply to walls
            StringCollection appliesTo = new StringCollection();
            string wallClassName = RXObject.GetClass(typeof(Wall)).Name;
            appliesTo.Add(wallClassName);
            def.SetToStandard(db);
            def.SubSetDatabaseDefaults(db);
            def.AlternateName = "SampleWallPropertySet";
            def.IsLocked = false;
            def.IsVisible = true;
            def.IsWriteable = true;

            StringCollection properties = new StringCollection();
            properties.Add("Width");
            properties.Add("Length");
            properties.Add("Height");
            foreach (string propName in properties)
            {
                PropertyDefinition propDef = new PropertyDefinition();
                propDef.SetToStandard(db);
                propDef.SubSetDatabaseDefaults(db);
                propDef.Automatic = true;
                propDef.Name = propName;
                propDef.Description = propName;
                propDef.IsVisible = true;
                propDef.IsReadOnly = true;
                propDef.SetAutomaticData(wallClassName, propName);
                def.Definitions.Add(propDef);
            }
            return def;
        }
        #endregion
    }
}
