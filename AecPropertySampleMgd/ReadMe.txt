C# Sample - AecPropertySampleMgd

This sample demonstrates the use of the AutoCAD Architecture .NET API support
for Property Data.


Getting Started

Load and build the project file AecPropertySampleMgd.csproj in Microsoft 
Visual Studio 2008. You can build the Release and/or the Debug configuration. 

You can use the AutoCAD NETLOAD command to load the resulting assembly from 
the program files folder. 

You can also register the sample application with the accompanying 
registration file. This will allow demand loading of the application whenever 
a command from the sample is entered. The registration can be done by simply 
double accompanying clicking the .reg file.


Usage

The extension application implements several commands as follows:

PropertyQuery - Enter the name of a property and a value. If a match is found,
the entity will be highlighted. Use the REGEN command to cancel the 
highlight. Property data on a object or from a style is supported. XREFs 
are not supported.

ListPropertySetDefByName - Finds the property set definition objectId with 
the given name.

CreatePropertySetDef - Creates a property set definition with entered name, 
a sample set of applies to filters, and samples set of property definitions.

FindPropertySetByNameOnObject - Finds the property set by its name on 
selected given object.

GetPropSetDefIdFromPropertyName - For a given Property in a PropertySet, get 
its PropertySetDefinition. This is to find out its data type and unit 
information.

GetPropertyDataByName - Gets the property data from a database resident object
based on an entered property name.

SetPropertyDataByName - Sets the property data from a database resident object 
based on an entered property name.

CreatePropSetOnDBObject - Creates a property set on a selected database 
resident object.

CreatePropSetDefAuto - Creates automatic property definitions using all 
available automatic properties for a single class.

CreatePropSetDefAutoDup - Creates automatic property definitions using 
the duplicate available automatic properties between two classes.


References

This sample references the following AutoCAD Architecture .NET assemblies:

  AutoCAD .NET manager wrapper (acmgd.dll)
  ObjectDBX .NET Managed wrapper (acdbmgd.dll)
  AEC Base .NET managed wrapper (AecBaseMgd.dll)
  AEC Property Data .NET managed wrapper (AecPropDataMgd.dll)


