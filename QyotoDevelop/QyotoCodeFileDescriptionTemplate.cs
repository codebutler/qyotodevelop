// QyotoCodeFileDescriptionTemplate.cs created with MonoDevelop
// User: eric at 12:53 AM 10/17/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections;
using System.Xml;

using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Templates;


namespace QyotoDevelop
{
	public class QyotoCodeFileDescriptionTemplate : TextFileDescriptionTemplate
	{
		public string BaseClass {
			get; set;
		}
		
		public override void ModifyTags (Project project, string language, string identifier, string fileName, ref Hashtable tags)
		{
			base.ModifyTags(project, language, identifier, fileName, ref tags);

			tags["BaseClass"] = BaseClass;
		}
	}
}
