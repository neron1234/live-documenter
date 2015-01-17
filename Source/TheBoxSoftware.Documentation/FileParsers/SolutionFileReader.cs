using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TheBoxSoftware.Documentation.FileParsers
{
    /// <summary>
    /// Reads a solution and returns the solution and its associated projects
    /// for referenced libraries and returns them.
    /// </summary>
    internal class SolutionFileReader : FileReader
    {
        private const string VersionPattern = @"Microsoft Visual Studio Solution File, Format Version ([\d\.]*)";
        private const string V10ProjectPattern = "Project.*\\\".*\\\".*\\\".*\\\".*\\\"(.*)\\\".*\\\".*\\\"";
        private string[] ValidExtensions = new string[] { ".csproj", ".vbproj", ".vcproj" };

        /// <summary>
        /// Initialises a new instance of the SolutionFileReader class.
        /// </summary>
        /// <param name="fileName">The filenname and path for the solution</param>
        public SolutionFileReader(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// Reads the contents of the solution and returns all of the solutions project
        /// file output assemblies.
        /// </summary>
        /// <returns>An array of assemblies output by the solution and its projects.</returns>
        public override List<DocumentedAssembly> Read()
        {
            string solutionFile = File.ReadAllText(this.FileName);
            List<string> projectFiles = new List<string>();
            List<DocumentedAssembly> references = new List<DocumentedAssembly>();

            // Find the version number
            Match versionMatch = Regex.Match(solutionFile, VersionPattern);

            // Find all the project files
            MatchCollection projectFileMatches = Regex.Matches(solutionFile, V10ProjectPattern);
            foreach (Match current in projectFileMatches)
            {
                if (current.Groups.Count == 2)
                {
                    string projectFile = current.Groups[1].Value;
                    if (ValidExtensions.Contains(System.IO.Path.GetExtension(projectFile)))
                    {
                        projectFiles.Add(projectFile);
                    }
                }
            }

            // for each project read all the references
            foreach (string project in projectFiles)
            {
                string fullProjectPath = System.IO.Path.GetDirectoryName(this.FileName) + "\\" + project;
                if (System.IO.File.Exists(fullProjectPath))
                {
                    ProjectFileReader reader = ProjectFileReader.Create(fullProjectPath);
                    reader.BuildConfiguration = this.BuildConfiguration;
                    references.AddRange(reader.Read());
                }
            }

            return references;
        }

        #region Properties
        /// <summary>
        /// The visual studio solution file version
        /// </summary>
        public string Version
        {
            get;
            set;
        }
        #endregion
    }
}
