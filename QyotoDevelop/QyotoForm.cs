// QyotoForm.cs
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
using System.Xml;

using MonoDevelop.Core.Serialization;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Projects.CodeGeneration;
using MonoDevelop.Core.Gui;
using MonoDevelop.Projects.Text;

namespace QyotoDevelop
{
	public class QyotoForm
	{
		string m_ClassName;
		string m_UiFileName;
		string m_BaseTypeName;
		string m_Namespace;
		DotNetProject m_Project;
		
		public QyotoForm()
		{
			/* For the serializer */
		}

		public QyotoForm(string uiFileName)
		{
			UiFileName = uiFileName;
		}

		public static QyotoForm WriteXml(XmlNode node, QyotoDesignInfo info)
		{
			string className = node.SelectSingleNode("widget").Attributes["name"].Value;
			string uiFileName = Path.Combine(info.QtGuiFolder, String.Format("{0}.ui", className));
			
			// Create .ui file
			using (StreamWriter writer = new StreamWriter(uiFileName)) {
				writer.Write(node.OuterXml);
			}
			
			uiFileName = Path.GetFileName(uiFileName);
			QyotoForm form = new QyotoForm(uiFileName);
			form.Project = info.Project;
			return form;
		}

		public DotNetProject Project {
			set {
				if (m_Project != null)
					throw new InvalidOperationException("project already set!");
				
				m_Project = value;

				ParseFile();
			}
			get {
				return m_Project;
			}
		}
		
		public string ClassName {
			get {
				return m_ClassName;
			}
		}

		public string BaseTypeName {
			get {
				return m_BaseTypeName;
			}
		}

		public string SourceCodeFile {
			get {
				string guiFolder = QyotoDesignInfo.FromProject(m_Project).QtGuiFolder;
				ProjectDom dom = ProjectDomService.GetProjectDom(m_Project);
				foreach (IType cls in dom.Types)
					if (cls.FullName == m_ClassName)
						foreach (IType part in cls.Parts)
							if (part.CompilationUnit != null && !part.CompilationUnit.FileName.FileName.StartsWith(guiFolder))
								return part.CompilationUnit.FileName;
				return null;
			}
		}

		public string GeneratedSourceCodeFile {
			get {
				string guiFolder = QyotoDesignInfo.FromProject(m_Project).QtGuiFolder;
				return Path.Combine(guiFolder, m_Project.LanguageBinding.GetFileName(m_ClassName));
			}
		}

		public string UiFile {
			get {
				return Path.Combine(QyotoDesignInfo.FromProject(m_Project).QtGuiFolder, m_UiFileName);
			}
		}
		
		[ItemProperty("UiFileName")]
		public string UiFileName {
			get {
				return m_UiFileName;
			}
			set {
				if (String.IsNullOrEmpty(value))
					throw new ArgumentNullException("value");

				if (Path.GetFileName(value) != value)
					throw new ArgumentException("value may not contain a path");
				
				m_UiFileName = value;

				if (m_Project != null)
					IdeApp.ProjectOperations.Save(m_Project);

				ParseFile();
			}
		}
		
		[ItemProperty("Namespace")]
		public string Namespace {
			get {
				return m_Namespace;
			}
			set {
				m_Namespace = value;
				
				if (m_Project != null)
					IdeApp.ProjectOperations.Save(m_Project);
			}
		}

		public void ParseFile()
		{
			if (m_Project == null)
				return;

			string fullPath = Path.Combine(QyotoDesignInfo.FromProject(m_Project).QtGuiFolder, m_UiFileName);
			
			if (!File.Exists(fullPath))
				return;
			
			using (FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read)) {
				XmlDocument doc = new XmlDocument();
				doc.Load(stream);
				m_ClassName = doc.SelectSingleNode("/ui/widget").Attributes["name"].Value;
				m_BaseTypeName = doc.SelectSingleNode("/ui/widget").Attributes["class"].Value;
			}

			// XXX: Queue code generation?
			
			ProjectNodeBuilder.FilesChanged(m_Project);
		}
	}
}
