// QyotoDesignInfo.cs created with MonoDevelop
// User: eric at 1:41 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
using System.Collections.Generic;

using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using MonoDevelop.Projects.CodeGeneration;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Core.Serialization;

namespace QyotoDevelop
{
	public class QyotoDesignInfo : IDisposable
	{
		DotNetProject          m_Project;
		IDotNetLanguageBinding m_Binding;
		FileSystemWatcher      m_Watcher;
		List<QyotoForm>        m_Forms = new List<QyotoForm>();

		public event EventHandler Changed;

		public static void EnableProject (Project project)
		{
			if (project.ExtendedProperties["QyotoDesignInfo"] == null || ((QyotoDesignInfo)project.ExtendedProperties["QyotoDesignInfo"]).Project == null) {
				new QyotoDesignInfo((DotNetProject)project);
			}
		}
		
		public static void DisableProject (Project project)
		{
			QyotoDesignInfo info = (QyotoDesignInfo)project.ExtendedProperties["QyotoDesignInfo"];
			if (info != null) {
				info.Dispose();
			}
		}

		public QyotoDesignInfo ()
		{
			/* used by the serializer */
		}
		
		private QyotoDesignInfo(DotNetProject project) : this ()
		{
			Project = project;
		}

		[ItemProperty("Forms")]
		public List<QyotoForm> Forms {
			get {
				return m_Forms;
			}
			set { /* This is only here for the serializer. Do not use */
				m_Forms = value;
				if (m_Project != null)
					ProjectNodeBuilder.FilesChanged(m_Project);
			}
		}

		public string QtGuiFolder {
			get { return Path.Combine(m_Project.BaseDirectory, "qt-gui"); }
		}
		
		public DotNetProject Project {
			get { return m_Project; }
			set {
				if (m_Project != null)
					throw new InvalidOperationException("Project already set.");
	
				if (value == null)
					throw new ArgumentNullException("project");
				
				if (value.ExtendedProperties["QyotoDesignInfo"] != null && value.ExtendedProperties["QyotoDesignInfo"] != this)
					throw new InvalidOperationException("Project already has a QyotoDesignInfo!");
				
				value.ExtendedProperties["QyotoDesignInfo"] = this;
				
				m_Project = value;
				m_Binding = Services.Languages.GetBindingPerLanguageName(Project.LanguageName) as IDotNetLanguageBinding;

				FileService.CreateDirectory(QtGuiFolder);
				
				m_Watcher = new FileSystemWatcher(QtGuiFolder);
				m_Watcher.Created += OnGuiFileCreated;
				m_Watcher.Deleted += OnGuiFileDeleted;
				m_Watcher.Changed += OnGuiFileChanged;
				m_Watcher.Renamed += OnGuiFileRenamed;

				// Just in case someone went and mucked around with the file, 
				// removing the reference but keeping the <QyotoDesignerInfo/>
				EnsureReferences();
				
				ProjectNodeBuilder.FilesChanged(m_Project);
			}
		}
		
		public void Dispose ()
		{
			m_Project.ExtendedProperties.Remove("QyotoDesignInfo");
			ProjectNodeBuilder.FilesChanged(m_Project);
			m_Binding = null;
		}
		
		void EnsureReferences ()
		{
			bool hasQyoto = false;

			foreach (ProjectReference reference in m_Project.References) {
				if (Util.GetReferenceName(reference) == "qt-dotnet") {
					hasQyoto = true;
					break;
				}
			}
			
			if (!hasQyoto) {
				m_Project.References.Add(new ProjectReference(ReferenceType.Gac, String.Format("qt-dotnet, {0}", Util.GetQyotoAssemblyVersion())));
			}
		}

		void OnGuiFileCreated (object o, FileSystemEventArgs args)
		{
			ProjectNodeBuilder.FilesChanged(m_Project);
		}

		void OnGuiFileUpdated (object o, FileSystemEventArgs args)
		{
			ProjectNodeBuilder.FilesChanged(m_Project);
		}

		void OnGuiFileRenamed (object o, FileSystemEventArgs args)
		{
			ProjectNodeBuilder.FilesChanged(m_Project);
		}

		void OnGuiFileDeleted (object o, FileSystemEventArgs args)
		{
			ProjectNodeBuilder.FilesChanged(m_Project);
		}

		void OnGuiFileChanged (object o, FileSystemEventArgs args)
		{
			ProjectNodeBuilder.FilesChanged(m_Project);
		}
	}
}
