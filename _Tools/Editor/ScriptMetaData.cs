using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ReachBeyond.VariableObjects.Editor {

	/// <summary>
	/// This script contains the meta information on one specific script.
	/// Note that it does not track the script's location; it only tracks
	/// info held within a script's JSON metadata.
	///
	/// This can be converted to and from JSON freely.
	/// </summary>
	[System.Serializable]
	public struct ScriptMetaData {

		#region Identifier strings
		/// <summary>
		/// Text string used in scripts to specify that they work with a Class.
		/// </summary>
		public static string ClassIdentifier {
			get {
				return ReferabilityMode.Class.ToString();
			}
		}

		/// <summary>
		/// Text string used in scripts to specify that they work with a Struct.
		/// </summary>
		public static string StructIdentifier {
			get {
				return ReferabilityMode.Struct.ToString();
			}
		}
		#endregion

		#region Variables
		// NOTE: Careful with renaming these. This will break
		//       the parsing process. All the files would need
		//       to be changed... a simple refactor won't work.
		public string name;
		public string type;
		public string referability;
		#endregion

		#region Constructors and and JSON conversions
		public ScriptMetaData(string name, string type, ReferabilityMode referability) {
			this.name = name;
			this.type = type;
			this.referability = referability.ToString();
		}

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
				if(string.Compare(referability, ClassIdentifier, ignoreCase: true) == 0) {
					return ReferabilityMode.Class;
				}
				else if(string.Compare(referability, StructIdentifier, ignoreCase: true) == 0) {
					return ReferabilityMode.Struct;
				}
				else {
					return ReferabilityMode.Unknown;
				}
			}
		} // End field

	} // End struct
} // End namespace