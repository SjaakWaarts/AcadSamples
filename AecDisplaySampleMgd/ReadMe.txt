C# Sample

This sample demonstrates the use of the Display system in AutoCAD Architecture using the .NET API.

Getting Started

Load the project file AecDisplaySampleMgd.csproj in Microsoft Visual Studio 2008. You can build the Release and/or the Debug configuration.  You can use the AutoCAD NETLOAD command to load the resulting assembly from the program folder. 

Usage

The extension application implements a command called "DisplaySample". 

References

This sample references the following AutoCAD Architecture .NET assemblies:

  AutoCAD .NET manager wrapper (acmgd.dll )
  ObjectDBX .NET Managed wrapper (acdbmgd.dll)
  AEC Base .NET managed wrapper (AecBaseMgd.dll)
  AEC Arch .NET managed wrapper (AecArchMgd.dll)

If you installed the product to a path other than the default, you need to remove and re-add the AutoCAD .NET assembly references using the "Browse..." button on the "Add Reference" dialog. You will also need to adjust the paths in the registry file.


