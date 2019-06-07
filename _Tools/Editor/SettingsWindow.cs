//
//  SettingsWindow.cs
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

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace ReachBeyond.VariableObjects.Editor {

	public class SettingsWindow : EditorWindow {
		// TODO Make the little instruction booklet icon open something relevant

		/// <summary>
		/// This is a simple struct used for our foldouts and tracking what
		/// the user has done with them.
		/// </summary>
		private struct FoldoutEventInfo {
			public bool isFoldedOut;
			public bool selectedDelete;
			public bool selectedRemake;
			public bool selectedEdit;

		}

#region Constants
		private const string EditorPrefPrefix = "ReachBeyond.VariableObjects.";

		private const string UnityVarFoldoutPref  = EditorPrefPrefix + "unityVarFoldout";
		private const string CustomVarFoldoutPref = EditorPrefPrefix + "customVarFoldout";

		private const string HorizontalScrollPref = EditorPrefPrefix + "scrollX";
		private const string VerticalScrollPref   = EditorPrefPrefix + "scrollY";
#endregion


#region Initialization
		[MenuItem("Window/Variable Objects")]
		public static void Init() {
			// Get existing open window or if none, make a new one:
			SettingsWindow window = (SettingsWindow)EditorWindow.GetWindow(typeof(SettingsWindow));
			window.Show();
		}
#endregion

#region Events
		private void OnEnable() {
			titleContent.text = "VarObj Settings";
		}

		private void OnGUI() {

			Vector2 scrollPos = new Vector2(
				EditorPrefs.GetFloat(HorizontalScrollPref, 0f),
				EditorPrefs.GetFloat(VerticalScrollPref, 0f)
			);

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

			DrawVarObjHierarchy(
				"Unity Variable Types",
				UnityVarFoldoutPref,
				ScriptSetManager.UnityVarFiles,
#if REACHBEYOND_VAROBJ_BUILTIN_MODE
				canEdit: true
#else
				canEdit: false
#endif
			);

			DrawVarObjHierarchy(
				"Custom Variable Types",
				CustomVarFoldoutPref,
				ScriptSetManager.CustomVarFiles,
				canEdit: true
			);

			EditorGUILayout.EndScrollView();

			EditorPrefs.SetFloat(HorizontalScrollPref, scrollPos.x);
			EditorPrefs.SetFloat(VerticalScrollPref, scrollPos.y);
		}
#endregion


#region GUI Drawing Functions
		private void DrawVarObjHierarchy(
			string masterLabel,
			string masterFoldoutPref,
			Dictionary<string, ScriptSetInfo> fileInfoDictionary,
			bool canEdit
		) {
			bool isFoldedOut;				// A catch-all variable for storing the foldout bools

			// Create the main foldout
			isFoldedOut = EditorPrefs.GetBool(masterFoldoutPref, defaultValue: false);
			isFoldedOut = EditorGUILayout.Foldout(isFoldedOut, masterLabel, toggleOnLabelClick: true);
			EditorPrefs.SetBool(masterFoldoutPref, isFoldedOut);

			// Only render the list itself if it's folded out.
			if(isFoldedOut) {

				EditorGUI.indentLevel++;

				// Step through all of the types found
				foreach(ScriptSetInfo fileInfo in fileInfoDictionary.Values) {

					// Draw the foldout (with its buttons, if folded out and editable).

					// We need to figure out if we're folded out, according to the preferences.
					string editorPrefKey = EditorPrefPrefix + fileInfo.Name;
					bool isTypeFoldedOut = EditorPrefs.GetBool(editorPrefKey, defaultValue: false);

					// We want a compact label if folded out. Otherwise, we'll list more info.
					string foldoutLabel = fileInfo.Name;
					if(!isTypeFoldedOut) {
						foldoutLabel += (" (" + fileInfo.TypeName + ", " + fileInfo.Referability.ToString() + ") ");
					}

					FoldoutEventInfo foldoutInfo = DrawFoldout(
						EditorPrefs.GetBool(editorPrefKey, defaultValue: false),
						foldoutLabel,
						canEdit
					);

					EditorPrefs.SetBool(editorPrefKey, foldoutInfo.isFoldedOut);


					// Again, only render the list of files if the foldout is open.
					if(foldoutInfo.isFoldedOut) {
						EditorGUI.indentLevel++;
						DrawFiles(fileInfo.GUIDs);
						EditorGUI.indentLevel--;
					}

					// Handle stuff with the delete button
					if(foldoutInfo.selectedDelete) {

						//Debug.Log("Delete " + fileInfo.name);
						//DeleteFilesForType(fileInfo);
						//ScriptFileManager.DeleteFilesForType(fileInfo, prompt: true);
						bool deletionConfirmed = EditorUtility.DisplayDialog(
							"Delete variable object scripts named '" + fileInfo.Name + "'?",
							"This action cannot be undone!"
							+ "\n(Check Variable Object Settings for list of files.)",
							"Delete them", "Spare them"
						);

						if(deletionConfirmed) {
							fileInfo.DeleteFiles();
						}
					}
					else if(foldoutInfo.selectedEdit) {
						NewVariableTypeWizard.CreateWizard(fileInfo);
					}
					else if(foldoutInfo.selectedRemake) {

						bool remakeConfirmed = EditorUtility.DisplayDialog(
							"Remake variable object scripts named '" + fileInfo.Name + "'?",
							"They will be placed inside " + fileInfo.DominantPath + "\n"
							+ "This action cannot be undone!\n"
							+ "(Check Variable Object Settings for list of files.)",
							"Remake them", "Maybe not"
						);

						if(remakeConfirmed) {
							List<string> modifiedFiles = fileInfo.RebuildFiles();

						}
					}
				} // End foreach(...fileInfo...)

				if(GUILayout.Button("Rebuild all")) {

					bool remakeConfirmed = EditorUtility.DisplayDialog(
						"Remake all " + masterLabel,
						"Remake ALL of the scripts? This could break things if you aren't careful!",
						"Remake them all!", "Hang on!"
					);

					if(remakeConfirmed) {
						foreach(ScriptSetInfo setInfo in fileInfoDictionary.Values) {
							setInfo.RebuildFiles();
						}
					}
				}

				EditorGUI.indentLevel--;
			} // End if(isFoldedOut)
		} // End DrawVarObjHierarchy


		/// <summary>
		/// Draws the list of files. These may be clicked on to selected them, but they cannot be tweaked.
		/// </summary>
		/// <param name="guids">List of GUIDs to reference.</param>
		private void DrawFiles(string[] guids) {
			foreach(string guid in guids) {
				MonoScript scriptObj = AssetDatabase.LoadAssetAtPath<MonoScript>(
					AssetDatabase.GUIDToAssetPath(guid)
				);

				using (new EditorGUI.DisabledScope(true)) {
					EditorGUILayout.ObjectField(scriptObj, typeof(MonoScript), allowSceneObjects: false);
				}
			}
		}

		/*

		/// <summary>
		/// Draws the foldout.
		/// </summary>
		/// <returns><c>true</c>, if foldout is opened by the user, <c>false</c> otherwise.</returns>
		/// <param name="foldout">If set to <c>true</c>, then the foldout will be opened.</param>
		/// <param name="content">The label to draw.</param>
		private void DrawFoldout(ref bool foldout, string content) {
			foldout = EditorGUILayout.Foldout(foldout, content, toggleOnLabelClick: true);
		}
		*/

		/// <summary>
		/// Draws the foldout. Also has a delete button.
		/// </summary>
		/// <param name="foldout">Foldout.</param>
		/// <param name="content">Content.</param>
		/// <param name="drawButtons">If true, extra buttons are drawn while folded out.</param>
		private FoldoutEventInfo DrawFoldout(bool foldout, string content, bool drawButtons) {

			const int SPACING = 10;
			const int DELETE_BUTTON_WIDTH = 60;
			const int REMAKE_BUTTON_WIDTH = 60;
			const int EDIT_BUTTON_WIDTH = 60;

			FoldoutEventInfo eventInfo = new FoldoutEventInfo();

			Rect mainRect = EditorGUILayout.GetControlRect();	// Total space we have to work with
			Rect foldoutRect = mainRect;

			if(drawButtons && foldout) {
				// Create our rectangle areas for buttons
				Rect deleteRect = mainRect;
				deleteRect.width = DELETE_BUTTON_WIDTH;

				Rect remakeRect = mainRect;
				remakeRect.width = REMAKE_BUTTON_WIDTH;

				Rect editRect = mainRect;
				editRect.width = EDIT_BUTTON_WIDTH;

				// Remove the button widths from the foldout's width
				foldoutRect.width -= (deleteRect.width + remakeRect.width + editRect.width + 2 * SPACING);

				// Put buttons in their places
				remakeRect.x += foldoutRect.width;
				editRect.x = remakeRect.x + remakeRect.width + SPACING;
				deleteRect.x = editRect.x + editRect.width + SPACING;

				// Draw buttons
				eventInfo.selectedRemake = GUI.Button(remakeRect, "Remake");
				eventInfo.selectedEdit = GUI.Button(editRect, "Edit");
				eventInfo.selectedDelete = GUI.Button(deleteRect, "Delete");
			}

			eventInfo.isFoldedOut = EditorGUI.Foldout(foldoutRect, foldout, content, toggleOnLabelClick: true);

			return eventInfo;
		}
#endregion

	} // End of class

} // End of namespace
