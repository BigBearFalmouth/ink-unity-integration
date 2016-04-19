using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Debug = UnityEngine.Debug;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ink.UnityIntegration {
	// Helper class for ink files that maintains INCLUDE connections between ink files
	[System.Serializable]
	public class InkFile {
		
		private const string includeKey = "INCLUDE ";

		// The full file path
		public string absoluteFilePath;
		public string absoluteFolderPath;
		// The file path relative to the Assets folder
		public string filePath;

		// The content of the .ink file
		public string fileContents;
		// The paths of the files included by this file
		public List<string> includePaths;
		// The loaded files included by this file
//		[System.NonSerialized]
		public List<InkFile> includes {
			get {
				List<InkFile> _includes = new List<InkFile>();
				foreach (InkFile inkFile in InkLibrary.inkLibrary) {
					if(inkFile.master == this && includePaths.Contains(inkFile.absoluteFilePath)) {
						_includes.Add(inkFile);
					}
				}
				return _includes;
			}
		}
		// If this file is included by another, the other is the master file.

		public InkFile master;

		// A reference to the ink file (UnityEngine.DefaultAsset)
		public UnityEngine.Object inkFile;
		// The compiled json file. Use this to start a story.
		public TextAsset jsonAsset;

		public bool lastCompileFailed = false;
		public List<string> errors = new List<string>();
		public List<string> warnings = new List<string>();
		public List<string> todos = new List<string>();

		public InkFile (string absoluteFilePath) {
			this.absoluteFilePath = absoluteFilePath;
			absoluteFolderPath = Path.GetDirectoryName(absoluteFilePath);
			filePath = absoluteFilePath.Substring(Application.dataPath.Length-6);
			fileContents = File.OpenText(absoluteFilePath).ReadToEnd();
			inkFile = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
			GetIncludePaths();
		}
		
		private void GetIncludePaths() {
			if (String.IsNullOrEmpty(includeKey))
				throw new ArgumentException("the string to find may not be empty", "value");
			includePaths = new List<string>();
			for (int index = 0;;) {
				index = fileContents.IndexOf(includeKey, index);
				if (index == -1)
					return;
				
				int lastIndex = fileContents.IndexOf("\n", index);
				if(lastIndex == -1) {
					index += includeKey.Length;
				} else {
					includePaths.Add(Path.Combine(absoluteFolderPath, fileContents.Substring(index + includeKey.Length, lastIndex - (index+ + includeKey.Length))));
					index = lastIndex;
				}
			}
		}
		
		// Finds include files from paths and the list of all the ink files to check.
		public void GetIncludes (InkFile[] inkFiles) {
//			includes = new List<InkFile>();
			foreach (InkFile inkFile in inkFiles) {
				if(includePaths.Contains(inkFile.absoluteFilePath)) {
					inkFile.master = this;
//					includes.Add(inkFile);
				}
			}
		}

		public void FindCompiledJSONAsset () {
			jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath))+".json");
		}
	}
}