# SxS Pro Field Data Plugin

[![Build status](https://ci.appveyor.com/api/projects/status/esuabwytd0w2wvkr/branch/master?svg=true)](https://ci.appveyor.com/project/SystemsAdministrator/sxs-pro-field-data-plugin/branch/master)

An AQTS field data plugin for AQTS 2019.2-or-newer systems, which can read discharge summary XML measurements from Teledyne SxS Pro software.

## Want to install this plugin?

- Install the plugin using the System Config page on your AQTS app server.

### Plugin Compatibility Matrix

Choose the appropriate version of the plugin for your AQTS app server.

| AQTS Version | Latest compatible plugin Version |
| --- | --- |
| AQTS 2021.4+<br/>...<br/>AQTS 2020.3 | [v21.1.2](https://github.com/AquaticInformatics/sxs-pro-field-data-plugin/releases/download/v21.1.2/SxSPro.plugin) |
| AQTS 2020.2 | [v20.2.1](https://github.com/AquaticInformatics/sxs-pro-field-data-plugin/releases/download/v20.2.1/SxSPro.plugin) |
| AQTS 2020.1<br/>AQTS 2019.4<br/>AQTS 2019.3<br/>AQTS 2019.2 | [v19.2.1](https://github.com/AquaticInformatics/sxs-pro-field-data-plugin/releases/download/v19.2.1/SxSPro.plugin) |

## Requirements for building the plugin from source

- Requires Visual Studio 2017 (Community Edition is fine)
- .NET 4.7 runtime

## Configuring the plugin

The format of SxS Pro XML files consumed by this plugin can vary from based on the SxS Pro user's regional settings for times and dates.

The SxS Pro software will generate different XML for US users vs. UK users.

Consider April Fool's Day, 2019 (an example date which ironically exposes the foolish bug).

US XML files will contain `<Date>4/1/2019</Date>`, but XML files saved in Great Britain wil contain `<Date>01/04/2019</Date>`.

Since the plugin runs on the AQTS server, it does not know which date format to expect, so the plugin must be configured with what to expect.

Should it prefer month/day/year (US-style) or day/month/year (UK-style)?

See the [Configuration page](src/SxSPro/Readme.md) for details.

## Building the plugin

- Load the `src\SxSProPlugin.sln` file in Visual Studio and build the `Release` configuration.
- The `src\SxSPro\deploy\Release\SxSPro.plugin` file can then be installed on your AQTS app server.

## Testing the plugin within Visual Studio

Use the included `PluginTester.exe` tool from the `Aquarius.FieldDataFramework` package to test your plugin logic on the sample files.

1. Open the SxSPro project's **Properties** page
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
