//
//  NewVariableTypeWizard.cs
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
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;

namespace ReachBeyond.VariableObjects.Editor {

	public class NewVariableTypeWizard : ScriptableWizard {

		#region Constants
		private const string EditorPrefPrefix = "ReachBeyond.VariableObjects.";
		private const string PathPref = EditorPrefPrefix + "WizardPath";
		#endregion

		#region Editor fields
		[Tooltip(
			"This is the name which will get used for names of the new classes. " +
			"For example, typing 'Apple' here will create scripts like 'AppleVariable.cs' and 'AppleRefEditor.cs'.\n\n" +

			"This means you must specify a unique name here. For example, you cannot use " +
			"'Vector3', because 'Vector3Variable' already exists."
		)]
		/// <summary>
		/// The name which is used for naming new types.
		/// </summary>
		[SerializeField] private string humanReadableName;

		[Tooltip(
			"Put the data type you are trying to support here.\n\n" +

			"This must match exactly how you would type it into your C# code. " +
			"For example, if you are trying to add support for a class named 'Orange', you must type 'Orange'.\n\n" +

			"Furthermore, if your 'Orange' is within a namespace, you have to account for this. " +
			"If you put your 'Orange' class in the 'Fruits' namespace, you must type 'Fruits.Orange'.\n\n" +

			"You can create many variable types that support the same base data type."
		)]
		/// <summary>
		/// The literal C# name of the data type.
		/// </summary>
		[SerializeField] private string dataType;

		[Tooltip(
			"Whether you are adding support for a struct or a class.\n\n" +
			"If you are adding support for a built-in data type (like int, float, or double), " +
			"choose 'Struct'."
		)]
		[SerializeField] private ReferabilityMode referability;

		[SerializeField] private int menuOrder;

		[Tooltip(
			"Folder to put the scripts in. Must be in the current project's assets folder, " +
			"and cannot be in an 'Editor' folder. This may create an Editor folder in the " +
			"target folder."
		)]
		[SerializeField] private UnityFolderPath targetFolder;
		#endregion

		#region Variables and Properties
		private ScriptSetInfo editTarget;
		private bool builtin;

		public bool InEditMode {
			get {
				// If we have an editTarget, then we're certainly
				// in edit mode.
				return editTarget != null;
			}
		}
		#endregion

		#region Factories
		/// <summary>
		/// Creates a new instance of the wizard with blank fields.
		/// This gets called when the user wants to open the new type.
		///
		/// Note that the target folder is remembered whenever the wizard
		/// successfully executes. The wizard restores this target folder when
		/// loaded using this functions. However, if the folder is no longer
		/// valid (i.e. doesn't exist anymore), it reverts back to the default
		/// folder. (Probably "Assets")
		/// </summary>
		[MenuItem("Window/Create new Variable Object Type...")]
		public static void CreateWizard() {
			//string previousPath = EditorPrefs.GetString(PathPref);

			//CreateWizard("", "", ReferabilityMode.Unknown, previousPath);
			CreateWizard(null);
		}

		/// <summary>
		/// Similar to the wizard parameterless version, but this wizard
		/// can work in one of two ways:
		///
		///  editTarget == null: A new variable type will be created
		///  editTarget != null: An old variable type will be overridden.
		///
		/// Unless the goal is to pass in non-null editTarget, call this
		/// without parameters.
		/// </summary>
		/// <param name="editTarget">
		/// Script set to override, if not null.
		/// </param>
		public static void CreateWizard(ScriptSetInfo editTarget) {
			NewVariableTypeWizard wizard = DisplayWizard<NewVariableTypeWizard>(
				"Create Variable Object Type", "Create"
			);

			if(editTarget != null) {
				wizard.humanReadableName = editTarget.Name;
				wizard.dataType = editTarget.TypeName;
				wizard.referability = editTarget.Referability;
				wizard.menuOrder = editTarget.MetaData.menuOrder;
				wizard.builtin = false;
				wizard.targetFolder = new UnityFolderPath(editTarget.DominantPath);
			}
			else {
				wizard.humanReadableName = "";
				wizard.dataType = "";
				wizard.referability = ReferabilityMode.Unknown;
				wizard.menuOrder = 350000;
				wizard.builtin = editTarget.MetaData.builtin;
				wizard.targetFolder = new UnityFolderPath(EditorPrefs.GetString(PathPref));
			}

			wizard.editTarget = editTarget;


			// Force a update...it's possible that, with the parameters we've
			// passed in, it's valid. Otherwise, it won't detect that it's valid
			// until the user changes ones of the fields.
			wizard.OnWizardUpdate();
		}

		public static void ReopenWizard(NewVariableTypeWizard oldWizard) {
			NewVariableTypeWizard wizard = DisplayWizard<NewVariableTypeWizard>(
				"Create Variable Object Type", "Create"
			);

			wizard.humanReadableName = oldWizard.humanReadableName;
			wizard.dataType = oldWizard.dataType;
			wizard.referability = oldWizard.referability;
			wizard.menuOrder = oldWizard.menuOrder;
			wizard.targetFolder = oldWizard.targetFolder;
			wizard.editTarget = oldWizard.editTarget;
			wizard.builtin = oldWizard.builtin;

			wizard.OnWizardUpdate();
		}
		#endregion


		#region Events
		private void OnWizardCreate() {

			try {
				ScriptMetaData newMetaData = new ScriptMetaData(
					humanReadableName, dataType, referability
				);

				if(InEditMode) {
					editTarget.RebuildFiles(newMetaData);
				}
				else {
					VariableTypeBuilder.CreateNewVariableType(
						newMetaData,
						targetFolder.Path
					);
				}

				// It worked, so we can go ahead and save the path now
				EditorPrefs.SetString(PathPref, targetFolder.Path);
			}
			catch(System.Exception e) {
				//Debug.LogError(e.Message);
				EditorUtility.DisplayDialog(
					title:   "Couldn't Create Scripts!",
					message: e.Message,
					ok:      "Go back"
				);

				// Re-open the menu.
				//CreateWizard(humanReadableName, dataType, referability, targetFolder.Path);
				ReopenWizard(this);
			}

		}

		private void OnWizardUpdate() {

			string errorMsg = "";
			string helpMsg = "";

			bool validName = false;
			bool validType = false;

			// We need to check for null, because the string is occasionally
			// null upon wizard's creation
			if(string.IsNullOrEmpty(humanReadableName)) {
				helpMsg += "Please enter the Human Readable Name.";
			}
			else if(!VariableTypeBuilder.IsValidName(humanReadableName)) {
				errorMsg +=
					"The Human Readable Name is not a valid C# variable" +
					" name!\n";
			}
			else if(ScriptSetManager.IsNameTaken(humanReadableName)
				&& !(InEditMode && humanReadableName == editTarget.Name)
			) {

				errorMsg +=
					"The Human Readable Name conflicts with another," +
					" already existing variable type!\n";
			}
			else {
				validName = true;
			}

			helpMsg += '\n';	// Force push the next line into a blank field.


			if(string.IsNullOrEmpty(dataType)) {
				helpMsg += "Please enter the Data Type.";
			}
			else if(!VariableTypeBuilder.IsValidName(dataType)) {
				errorMsg += "The Data Type is not a valid C# variable name!";
			}
			else {
				validType = true;
			}


			helpMsg += '\n';	// Force push the next line into a blank field.


			if(referability == ReferabilityMode.Unknown) {
				helpMsg += "Please choose a referability mode.";
			}


			// We use Trim here because having extra newlines looks ugly.
			errorString = errorMsg.Trim();
			helpString = helpMsg;

			isValid = (
				validName &&
				validType &&
				referability != ReferabilityMode.Unknown
			);
		}

		protected override bool DrawWizardGUI() {
			bool changed = base.DrawWizardGUI();

			EditorGUILayout.HelpBox(
				"Keep in mind...\n" +
				"1) The C# type refered to by Data Type should already exist elsewhere in your code.\n" +
				"2) If uncertain, read the tooltips.\n" +
				"3) If still uncertain, read the other documentation.\n" +
				"4) To fix a typo, go to the 'VarObj Settings' window and remake.\n" +
				"5) Unity cannot undo this operation.",
				MessageType.Info, true);

			return changed;
		}
		#endregion


	} // End class

} // End namespace
