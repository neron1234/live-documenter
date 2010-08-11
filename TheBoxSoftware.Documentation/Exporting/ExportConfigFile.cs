﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace TheBoxSoftware.Documentation.Exporting {
	using Ionic.Zip;

	/// <summary>
	/// Reads a file that contains all of the information needed to perform an export.
	/// </summary>
	/// <remarks>
	/// This file is a zip file which contains the following files:
	/// <list type="">
	///		<item>export.config [required] describes the main details of the export</item>
	/// </list>
	/// </remarks>
	public class ExportConfigFile {
		/// <summary>
		/// Initializes a new instance of the <see cref="ExportConfigFile"/> class.
		/// </summary>
		/// <param name="file">The file.</param>
		private ExportConfigFile(string filename) {
			this.ConfigFile = filename;
			using (ZipFile file = new ZipFile(filename)) {
				// get the config file
				XmlDocument doc = new XmlDocument();
				Stream ms = new MemoryStream();
				file["export.config"].Extract(ms);
				ms.Seek(0, SeekOrigin.Begin);
				doc.LoadXml(new StreamReader(ms).ReadToEnd());

				this.Name = doc.SelectSingleNode("/export/name").InnerText;
				this.Exporter = this.UnpackExporter(doc.SelectSingleNode("/export/exporter").InnerText);
			}
		}

		/// <summary>
		/// The full filename and path of the config file
		/// </summary>
		/// <value>The conig file.</value>
		protected string ConfigFile { get; set; }

		/// <summary>
		/// Gets the display name of this export configuration.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// The exporter to be used as part of the export for this export configuration.
		/// </summary>
		/// <value>The exporter.</value>
		public Exporters Exporter { get; set; }

		/// <summary>
		/// Gets the XSLT file from the export configuration file.
		/// </summary>
		/// <returns></returns>
		public virtual Stream GetXslt() {
			using (ZipFile file = new ZipFile(this.ConfigFile)) {
				// get the config file
				XmlDocument doc = new XmlDocument();
				Stream ms = new MemoryStream();
				file["export.config"].Extract(ms);
				ms.Seek(0, SeekOrigin.Begin);
				doc.LoadXml(new StreamReader(ms).ReadToEnd());

				string xslt = doc.SelectSingleNode("/export/xslt").InnerText;
				MemoryStream xsltStream = new MemoryStream();
				file[xslt].Extract(xsltStream);
				xsltStream.Seek(0, SeekOrigin.Begin);
				return xsltStream;
			}
		}

		/// <summary>
		/// Saves all of the registered output files in the export configuration file
		/// to the specified location.
		/// </summary>
		/// <param name="location">The location.</param>
		public virtual void SaveOutputFilesTo(string location) {
			using (ZipFile file = new ZipFile(this.ConfigFile)) {
				// get the config file
				XmlDocument doc = new XmlDocument();
				Stream ms = new MemoryStream();
				file["export.config"].Extract(ms);
				ms.Seek(0, SeekOrigin.Begin);
				doc.LoadXml(new StreamReader(ms).ReadToEnd());

				XmlNodeList files = doc.SelectNodes("/export/outputfiles/file");
				foreach (XmlNode current in files) {
					string from = current.Attributes["internal"] == null ? string.Empty : current.Attributes["internal"].Value;
					string to = current.Attributes["output"] == null ? string.Empty : current.Attributes["output"].Value;

					if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) {
						continue;
					}

					if (file[from] == null || file[from].IsDirectory) {
						continue;
					}

					file[from].Extract(location);
				}
			}
		}

		private Exporters UnpackExporter(string value) {
			switch (value) {
				case "web":
					return Exporters.Website;
				default:
					return Exporters.Website;
			}
		}

		public static ExportConfigFile Create(string filename) {
			return new ExportConfigFile(filename);
		}
	}
}