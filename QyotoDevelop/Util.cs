// Util.cs created with MonoDevelop
// User: eric at 4:46 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

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

		public static bool HasQyotoReference (DotNetProject project)
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
