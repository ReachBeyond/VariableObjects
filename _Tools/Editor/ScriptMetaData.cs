//
//  ScriptMetaData.cs
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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ReachBeyond.VariableObjects.Editor {

	/// <summary>
	/// This script contains the meta information on one specific script.
	/// Note that it does not track the script's location; it only tracks
	/// info held within a script's JSON metadata.
	///
	/// This can be converted to and from JSON easily. It can also be
	/// used to apply changes to templated strings.
	/// </summary>
	[System.Serializable]
	public struct ScriptMetaData {

		#region Variables
		// NOTE: Careful with renaming these. This will break
		//       the parsing process. All the files would need
		//       to be changed... a simple refactor won't work.
		public string name;
		public string type;
		public string referability;
		public int menuOrder;
		#endregion

		#region Constructors
		public ScriptMetaData(string name, string type, ReferabilityMode referability, int menuOrder = 20000) {
			this.name = name;
			this.type = type;
			this.referability = referability.ToString();
			this.menuOrder = menuOrder;
		}
		#endregion

		#region JSON conversions
		public static ScriptMetaData FromJson(string rawJson) {
			return JsonUtility.FromJson<ScriptMetaData>(rawJson);
		}

		public string ToJson() {
			return JsonUtility.ToJson(this, prettyPrint: true);
		}
		#endregion

		/// <summary>
		/// Attempts to parse the referabilityName as a ReferabilityMode
		/// enum. If the parsing fails, ReferabilityMode.Unknown is
		/// returned.
		/// </summary>
		public ReferabilityMode ParsedReferability {
			get {
				ReferabilityMode result;

				if(!System.Enum.TryParse(referability, true, out result)) {
					result = ReferabilityMode.Unknown;
				}

				return result;
			}
		} // End field

		#region Substitutions
		/// <summary>
		/// Creates a dictionary which can be used for substituting our metadata
		/// into a string. Currently respects @Name@, @Type@, @Referable@, and
		/// @Order@.
		/// </summary>
		public Dictionary<string, string> SubstitutionRules {
			get {
				Dictionary<string, string> substitutionTargets = new Dictionary<string, string>();
				substitutionTargets["@Name@"] = name;
				substitutionTargets["@Type@"] = type;
				substitutionTargets["@Referable@"] = ParsedReferability.ToString();
				substitutionTargets["@Order@"] = menuOrder.ToString();

				return substitutionTargets;
			}
		}

		/// <summary>
		/// Applies the substitutions from SubstitutionDictionary.
		/// </summary>
		/// <param name="templateText">The text with the templates.</param>
		/// <returns>The templateText with all fields replaced.</returns>
		public string ApplyReplacements(string templateText) {
			foreach(KeyValuePair<string, string> rule in SubstitutionRules) {
				templateText = Regex.Replace(templateText, rule.Key, rule.Value);
			}

			return templateText;
		}
		#endregion

	} // End struct
} // End namespace