﻿
namespace TheBoxSoftware.Exporter
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using System.IO;

    /// <summary>
    /// Class that describes which exporters to run and how those exporters will 
    /// execute.
    /// </summary>
    [Serializable]
    [XmlRoot("configuration")]
    public class Configuration
    {
        [XmlElement("document")]
        public string Document { get; set; }

        [XmlArray("filters")]
        [XmlArrayItem("filter")]
        public List<Reflection.Visibility> Filters { get; set; }

        [XmlArray("outputs")]
        [XmlArrayItem("ldec")]
        public List<Output> Outputs { get; set; }

        public class Output
        {
            [XmlAttribute("location")]
            public string Location { get; set; }

            [XmlText()]
            public string File { get; set; }
        }

        public Configuration()
        {
            Outputs = new List<Output>();
            Filters = new List<Reflection.Visibility>();
        }

        /// <summary>
        /// Deserializes a Configuration from the <paramref name="fromFile"/>.
        /// </summary>
        /// <param name="fromFile">The file to read the project from.</param>
        /// <returns>The instantiated project.</returns>
        public static Configuration Deserialize(string fromFile)
        {
            using(Stream fs = new FileStream(fromFile, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                return (Configuration)serializer.Deserialize(fs);
            }
        }

        internal void AddOutput(string location, string ldec)
        {
            Output output = new Output();
            output.File = ldec;
            output.Location = location;
            Outputs.Add(output);
        }

        /// <summary>
        /// Checks if the configuration is valid and if not outputs the issues to the console and returns
        /// false.
        /// </summary>
        /// <returns></returns>
        internal bool IsValid(ILog log)
        {
            bool isValid = true;

            if(!File.Exists(this.Document))
            {
                log.LogError($"The document '{Document}' does not exist.");
                isValid = false;
            }

            return isValid;
        }
    }
}