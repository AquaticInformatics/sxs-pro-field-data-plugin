using System;
using System.IO;
using FieldDataPluginFramework;
using FieldDataPluginFramework.Context;
using FieldDataPluginFramework.Results;
using SxSPro.Schema;

namespace SxSPro
{
    public class Plugin : IFieldDataPlugin
    {
        public ParseFileResult ParseFile(Stream fileStream, IFieldDataResultsAppender appender, ILog logger)
        {
            var xmlRoot = SectionBySectionSerializer.DeserializeNoThrow(fileStream, logger);

            if (xmlRoot == null)
            {
                return ParseFileResult.CannotParse();
            }

            var stationNo = xmlRoot.Summary?.WinRiver_II_Section_by_Section_Summary?.Station_No;

            if (string.IsNullOrWhiteSpace(stationNo))
            {
                logger.Error("File can be parsed but there is no Station_No specified.");
                return ParseFileResult.SuccessfullyParsedButDataInvalid("Missing Station_No.");
            }
            try
            {
                var trimmedLocationIdentifier = stationNo.Trim();
                var location = appender.GetLocationByIdentifier(trimmedLocationIdentifier);

                return ParseXmlRootNoThrow(location, xmlRoot, appender, logger);

            }
            catch (Exception exception)
            {
                logger.Error($"Cannot find location with identifier {stationNo}.");
                return ParseFileResult.CannotParse(exception);
            }
        }

        private ParseFileResult ParseXmlRootNoThrow(LocationInfo location, XmlRoot xmlRoot,
            IFieldDataResultsAppender appender, ILog logger)
        {
            try
            {
                var parser = new Parser(location, appender, logger);

                parser.Parse(xmlRoot);

                return ParseFileResult.SuccessfullyParsedAndDataValid();
            }
            catch (Exception exception)
            {
                logger.Error($"Something went wrong: {exception.Message}\n{exception.StackTrace}");
                return ParseFileResult.SuccessfullyParsedButDataInvalid(exception);
            }
        }

        public ParseFileResult ParseFile(Stream fileStream, LocationInfo targetLocation,
            IFieldDataResultsAppender appender, ILog logger)
        {
            var xmlRoot = SectionBySectionSerializer.DeserializeNoThrow(fileStream, logger);

            return xmlRoot == null 
                ? ParseFileResult.CannotParse() 
                : ParseXmlRootNoThrow(targetLocation, xmlRoot, appender, logger);
        }
    }
}
