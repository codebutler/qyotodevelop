// QyotoDesignInfo.cs
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
using System.IO;
using System.Collections.Generic;

using MonoDevelop.Core;
using MonoDevelop.Core.Gui;
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
		FileSystemWatcher      m_Watcher;
		EventyList<QyotoForm>  m_Forms;

		public static QyotoDesignInfo FromProject (Project project)
		{
			return FromProject(project, false);
		}
		
		public static QyotoDesignInfo FromProject (Project project, bool createIfNeeded)
		{
			QyotoDesignInfo info = (QyotoDesignInfo)project.ExtendedProperties["QyotoDesignInfo"];
			if (info == null) {
				if (createIfNeeded) {
					QyotoDesignInfo.EnableProject(project);
					info = (QyotoDesignInfo)project.ExtendedProperties["QyotoDesignInfo"];
				} else {
					throw new Exception("project does not have a QyotoDesignInfo!");
				}
			}
			return info;
		}
		
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
		public EventyList<QyotoForm> Forms {
			get {
				if (m_Forms == null) {
					m_Forms = new EventyList<QyotoForm>();
					m_Forms.ItemAdded   += OnFormAdded;
					m_Forms.ItemRemoved += OnFormRemoved;
				}
				return m_Forms;
			}
			set {
				if (m_Forms != null)
					throw new InvalidOperationException("For serializer only. Do not use.");
				
				m_Forms = value;
				m_Forms.ItemAdded   += OnFormAdded;
				m_Forms.ItemRemoved += OnFormRemoved;
				foreach (QyotoForm form in m_Forms)
					OnFormAdded(null, form);					
				
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
				if (m_Project != null) {
					if (m_Project != value)
						throw new InvalidOperationException("Different project already set!");
					return;
				}
	
				if (value == null)
					throw new ArgumentNullException("project");
				
				if (value.ExtendedProperties["QyotoDesignInfo"] != null && value.ExtendedProperties["QyotoDesignInfo"] != this)
					throw new InvalidOperationException("Project already has a QyotoDesignInfo!");
				
				value.ExtendedProperties["QyotoDesignInfo"] = this;
				
				m_Project = value;

				if (!Directory.Exists(QtGuiFolder))
					FileService.CreateDirectory(QtGuiFolder);

				foreach (QyotoForm form in Forms)
					form.Project = m_Project;
				
				m_Watcher = new FileSystemWatcher(QtGuiFolder);
				m_Watcher.Deleted += (FileSystemEventHandler)DispatchService.GuiDispatch(new FileSystemEventHandler(OnGuiFileDeleted));
				m_Watcher.Changed += (FileSystemEventHandler)DispatchService.GuiDispatch(new FileSystemEventHandler(OnGuiFileChanged));
				m_Watcher.Renamed += (RenamedEventHandler)DispatchService.GuiDispatch(new RenamedEventHandler(OnGuiFileRenamed));
				m_Watcher.EnableRaisingEvents = true;

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

		void OnGuiFileRenamed (object o, RenamedEventArgs args)
		{
			ProjectNodeBuilder.FilesChanged(m_Project);
			foreach (QyotoForm form in m_Forms) {
				if (form.UiFileName == args.OldName) {
					form.UiFileName = args.Name;
					break;
				}
			}
		}

		void OnGuiFileDeleted (object o, FileSystemEventArgs args)
		{
			// XXX: Do something??
		}

		void OnGuiFileChanged (object o, FileSystemEventArgs args)
		{
			foreach (QyotoForm form in m_Forms) {
				if (form.UiFileName == args.Name) {
					form.ParseFile();
					break;
				}
			}
		}

		void OnFormAdded (object o, QyotoForm form)
		{
		}
		
		void OnFormRemoved (object o, QyotoForm form)
		{
			m_Project.Files.Remove(form.GeneratedSourceCodeFile);
			ProjectNodeBuilder.FilesChanged(m_Project);
			IdeApp.ProjectOperations.Save(m_Project);
		}
	}
}
