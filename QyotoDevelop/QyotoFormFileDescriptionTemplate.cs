// QyotoFormFileDescriptionTemplate.cs created with MonoDevelop
// User: eric at 11:58 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

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
		QyotoCodeFileDescriptionTemplate m_FileTemplate;
		XmlNode m_UiNode;
		
		public override string Name {
			get { return "QyotoForm"; }
		}

		public override void Load (XmlElement formNode)
		{
			m_UiNode = formNode.SelectSingleNode("ui");
			if (m_UiNode == null)
				throw new Exception("<ui> missing in template");
			
			XmlNode fileNode = formNode.SelectSingleNode("QyotoCodeFile");
			if (fileNode == null)
				throw new Exception("<QyotoCodeFile> missing in template");
			
			m_FileTemplate = (QyotoCodeFileDescriptionTemplate)FileDescriptionTemplate.CreateTemplate((XmlElement)fileNode);

			// XXX: This may need to be translated from the C++ name to C# eventually.
			m_FileTemplate.BaseClass = m_UiNode.SelectSingleNode("widget").Attributes["class"].Value;
		}

		public override bool SupportsProject (Project project, string projectPath)
		{
			return Util.SupportsDesigner(project);
		}

		public override bool AddToProject (Project project, string language, string directory, string name)
		{
			// Create .cs file
			string sourceFileName = m_FileTemplate.GetFileName(project, language, directory, name);
			m_FileTemplate.AddFileToProject(project, language, directory, name);
			ProjectDomService.Parse(project, sourceFileName, null); // XXX: Shouldn't this be part of AddFileToProject?

			QyotoDesignInfo info = (QyotoDesignInfo)project.ExtendedProperties["QyotoDesignInfo"];
			if (info == null) {
				QyotoDesignInfo.EnableProject(project);
				info = (QyotoDesignInfo)project.ExtendedProperties["QyotoDesignInfo"];
			}

			string className = m_UiNode.SelectSingleNode("widget").Attributes["name"].Value;
			string uiFileName = Path.Combine(info.QtGuiFolder, String.Format("{0}.ui", className));
			
			// Create .ui file
			using (StreamWriter writer = new StreamWriter(uiFileName)) {
				writer.Write(m_UiNode.OuterXml);
			}			

			info.Forms.Add(new QyotoForm(uiFileName, className));	

			return true;
		}
		
		public override void Show ()
		{
			m_FileTemplate.Show();
		}
	}
}
