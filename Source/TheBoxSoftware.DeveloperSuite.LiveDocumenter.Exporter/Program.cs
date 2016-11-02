﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace TheBoxSoftware.Exporter 
{
	internal class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
		static void Main(string[] args) {
            Program p = new Program();

			bool printHelp = false;
			Configuration configuration = null;
            bool verbose = false;

            Console.WriteLine(string.Empty); // always start hte output with a new line clearing from the command data

			// read all the arguments
			if (args == null || args.Length == 0) {
				printHelp = true;
			}
			else {
                string configFile;

                p.ReadArguments(args, out configFile, out verbose, out printHelp);

				if (!printHelp) {
                    if (string.IsNullOrEmpty(configFile))
                    {
                        Logger.Log("No configuration file was provided.\n", LogType.Error);
                    }
                    else if (File.Exists(configFile))
                    {
                        try
                        {
                            configuration = Configuration.Deserialize(configFile);

                            // if no filters are defined, default to Public/Protected
                            if (configuration.Filters == null || configuration.Filters.Count == 0)
                            {
                                configuration.Filters.Add(Reflection.Visibility.Public);
                                configuration.Filters.Add(Reflection.Visibility.Protected);
                            }
                        }
                        catch (InvalidOperationException e)
                        {
                            Logger.Log(string.Format("There was an error reading the configuration file\n  {0}", e.Message), LogType.Error);
                            return; // bail we have no configuration or some of it is missing
                        }
                    }
                    else
                    {
                        Logger.Log(string.Format("The config file '{0}' does not exist", configFile), LogType.Error);
                    }
				}
			}

			if (printHelp) {
				p.PrintHelp();
			}
			else if(configuration != null) {
				if (configuration.IsValid()) {
                    Logger.Init(verbose);
                    p.PrintVersionInformation();

					Exporter exporter = new Exporter(configuration, verbose);
					exporter.Export();
				}
			}

            Console.WriteLine(); // space at end of outpuut for readability
		}

        /// <summary>
        /// Reads the arguments from the command line.
        /// </summary>
        /// <param name="args">The arguments provided by the user.</param>
        /// <param name="configuration">The configuration file to be processed.</param>
        /// <param name="verbose">Indicates if the output should be verbose or not.</param>
        /// <remarks>
        /// <para>The command line takes the following arguments:</para>
        /// <list type="">
        ///     <item>-h show help</item>
        ///     <item>-v verbose output</item>
        ///     <item>[file] configuration file</item>
        /// </list>
        /// </remarks>
        private void ReadArguments(string[] args, out string configuration, out bool verbose, out bool showHelp)
        {
            List<string> arguments = new List<string>(args);

            // pre the output variables
            configuration = string.Empty;
            verbose = false;
            showHelp = false;

            foreach (string modifier in arguments)
            {
                switch (modifier)
                {
                    case "-h":
                    case "help":
                    case "?":
                        showHelp = true;
                        break;
                    case "-v":
                        verbose = true;
                        break;
                }
            }

            // get the details of the configuration file.
            if(arguments.Count > 0)
            {
                string lastItem = arguments[arguments.Count - 1];
                if (!lastItem.StartsWith("-"))
                {
                    configuration = lastItem;
                }
            }
        }

		/// <summary>
		/// Outputs the help information
		/// </summary>
		private void PrintHelp() {
            this.PrintVersionInformation();

            string help =
                "\nThe exporter takes the following arguments\n" +
                "   exporter [modifiers] <filename>\n\n" +
                "   modifiers:\n" +
                "     -h        show help information\n" +
                "     -v        show verbose export details\n\n" +
                "   <filename>  The path to the configuration xml file.\n";
            Logger.Log(help);
		}

        /// <summary>
        /// Prints the exporters current version and details to the console.
        /// </summary>
        private void PrintVersionInformation()
        {
            // get version information
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            Logger.Verbose(string.Format("Live Documenter Exporter Version: {0}\n\n", fvi.ProductVersion));
        }
	}
}
