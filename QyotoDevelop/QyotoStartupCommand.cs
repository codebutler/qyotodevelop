// QyotoStartupCommand.cs created with MonoDevelop
// User: eric at 4:43 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using MonoDevelop.Components.Commands;

namespace QyotoDevelop
{
	public class QyotoStartupCommand : CommandHandler
	{		
		protected override void Run()
		{
			ReferenceManager.Initialize();
			QyotoDevelopService.Initialize();
		}
	}

	public static class QyotoDevelopService
	{
		public static void Initialize ()
		{
			IdeApp.Workspace.SolutionLoaded   += OnSolutionLoaded;
			IdeApp.Workspace.SolutionUnloaded += OnSolutionUnloaded;
		}

		static void OnSolutionLoaded (object o, SolutionEventArgs args)
		{
			args.Solution.SolutionItemAdded += OnSolutionItemAdded;
			foreach (SolutionItem item in args.Solution.Items) {
				if (item is DotNetProject) {
					OnProjectAdded((DotNetProject)item);
				}
			}
		}

		static void OnSolutionUnloaded (object o, SolutionEventArgs args)
		{
			args.Solution.SolutionItemAdded -= OnSolutionItemAdded;
		}

		static void OnSolutionItemAdded (object o, SolutionItemChangeEventArgs args)
		{
			if (args.SolutionItem is DotNetProject) {
				OnProjectAdded((DotNetProject)args.SolutionItem);
			}
		}

		static void OnProjectAdded (DotNetProject project)
		{
			QyotoDesignInfo info = (QyotoDesignInfo)project.ExtendedProperties["QyotoDesignInfo"];
			if (info != null) {
				info.Project = project;
			}
		}
	}
}
