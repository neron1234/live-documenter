﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheBoxSoftware.Reflection.Core {
	using TheBoxSoftware.Reflection.Core.PE;
	using TheBoxSoftware.Reflection.Core.COFF;

	public class Directory {
		public Directory() {
		}

		#region Methods
		/// <summary>
		/// Factory method for instantiating different directories from the file
		/// </summary>
		/// <param name="directory">The type of directory to create</param>
		/// <param name="fileContents">The contents of the file being read</param>
		/// <param name="address">The address of the directory</param>
		/// <returns></returns>
		public static Directory Create(DataDirectories directory, byte[] fileContents, int address) {
			Directory createdDirectory = null;

			switch (directory) {
				case DataDirectories.CommonLanguageRuntimeHeader:
					createdDirectory = new CLRDirectory(fileContents, address);
					break;

				default:
					createdDirectory = new Directory();
					break;
			}

			createdDirectory.Name = directory.ToString();

			return createdDirectory;
		}

		public virtual void ReadDirectories(PeCoffFile containingFile) {
		}
		#endregion

		#region Properties
		/// <summary>
		/// The name of the directory
		/// </summary>
		public string Name {
			get;
			set;
		}
		#endregion
	}
}