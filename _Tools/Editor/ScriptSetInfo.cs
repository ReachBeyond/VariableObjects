//
//  ScriptSetInfo.cs
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


﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace ReachBeyond.VariableObjects.Editor {

	/// <summary>
	/// This class is used for tracking all files related to the given
	/// variable object type.
	/// </summary>
	public class ScriptSetInfo {

		#region Properties
		/// <summary>
		/// The human readable name that is used to refer to the variable object.
		/// </summary>
		public string Name {
			get {
				return MetaData.name;
			}
		}

		/// <summary>
		/// The type which the variable object supports.
		/// </summary>
		public string TypeName {
			get {
				return MetaData.type;
			}
		}

		/// <summary>
		/// The referability mode of the type this script set deals with.
		/// </summary>
		public ReferabilityMode Referability {
			get {
				return MetaData.ParsedReferability;
			}
		}

		/// <summary>
		/// All file GUIDs that go with this type. Note that this is a copy
		/// and cannot be modified directly. Refer to AddGUID for that.
		/// </summary>
		public string[] GUIDs {
			get {
				return _GUIDs.ToArray();
			}
		}

		public ScriptMetaData MetaData {
			get; private set;
		}

		/// <summary>
		/// Our best guess as to the path of the scripts. Since there are
		/// usually more than one script and they can't all be in the
		/// same place, it's possible that this might return something
		/// weird.
		///
		/// Never will yield an editor path, however.
		/// </summary>
		public string DominantPath {
			get {
				string resultPath = "";
				int index = 0;
				string[] GUIDs = this.GUIDs;	// Grab this and just cache to be efficient.
				while(resultPath == "" && index < GUIDs.Length) {
					string path = AssetDatabase.GUIDToAssetPath(GUIDs[index]);

					// We want something which ISN'T in the editor assembly because
					// that's easier to work with. Of course, this assumes that at
					// least ONE script isn't in the editor assembly. Things might
					// go south if that's not the case.
					if(!UnityPathUtils.IsInEditorAssembly(path)) {
						resultPath = new FileInfo(path).Directory.FullName;
					}
					else {
						index++;
					}

				}

				return resultPath;
			}
		}
		#endregion

		#region Variables
		private List<string> _GUIDs;
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a new script based on the given metadata.
		/// </summary>
		/// <param name="metaData">Data of the scripts.</param>
		public ScriptSetInfo(ScriptMetaData metaData) {
			MetaData = metaData;
			_GUIDs = new List<string>();
		}
		#endregion


		public void AddGUID(string newGUID) {
			if(!string.IsNullOrEmpty(newGUID) && !_GUIDs.Contains(newGUID)) {
				_GUIDs.Add(newGUID);
			}
		}


		#region File set manipulators
		/// <summary>
		/// Deletes each file associated with this set of scripts. This has
		/// no prompting and uses AssetDatabase to delete the files.
		///
		/// It will also clean up empty editor folders. This could result in
		/// weird side effects if the user has many editor folder strewn
		/// about.
		///
		/// Be warned that this is NOT instant, as it relies on
		/// AssetDatabase.DeleteAsset. This will queue up the deletions
		/// on one of Unity's other threads.
		/// </summary>
		public void DeleteFiles() {

			if(_GUIDs.Count > 0) {
				// We need to get the editor path first
				string samplePath = Path.GetDirectoryName(
					AssetDatabase.GUIDToAssetPath(_GUIDs[0])
				);
				string editorPath = UnityPathUtils.GetEditorFolder(samplePath);
				//string absEditorPath = UnityPathUtils.RelativeToAbsolute(relEditorPath);

				foreach(string GUID in _GUIDs) {
					AssetDatabase.DeleteAsset(
						AssetDatabase.GUIDToAssetPath(GUID)
					);
				}

				if(Directory.Exists(editorPath) &&
					Directory.GetFiles(editorPath).Length == 0 &&
					Directory.GetDirectories(editorPath).Length == 0
				) {
					AssetDatabase.DeleteAsset(editorPath);
				}
			} // End if
		} // End DeleteFiles

		/// <summary>
		/// Rebuild the files associated with this type based upon its meta data.
		/// This can potentially delete scripts if the templates don't
		/// explicitly build them.
		///
		/// Scripts are dumped into the folder pointed to by DominantPath.
		/// </summary>
		/// <returns>The paths of the new/modified files.</returns>
		public List<string> RebuildFiles() {
			return RebuildFiles(MetaData);
		}

		/// <summary>
		/// Rebuild the files associated with this type based upon its meta data.
		/// This can potentially delete scripts if the templates don't
		/// explicitly build them.
		///
		/// Scripts are dumped into the folder pointed to by DominantPath.
		/// </summary>
		/// <param name="newMetaData">
		/// The new metadata to use. This simply means that the new scripts
		/// will use this metadata, and then we'll delete old scripts which
		/// haven't been overriden. It does NOT mean that files will get
		/// renamed.
		/// </param>
		/// <returns>The paths of the new/modified files.</returns>
		public List<string> RebuildFiles(ScriptMetaData newMetaData) {

			// We'll save the file paths for later... we can never really
			// trust that things won't get updated after we modify the files.
			string[] origPaths = GUIDs;
			for(int i = 0; i < origPaths.Length; i++) {
				origPaths[i] = UnityPathUtils.LocalizeDirectorySeparators(
					AssetDatabase.GUIDToAssetPath(origPaths[i])
				);
			}

			List<string> resultFiles = VariableTypeBuilder.CreateNewVariableType(
				newMetaData,
				UnityPathUtils.AbsoluteToRelative(DominantPath),
				overrideExisting: true
			);

			// Once we perform the reset, we'll want to clean up any extra files
			// which were not in our set of templates. If we find any,
			// we'll make sure to delete 'em and get everything cleaned up.
			foreach(string origPath in origPaths) {
				if(!resultFiles.Contains(origPath)) {
					AssetDatabase.DeleteAsset(origPath);
				}
			}

			return resultFiles;
		}

		#endregion

		public override string ToString () {
			string filePaths = "";

			foreach(string guid in _GUIDs) {
				filePaths += AssetDatabase.GUIDToAssetPath(guid) + '\n';
			}

			return string.Format (
				"Name: " + Name + '\n' +
				"Type: " + TypeName + '\n' +
				"Referability: " + Referability.ToString() + '\n' +
				filePaths );
		}

	} // End of class

} // End of namespace
