// QyotoFormFileDescriptionTemplate.cs
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
using System.Xml;
using System.IO;

using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Templates;

namespace QyotoDevelop
{	
	public class QyotoFormFileDescriptionTemplate : FileDescriptionTemplate
	{
		SingleFileDescriptionTemplate m_FileTemplate;
		XmlNode m_UiNode;
		
		public override string Name {
			get { return "QyotoForm"; }
		}

		public override void Load (XmlElement formNode)
		{
			m_UiNode = formNode.SelectSingleNode("ui");
			if (m_UiNode == null)
				throw new Exception("<ui> missing in template");
			
			XmlNode fileNode = formNode.SelectSingleNode("File");
			if (fileNode == null)
				throw new Exception("<File> missing in template");
			
			m_FileTemplate = (SingleFileDescriptionTemplate)FileDescriptionTemplate.CreateTemplate((XmlElement)fileNode);
		}

		public override bool SupportsProject (Project project, string projectPath)
		{
			return Util.SupportsDesigner(project);
		}

		public override bool AddToProject (MonoDevelop.Projects.SolutionItem policyParent, MonoDevelop.Projects.Project project, string language, string directory, string name)
		{
			// Create .cs file
			string sourceFileName = m_FileTemplate.GetFileName(policyParent, project, language, directory, name);
			m_FileTemplate.AddFileToProject(policyParent, project, language, directory, name);
			ProjectDomService.Parse(project, sourceFileName, null); // XXX: Shouldn't this be part of AddFileToProject?

			QyotoDesignInfo info = QyotoDesignInfo.FromProject(project, true);

			// Create .ui file

			string[,] tags = {
				{"Name", Path.GetFileNameWithoutExtension(name)}
			};

			string content = m_UiNode.OuterXml;
			content = StringParserService.Parse(content, tags);

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(content);
			
			QyotoForm form = QyotoForm.WriteXml(doc.DocumentElement, info);
			info.Forms.Add(form);
			ProjectNodeBuilder.FilesChanged(project);

			return true;
		}
		
		public override void Show ()
		{
			m_FileTemplate.Show();
		}
	}
}
