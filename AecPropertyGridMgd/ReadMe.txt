C# Sample

This sample demonstrates the use of the .NET support in AutoCAD Architecture:

- uses the ACA .NET API modules AecBaseMgd, AecArchMgd and AecStructureMgd
- verifes that the ACA .NET API modules are demand loaded
- makes use of the category attribute supported by all ACA .NET objects
- uses the property grid to view/modify object properties

Getting Started

Load the project file AecMgdPropertyGrid.csproj in Microsoft Visual Studio 2008. You can build the Release and/or the Debug configuration. Use the registration file to automatically load the assembly at program start. You can also use the AutoCAD NETLOAD command to load the resulting assembly from the program folder. The extension application implements a command called "PropertyGrid".

References

This sample uses references to the AutoCAD .NET assemblies:

  AutoCAD .NET manager wrapper (acmgd.dll )
  ObjectDBX .NET Managed wrapper (acdbmgd.dll)

No references for ACA .NET assemblies are required. The appropriate assembly is determined by registry keys and loaded at runtime. 


If you installed the product to a path other than the default, you need to remove and re-add the AutoCAD .NET assembly references using the "Browse..." button on the "Add Reference" dialog. You will also need to adjust the paths in the registry file.

NETLOAD command

This is a new feature for AutoCAD 2005. After an assembly has been loaded, you can use the "ARX C " command to display the available commands in the loaded assembly. Note that there is also script compatible version of the command, "_NETLOAD".

