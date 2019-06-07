//
//  UnityPathUtilsTests.cs
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


﻿using NUnit.Framework;
using UnityEngine;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace ReachBeyond.VariableObjects.Editor.Tests {

	[TestFixture]
	public class UnityPathUtilsTests {

		[Test]
		public void TestValidRelativePath() {
			string testPath = UnityPathUtils.Combine("Assets", "Things/test.txt");
			Assert.That(UnityPathUtils.IsRelativePath(testPath), Is.True);
		}

		[Test]
		public void TestInvalidRelativePath() {
			string testPath = UnityPathUtils.Combine(Application.dataPath, "Things/test.txt");
			Assert.That(UnityPathUtils.IsRelativePath(testPath), Is.False);
		}

		[Test]
		public void TestAbsoluteToRelative() {
			// Gotta do this because we need consistent directory separators.
			string dataPath = Application.dataPath.Replace('/', Path.DirectorySeparatorChar);
			string basePath = UnityPathUtils.Combine("Things", "test.txt");

			string absPath = UnityPathUtils.Combine( dataPath, basePath );
			string relPath = UnityPathUtils.Combine( "Assets", basePath );

			Assert.That(
				UnityPathUtils.AbsoluteToRelative(absPath),
				Is.EqualTo(relPath)
			);
		}

		[Test]
		public void TestRelativeToAbsolute() {
			// Gotta do this because we need consistent directory separators.
			string dataPath = Application.dataPath.Replace('/', Path.DirectorySeparatorChar);
			string basePath = UnityPathUtils.Combine("Things", "test.txt");

			string absPath = UnityPathUtils.Combine( dataPath, basePath );
			string relPath = UnityPathUtils.Combine( "Assets", basePath );

			Assert.That(
				UnityPathUtils.RelativeToAbsolute(relPath),
				Is.EqualTo(absPath)
			);
		}

		[TestCase("Meh/", "Meh/Editor")]
		[TestCase("Meh", "Meh/Editor")]
		[TestCase("Editor/", "Editor/")]
		[TestCase("Editor", "Editor")]
		[TestCase("Meh/Editor/things", "Meh/Editor/things")]
		[TestCase("Meh/EditorMeepo/things", "Meh/EditorMeepo/things/Editor")]
		[TestCase("Meh/EditorMeepo/things/", "Meh/EditorMeepo/things/Editor")]
		[TestCase("Hello/Meep", "Hello/Meep/Editor")]
		[TestCase("Hello/Editor", "Hello/Editor")]
		[TestCase("Hello/Editor/Foo", "Hello/Editor/Foo")]
		public void TestGetEditorFolder(string initPath, string expectedPath) {
			initPath = initPath.Replace('/', Path.DirectorySeparatorChar);
			expectedPath = expectedPath.Replace('/', Path.DirectorySeparatorChar);

			Assert.That(
				UnityPathUtils.GetEditorFolder(initPath),
				Is.EqualTo(expectedPath)
			);
		}

		[TestCase("Hello/Meep/Editor", true)]
		[TestCase("Hello/Editor", true)]
		[TestCase("Hello/Editor/", true)]
		[TestCase("Hello/Editor/Meep", true)]
		[TestCase("Hello/EditorEditor/Editor", true)]
		[TestCase("Editor/Woah/EditorLie", true)]
		[TestCase("Editor/Woah", true)]
		[TestCase("Hello/EditorDerp/Meep", false)]
		[TestCase("Hello/EditorEditor/Meep", false)]
		[TestCase("Harhar/lol", false)]
		public void TestIsInEditorAssembly(string path, bool expectedValue) {
			Assert.That(
				UnityPathUtils.IsInEditorAssembly(path),
				Is.EqualTo(expectedValue)
			);
		}

		[TestCase("Hello", "Hi", "Hello/Hi")]
		[TestCase("Hello/", "Hi", "Hello/Hi")]
		[TestCase("Hello", "Hi/", "Hello/Hi/")]
		[TestCase("Hello/", "Hi/", "Hello/Hi/")]
		[TestCase("Hello/", "/Hi", "/Hi")]
		[TestCase("C:/Hello/", "/Hi", "/Hi")]
		[TestCase("C:/Hello/", "C:/Hi", "C:/Hi")]
		[TestCase("/Hello/", "/Hi", "/Hi")]
		[TestCase("/Hello/", "Hi", "/Hello/Hi")]
		public void TestCombine(string path1, string path2, string expected) {
			
			path1 = path1.Replace('/', Path.DirectorySeparatorChar);
			path2 = path2.Replace('/', Path.DirectorySeparatorChar);
			expected = expected.Replace('/', Path.DirectorySeparatorChar);

			Assert.That(
				UnityPathUtils.Combine(path1, path2),
				Is.EqualTo(expected)
			);
		}

	} // End class
} // End namespace
