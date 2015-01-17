using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using TheBoxSoftware.Documentation.FileParsers;

namespace TheBoxSoftware.Documentation 
{
	/// <summary>
	/// Class that reads solutions, projects and libraries and converts them in to
	/// DocumentedAssembly lists.
	/// </summary>
	/// <remarks>
	/// <see cref="Document"/>s are collections of DocumentedAssembly files, this class
	/// parses input files and returns the correctly instantiated DocumentAssembly instances.
	/// </remarks>
	/// <example>
	/// The filename and build configuration are required.
	/// <code>
	/// List&lt;DocumentedAssembly&gt; assemblies = InputFileReader.Read(
	///     "c:\projects\mysolution.sln", "Debug"
	///     );
	/// Document doc = new Document(assemblies);
	/// </code>
	/// </example>
	/// <seealso cref="Document"/>
	/// <seealso cref="DocumentedAssembly"/>
	public static class InputFileReader 
    {
		/// <summary>
		/// Reads and parses the file and returns all of the associated library
		/// references
		/// </summary>
		/// <param name="fileName">The filename to read</param>
		/// <returns>An array of <see cref="DocumentedAssembly"/> instances that
		/// represent the assemblies to be documented by the application.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the <paramref name="fileName"/> provided is null or an
		/// empty string.
		/// </exception>
        /// <exception cref="ArgumentException">The filename is not an accepted extension</exception>
        public static List<DocumentedAssembly> Read(string fileName, string buildConfiguration)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            List<DocumentedAssembly> files = null;
            FileReader reader = null;

            switch (Path.GetExtension(fileName).ToLower())
            {
                case ".sln":
                    reader = new SolutionFileReader(fileName);
                    break;

                case ".csproj":
                case ".vbproj":
                case ".vcproj":
                    reader = ProjectFileReader.Create(fileName);
                    break;

                case ".dll":
                case ".exe":
                    reader = new LibraryFileReader(fileName);
                    break;
                default:
                    throw new ArgumentException("Provided filename is for a non valid file type", fileName);
            }

            reader.BuildConfiguration = string.IsNullOrEmpty(buildConfiguration) ? "Debug" : buildConfiguration;

            files = reader.Read();
            return files;
        }
	}
}
