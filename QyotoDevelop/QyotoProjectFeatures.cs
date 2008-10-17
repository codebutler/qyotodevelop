// QyotoProjectFeatures.cs created with MonoDevelop
// User: eric at 1:34 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using MonoDevelop.Ide.Templates;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using Gtk;

namespace QyotoDevelop
{
	class QyotoFeatureWidget : Gtk.VBox
	{
		public QyotoFeatureWidget (DotNetProject project)
		{
			Spacing = 6;
		}
	}
	
	public class QyotoProjectFeatures : ISolutionItemFeature
	{
		public string Title {
			get { return "Qyoto Support"; }
		}

		public string Description {
			get { return "Enables support for Qyoto in the project."; }
		}

		public bool SupportsSolutionItem (SolutionFolder parentCombine, SolutionItem entry)
		{
			return Util.SupportsRefactoring(entry as DotNetProject);
		}

		public Widget CreateFeatureEditor (SolutionFolder parentCombine, SolutionItem entry)
		{
			return new QyotoFeatureWidget((DotNetProject)entry);
		}

		public void ApplyFeature (SolutionFolder parentCombine, SolutionItem entry, Widget editor)
		{
			QyotoDesignInfo.EnableProject((DotNetProject)entry);
		}

		public string Validate (SolutionFolder parentCombine, SolutionItem entry, Gtk.Widget editor)
		{
			return null;
		}
		
		public bool IsEnabled (SolutionFolder parentCombine, SolutionItem entry)
		{
			return Util.SupportsDesigner((Project)entry);
		}
	}
}
