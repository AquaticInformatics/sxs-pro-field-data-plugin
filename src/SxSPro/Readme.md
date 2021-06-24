# SxS Pro Field Data Plugin

How do you know if the plugin is configured correctly?

Try parsing a file measured on the 13th day of a month or later. If you are happy with the parsed visit, then your configuration is good.

Using a file with a "13" or higher for the day value will immediately detect a format problem if the expected month & day columns are swapped.

## Configuring the plugin

The format of SxS Pro XML files consumed by this plugin can vary from based on the SxS Pro user's regional settings for times and dates.

The SxS Pro software will generate different XML for US users vs. UK users.

Consider April Fool's Day, 2019 (an example date which ironically exposes the foolish bug).

US XML files will contain `<Date>4/1/2019</Date>`, but XML files saved in Great Britain wil contain `<Date>01/04/2019</Date>`.

Since the plugin runs on the AQTS server, it does not know which date format to expect, so the plugin must be configured with what to expect.

Should it prefer month/day/year (US-style) or day/month/year (UK-style)?

### Choose a standard date/time format for your agency!

Windows systems provide [quite a variety of options](https://www.basicdatepicker.com/samples/cultureinfo.aspx), so make sure your technicians know which regional settings to use when exporting XML from SxS Pro.

To make matters worse, Fred could be running on a UK version of Windows 10, which defaults to "dd/MM/yyyy" but Fred could have changed his regional settings to Czech, which uses "d. M. yyyy". Any SxS Pro XML file that Fred's computer exports would have `<Date>1. 4. 2019</Date>`.

Yeah, it's a pretty gross problem to contain. But if you need to ingest SxS measurements reliably, you will need to standardize how your files are prepared.

## How to inspect your XML file:

There are two nodes in the `<WinRiver_II_Section_by_Section_Summary>` node near the top of the file:

The `<Date>` and `<Start_End_Time>` nodes contain dates and times which will be influenced by the current Windows regional settings when the files are exported from SxS Pro software.

Here is the start of a typical SxS Pro XML file, with the relevant nodes:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<XmlRoot>
  <Summary>
    <WinRiver_II_Section_by_Section_Summary>
      <Site_Name>Logan Creek near Uehling</Site_Name>
      <Printed_Date>11/29/2019</Printed_Date>
      <Station_No>6799500</Station_No>
      <Air_Temp>-</Air_Temp>
      <River_Name></River_Name>
      <Location></Location>
      <Date>11/6/2019</Date>
      <Start_End_Time>12:44:55 PM/1:08:51 PM</Start_End_Time>
      <Party>LLG JDH JJV</Party>
      <Software_Ver>1.00</Software_Ver>
      <Units_of_Measure>English</Units_of_Measure>
      <Meas_No>3</Meas_No>
      <No_Stations>25</No_Stations>
```

In this example we can tell from `<Printed_Date>` that a "M/d/yyyy" format was used, since a month value of 29 makes no sense.

## The `Config.json` file stores the plugin's configuration

The plugin can be configured via a [`Config.json`](./Config.json) JSON document, to control the date and time formats used by your organization.

The JSON configuration is stored in different places, depending on the version of the plugin.

| Version | Configuration location |
| --- | --- |
| 20.2.x | Use the Settings page of the System Config app to change the settings.<br/><br/>**Group**: `FieldDataPluginConfig-SxSPro`<br/>**Key**: `Config`<br/>**Value**: The entire contents of the Config.json file. If blank or omitted, the plugin's default [`Config.json`](./Config.json) is used. |
| 19.2.x | Read from the `Config.json` file in the plugin folder, at `%ProgramData%\Aquatic Informatics\AQUARIUS Server\FieldDataPlugins\SxSPro\Config.json` |

This JSON document is reloaded each time a SxS file is uploaded to AQTS for parsing. Updates to the setting will take effect on the next SxS file parsed.

The JSON configuration information stores two settings:

| Property Name | Description |
| --- | --- |
| **DateFormats** | A list of date format format strings, used to parse the `<Date>` node. |
| **TimeFormats** | A list of time format format strings, used to parse the `<Start_End_Time>` node. |

The first format string to match a date or time is used.

By default, the plugin assumes US regional settings for dates and times, plus ISO 8601 for dates.

```json
{
  "DateFormats": [ "M/d/yyyy", "M-d-yyyy", "yyyy/M/d", "yyyy-M-d" ],
  "TimeFormats": [ "h:m:s tt", "h:m tt", "H:m:s", "H:m" ]
}
```

Notes:
- Editing JSON files [can be tricky](#json-editing-tips). Don't include a trailing comma after the last item in the list.
- Date format strings should only include uppercase `M` (month) patterns, not lowercase `m` (minute) patterns.
- Date format lists should not be ambiguous. Don't include both a day+month pattern and a month+day pattern.
- Time format lists should not be ambiguous. Each pattern should include both lowercase `h` (12-hour) and `tt` (AM/PM designator), or should use uppercase `H` (24-hour) and no `tt` pattern at all.

## Tips about `Format` strings:
`Format` values are [.NET custom date/time format strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).

These format strings can be rather fussy to deal with, so take care to consider some of the common edge cases:
- Format strings are case-sensitive. Common mistakes are made for month-vs-minute and 24-hour-vs-12-hour patterns.
- Uppercase ['M'](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings#M_Specifier) matches month digits, between 1 and 12.
- Lowercase ['m'](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings#mSpecifier) matches minute digits, between 0 and 59.
- Uppercase ['H'](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings#H_Specifier) matches 24-hour hour digits, between 0 and 23.
- Lowercase ['h'](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings#hSpecifier) matches 12-hour hour digits, between 1 and 12, and require a ['t' or 'tt'](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings#tSpecifier) pattern to distinguish AM from PM.
- Prefer single-character patterns when possible, since they match double-digit values as well. Eg. 'H:m' will match '2:35' and '14:35', but 'HH:mm" will not match '2:35' since the 'HH' means exactly-2-digits.

## JSON editing tips

Editing [JSON](https://json.org) can be a tricky thing.

Sometimes the plugin code can detect a poorly formatted JSON document and report a decent error, but sometimes a poorly formatted JSON document will appear to the plugin as just an empty document.

Here are some tips to help eliminate common JSON config errors:
- Edit JSON in a real text editor. Notepad is fine, [Notepad++](https://notepad-plus-plus.org/) or [Visual Studio Code](https://code.visualstudio.com/) are even better choices.
- Don't try editing JSON in Microsoft Word. Word will mess up your quotes and you'll just have a bad time.
- Try validating your JSON using the online [JSONLint validator](https://jsonlint.com/).
- Whitespace between items is ignored. Your JSON document can be single (but very long!) line, but the convention is separate items on different lines, to make the text file more readable.
- All property names must be enclosed in double-quotes (`"`). Don't use single quotes (`'`) or smart quotes (`“` or `”`), which are actually not that smart for JSON!
- Avoid a trailing comma in lists. JSON is very fussy about using commas **between** list items, but rejects lists when a trailing comma is included. Only use a comma to separate items in the middle of a list.

### Adding comments to JSON

The JSON spec doesn't support comments, which is unfortunate.

However, the code will simply skip over properties it doesn't care about, so a common trick is to add a dummy property name/value string. The code won't care or complain, and you get to keep some notes close to other special values in your custom JSON document.

Instead of this:

```json
{
  "ExpectedPropertyName": "a value",
  "AnotherExpectedProperty": 12.5 
}
```

Try this:

```json
{
  "_comment_": "Don't enter a value below 12, otherwise things break",
  "ExpectedPropertyName": "a value",
  "AnotherExpectedProperty": 12.5 
}
```

Now your JSON has a comment to help you remember why you chose the `12.5` value.
