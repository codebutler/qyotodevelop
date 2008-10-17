// ReferenceManager.cs created with MonoDevelop
// User: eric at 1:58 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

namespace QyotoDevelop
{	
	public static class ReferenceManager
	{
		public static void Initialize ()
		{
			IdeApp.Workspace.ReferenceAddedToProject     += OnReferenceAdded;
			IdeApp.Workspace.ReferenceRemovedFromProject += OnReferenceRemoved;
		}
		
		static void OnReferenceAdded (object o, ProjectReferenceEventArgs args)
		{
			if (!Util.IsQyotoReference(args.ProjectReference))
				return;

			Project project = args.Project as DotNetProject;
			if (project != null && project.ExtendedProperties["QyotoDesignInfo"] == null)
				QyotoDesignInfo.EnableProject(project);
		}

		static void OnReferenceRemoved (object o, ProjectReferenceEventArgs args)
		{
			if (!Util.IsQyotoReference(args.ProjectReference))
			    return;

			DotNetProject dnp = args.Project as DotNetProject;

			if (dnp != null) {
				if (MonoDevelop.Core.Gui.MessageService.Confirm("Qyoto features will be disabled by removing the qt-dotnet reference.", new MonoDevelop.Core.Gui.AlertButton("Disable Qyoto")))
					QyotoDesignInfo.DisableProject(dnp);
				else
					dnp.References.Add(new ProjectReference(ReferenceType.Gac, args.ProjectReference.StoredReference));
			}
		}
	}
}
