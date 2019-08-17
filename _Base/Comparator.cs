//
//  Registrater.cs
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


using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using ReachBeyond.EventObjects;

namespace ReachBeyond.VariableObjects.Base {

	public class Comparator<Type, VarType, ConstRef> : MonoBehaviour
		where Type : class
		where VarType : ClassVariable<Type>
		where ConstRef : ConstReference<Type, VarType> {

		// Don't go trying to optimize this out. "If something is any one of these"
		// is very different from "if none of them are", so don't go thinking we
		// don't need it.
		public enum CompareMode {
			Any, None
		}

		public bool checkOnEnable = false;
		public ConstRef checkTarget;
		public CompareMode mode;
		public ConstRef[] ofThese;

		[Space(10)]
		public EventObjectInvoker onTrue;
		public EventObjectInvoker onFalse;

		private void OnEnable() {
			if(checkOnEnable) {
				Check();
			}
		}

		public void Check() {
			bool success = false;


			switch(mode) {
				case CompareMode.Any:
					success = ofThese.Any((r) => checkTarget.ConstValue == r.ConstValue);
					break;
				case CompareMode.None:
					success = !ofThese.Any((r) => checkTarget.ConstValue == r.ConstValue);
					break;
			}

			if(success) {
				onTrue.Invoke();
			}
			else {
				onFalse.Invoke();
			}

		}

	} // End class
} // End namespace
