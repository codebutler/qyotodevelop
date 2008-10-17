// FormsFolder.cs created with MonoDevelop
// User: eric at 4:27 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

using MonoDevelop.Projects;

namespace QyotoDevelop
{
	class FormsFolder
	{
		Project project;
		
		public event EventHandler Changed;
		
		public FormsFolder (Project project)
		{
			this.project = project;
			QyotoDesignInfo info = QyotoDesignInfo.FromProject (project);
			
			info.Changed += OnUpdateFiles;
		}
		
		public void Dispose ()
		{
			QyotoDesignInfo info = QyotoDesignInfo.FromProject (project);
			info.Changed -= OnUpdateFiles;
		}
		
		void OnUpdateFiles (object s, EventArgs args)
		{
			if (Changed != null) Changed (this, EventArgs.Empty);
		}
		
		public Project Project {
			get { return project; }
		}
	}
}
