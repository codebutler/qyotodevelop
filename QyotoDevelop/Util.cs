// Util.cs
//
// Copyright (c) 2008 Eric Butler <eric@extremeboredom.net>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using MonoDevelop.Projects.CodeGeneration;


namespace QyotoDevelop
{
	public static class Util
	{
		public static string GetQyotoAssemblyVersion ()
		{
			// XXX:
			return "Version=4.4.0.0, Culture=neutral, PublicKeyToken=194a23ba31c08164";
		}
		
		public static string GetReferenceName (ProjectReference pref)
		{
			string stored = pref.StoredReference;
			int idx =stored.IndexOf (",");
			if (idx == -1)
				return stored.Trim ();

			return stored.Substring (0, idx).Trim ();
		}
		
		public static bool IsQyotoReference (ProjectReference pref)
		{
			if (pref.ReferenceType != ReferenceType.Gac)
				return false;

			int idx = pref.StoredReference.IndexOf(",");
			if (idx == -1)
				return false;

			string name = pref.StoredReference.Substring(0, idx).Trim();
			return name == "qt-dotnet";
		}

		private static bool HasQyotoReference (DotNetProject project)
		{
			foreach (ProjectReference pref in project.References)
				if (IsQyotoReference(pref))
					return true;
			return false;
		}

		public static bool SupportsDesigner (Project project)
		{
			DotNetProject dnp = project as DotNetProject;
			return dnp != null && Util.HasQyotoReference(dnp) && SupportsRefactoring(dnp);
		}

		public static bool SupportsRefactoring (DotNetProject project)
		{
			if (project == null || project.LanguageBinding == null || project.LanguageBinding.GetCodeDomProvider () == null)
				return false;
			RefactorOperations ops = RefactorOperations.AddField | RefactorOperations.AddMethod | RefactorOperations.RenameField | RefactorOperations.AddAttribute;
			CodeRefactorer cref = IdeApp.Workspace.GetCodeRefactorer (project.ParentSolution);
			return cref.LanguageSupportsOperation (project.LanguageBinding.Language, ops); 
		}
		
		public static bool HasDesignedObjects (Project project)
		{
			if (!Util.SupportsDesigner(project) || project.ExtendedProperties["QyotoDesignInfo"] == null)
				return false;

			QyotoDesignInfo info = (QyotoDesignInfo)project.ExtendedProperties["QyotoDesignInfo"];
			return info.Forms.Count > 0;
		}

	}
}
