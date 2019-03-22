using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ReachBeyond.VariableObjects.Editor {

	/// <summary>
	/// This is a dummy struct which handles converting data to and from
	/// JSON format.
	/// </summary>
	[System.Serializable]
	public struct JsonContainer {

		/// <summary>
		/// Text string used in scripts to specify that they work with a Class.
		/// </summary>
		//public const string ClassIdentifier = "Class";
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

		// NOTE: Careful with renaming these. This will break
		//       the parsing process. All the files would need
		//       to be changed... a simple refactor won't work.
		public string name;
		public string type;
		public string referability;

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

		/// <summary>
		/// Creates a new ScriptInfo object which is pre-populated
		/// with the values in this instance of VObjData.
		/// </summary>
		/// <returns>A new ScriptInfo object.</returns>
		public ScriptInfo ToScriptInfo() {
			return new ScriptInfo(name, type, ParsedReferability);
		}
	} // End struct
} // End namespace