﻿using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using FieldDataPluginFramework;
using SxSPro.Schema;

namespace SxSPro
{
    public class SectionBySectionSerializer
    {
        public static XmlRoot DeserializeNoThrow(Stream stream, ILog logger)
        {
            try
            {
                using (var streamReader = new StreamReader(stream, Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true))
                {
                    var xmlString = streamReader.ReadToEnd();
                    stream.Position = 0;

                    var serializer = new XmlSerializer(typeof(XmlRoot));
                    var memoryStream = new MemoryStream((new UTF8Encoding()).GetBytes(xmlString));

                    return serializer.Deserialize(memoryStream) as XmlRoot;
                }
            }
            catch (Exception exception)
            {
                logger.Error($"Deserialization failed:{exception}");
                return null;
            }
        }
    }
}
