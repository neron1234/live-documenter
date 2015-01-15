using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheBoxSoftware.Documentation
{
    /// <summary>
    /// Represents a single build configuration.
    /// </summary>
    /// <remarks>
    /// A build configuration is a named configuration of how and where a project gets
    /// built.
    /// </remarks>
    public sealed class BuildConfiguration
    {
        private string name;
        private string outputPath;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string OutputPath
        {
            get { return outputPath; }
            set { outputPath = value; }
        }
    }
}
