//
//  ScriptSetManager.cs
//
//  Author:
//       Autofire <http://www.reach-beyond.pro/>
//
//  Copyright (c) 2018 ReachBeyond
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

#pragma warning disable CS0649

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System;

namespace ReachBeyond.VariableObjects.Editor {

	/// <summary>
	/// This class provides several tools related to pre-existing files. It
	/// keeps an organized record of all known variable object files, as well
	/// as any templates.
	///
	/// Results are cached and only updated when necessary, so there is little
	/// drawback to making repeated calls to these records.
	///
	/// Note that functions which create files are not included. For that, see
	/// ReachBeyond.VariableObjects.Editor.VariableTypeBuilder.
	/// </summary>
	public static class ScriptSetManager {


#region Constants
		/// <summary>Base label used when building the other labels.</summary>
		public const string BaseLabel = "ReachBeyond.VariableObjects";

		/// <summary>Label for Unity-based VarObjs.</summary>
		public const string UnityLabel = BaseLabel + ".Unity";
		/// <summary>Label for project-specific VarObjs.</summary>
		public const string CustomLabel = BaseLabel + ".Custom";

		public const string DataHeader = "/* DO NOT REMOVE -- START VARIABLE OBJECT INFO -- DO NOT REMOVE **";
		public const string DataFooter = "** DO NOT REMOVE --  END VARIABLE OBJECT INFO  -- DO NOT REMOVE */";
#endregion

#region Variables
		/// <summary>
		/// Variable objects that support types that come packages with C# and Unity.
		/// </summary>
		private static Dictionary<string, ScriptSetInfo> _unityVarFiles;

		/// <summary>
		/// Variable objects that support types that are unique to the project.
		/// </summary>
		private static Dictionary<string, ScriptSetInfo> _customVarFiles;

		/// <summary>
		/// If true, then the assembly dictionaries are outdated; we need to refresh them before we use them again.
		/// </summary>
		private static bool assemblyDictionariesAreOutdated;
#endregion

#region Properties
		/// <summary>
		/// Gets the general purpose, Unity variable info. The keys are the human readable names that go with each
		/// Variable Object.
		/// </summary>
		/// <value>The dictionary of general Unity variable files.</value>
		public static Dictionary<string, ScriptSetInfo> UnityVarFiles {
			get {
				UpdateDictionaries();
				return new Dictionary<string, ScriptSetInfo>(_unityVarFiles);
			}
		}

		/// <summary>
		/// Gets the project-specific variable file info. The keys are the human readable names that go with each
		/// Variable Object.
		/// </summary>
		/// <value>The dictionary of project-sepcific variable files.</value>
		public static Dictionary<string, ScriptSetInfo> CustomVarFiles {
			get {
				UpdateDictionaries();
				return new Dictionary<string, ScriptSetInfo>(_customVarFiles);
			}
		}
#endregion


#region Public File Manipulation Methods
		/// <summary>
		/// Returns true if the given name is already being used by another
		/// variable object.
		/// </summary>
		/// <returns><c>true</c>, if name was taken.</returns>
		/// <param name="name">Name to check.</param>
		public static bool IsNameTaken(string name) {
			return CustomVarFiles.Keys.Contains(name)
				|| UnityVarFiles.Keys.Contains(name);
		}
#endregion


#region Constructor and Events
		static ScriptSetManager() {
			assemblyDictionariesAreOutdated = true;

			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
		}

		private static void OnBeforeAssemblyReload() {
			// We'll just assume that some of the changed scripts are important.
			assemblyDictionariesAreOutdated = true;
		}
#endregion


#region Record updaters
		private static void UpdateDictionaries() {
			if(assemblyDictionariesAreOutdated) {
				assemblyDictionariesAreOutdated = false;

				#if REACHBEYOND_VARIABLES_DEBUG
				Debug.Log("Refreshing dictionaries...");
				#endif

				_unityVarFiles = BuildTypeInfo(UnityLabel);
				_customVarFiles = BuildTypeInfo(CustomLabel);

				#if REACHBEYOND_VARIABLES_DEBUG
				Debug.Log(
					"== UNITY VARIABLE DICTIONARY =="
				);
				DebugTypeInfoDictionary(_unityVarFiles);

				Debug.Log(
					"== CUSTOM VARIABLE DICTIONARY =="
				);
				DebugTypeInfoDictionary(_customVarFiles);
				#endif

			} // End if(assemblyDictionariesAreOutdated)
		}

#endregion


#region Dictionary-building tools
		/// <summary>
		/// Builds the type info, based on all of the files that match the given
		/// label. The keys are the human-readable names, NOT the type-names
		/// which the objects might handle.
		/// </summary>
		/// <returns>
		/// The type info dictionary, with the human-readable
		/// type names being the keys.
		/// </returns>
		/// <param name="label">Label which to search for.</param>
		private static Dictionary<string, ScriptSetInfo> BuildTypeInfo(string label) {

			Dictionary<string, ScriptSetInfo> allTypeInfo =
				new Dictionary<string, ScriptSetInfo>();

			string[] allGuids = AssetDatabase.FindAssets("l:" + label);

			foreach(string guid in allGuids) {

				// Used for tracking stuff we read from this specific file.
				ScriptMetaData fileData = ExtractDataFromFile(
					AssetDatabase.GUIDToAssetPath(guid)
				);

				// We need to see if we already know about a variable object
				// which uses this name. If not, we'll build a new typeInfo
				// object. At the end of all of this, we save the GUID of
				// the file into the typeInfo object.
				ScriptSetInfo typeInfo;
				if(!allTypeInfo.TryGetValue(fileData.name, out typeInfo)) {

					// First file of this typeName, so allTypeInfo doesn't
					// contain any info on it.
					typeInfo = new ScriptSetInfo(fileData);
					allTypeInfo[typeInfo.Name] = typeInfo;

					if(fileData.ParsedReferability == ReferabilityMode.Unknown) {
						Debug.LogWarning(
							"Unable to identify the referability mode for "
							+ AssetDatabase.GUIDToAssetPath(guid)
						);
					}
				} // End if(!allTypeInfo.TryGetValue(typeName, out typeInfo))
				else {
					// Only need to do these checks if there was another
					// object which claimed this name. So there's a chance
					// we'll see a mismatch in something.
					if(typeInfo.TypeName != fileData.type) {
						Debug.LogWarning(
							"Type mismatch in "
							+ AssetDatabase.GUIDToAssetPath(guid) + '\n'
							+ "Expected '" + typeInfo.TypeName
							+ "' but found '" + fileData.type + "'"
						);
					}

					if(typeInfo.Referability != fileData.ParsedReferability) {
						Debug.LogWarning(
							"Referability mismatch in "
							+ AssetDatabase.GUIDToAssetPath(guid) + '\n'
							+ "Expected '" + typeInfo.Referability.ToString()
							+ "' but found '" + fileData.ParsedReferability.ToString() + "'"
						);
					}
				} // End if(!allTypeInfo.TryGetValue(typeName, out typeInfo))

				// Now we can FINALLY track this file.
				typeInfo.AddGUID(guid);

			} // End foreach(string guid in allGuids)

			return allTypeInfo;
		}

		/// <summary>
		/// Attempts to extract the data from a given file. The data is expected
		/// to be in a JSON format, and is expected to fall between a line that
		/// contains DataHeader and another that contains DataFooter. Only the
		/// first of such blocks is heeded, and the rest are ignored.
		/// </summary>
		/// <returns>The data from the file.</returns>
		/// <param name="path">Path of file.</param>
		private static ScriptMetaData ExtractDataFromFile( string path ) {

			StreamReader reader;

			string rawJSON = "";
			bool foundStart = false;
			bool foundEnd = false;


			reader = new StreamReader(path);

			try {
				// Read until we find the start of the data
				while(!foundStart && !reader.EndOfStream) {
					string line = reader.ReadLine();
					foundStart = line.Contains(DataHeader);
				}

				// Now read until we find the end of data or end of file
				while(!foundEnd && !reader.EndOfStream) {
					string line = reader.ReadLine();

					if(line.Contains(DataFooter)) {
						foundEnd = true;
					}
					else {
						rawJSON += line;
					}
				}
			}
			finally {
				reader.Close();
			}

			//UnityEngine.Debug.Log(ScriptMetaData.FromJson(rawJSON).ToJson());

			return ScriptMetaData.FromJson(rawJSON);
		}


		/// <summary>
		/// Cuts out the comments and whitespaces in a line and returns the
		/// trimmed output. Note that this also cuts out the asterisks as well.
		/// </summary>
		/// <returns>The word from line.</returns>
		/// <param name="line">Line to parse.</param>
		private static string ExtractWordFromLine(string line) {
			line = Regex.Replace(line, "\\s+", ""); // Drop all white spaces
			return Regex.Replace(line, "[*/]", ""); // Drop all comment chars
		}

		private static void DebugTypeInfoDictionary(Dictionary<string, ScriptSetInfo> dict) {
			foreach(KeyValuePair<string, ScriptSetInfo> pair in dict) {
				Debug.Log(pair.Value.ToString());
			}
		}
#endregion

	} // End class

} // End namespace
