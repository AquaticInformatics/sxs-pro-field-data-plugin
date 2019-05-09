# SxS Pro Field Data Plugin

[![Build status](https://ci.appveyor.com/api/projects/status/esuabwytd0w2wvkr/branch/master?svg=true)](https://ci.appveyor.com/project/SystemsAdministrator/sxs-pro-field-data-plugin/branch/master)

An AQTS field data plugin for AQTS 2018.2-or-newer systems, which can read discharge summary XML measurements from Teledyne SxS Pro software.

## Want to install this plugin?

- Download the latest release of the plugin [here](../../releases/latest)
- Install it using the [FieldVisitPluginTool](https://github.com/AquaticInformatics/aquarius-field-data-framework/tree/master/src/FieldDataPluginTool)

## Requirements for building the plugin from source

- Requires Visual Studio 2017 (Community Edition is fine)
- .NET 4.7 runtime

## Building the plugin

- Load the `src\SxSProPlugin.sln` file in Visual Studio and build the `Release` configuration.
- The `src\SxSPro\deploy\Release\SxSPro.plugin` file can then be installed on your AQTS app server.

## Testing the plugin within Visual Studio

Use the included `PluginTester.exe` tool from the `Aquarius.FieldDataFramework` package to test your plugin logic on the sample files.

1. Open the HydstraGauging project's **Properties** page
2. Select the **Debug** tab
3. Select **Start external program:** as the start action and browse to `"src\packages\Aquarius.FieldDataFramework.18.4.2\tools\PluginTester.exe`
4. Enter the **Command line arguments:** to launch your plugin

```
/Plugin=SxSPro.dll /Json=AppendedResults.json /Data=..\..\..\..\data\SxSProDischargeSummary.xml
```

The `/Plugin=` argument can be the filename of your plugin assembly, without any folder. The default working directory for a start action is the bin folder containing your plugin.

5. Set a breakpoint in the plugin's `ParseFile()` methods.
6. Select your plugin project in Solution Explorer and select **"Debug | Start new instance"**
7. Now you're debugging your plugin!

See the [PluginTester](https://github.com/AquaticInformatics/aquarius-field-data-framework/tree/master/src/PluginTester) documentation for more details.

## Installation of the plugin

Use the [FieldDataPluginTool](https://github.com/AquaticInformatics/aquarius-field-data-framework/tree/master/src/FieldDataPluginTool) to install the plugin on your AQTS app server.
