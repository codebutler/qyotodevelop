// FormsFolderNodeBuilder.cs created with MonoDevelop
// User: eric at 4:30 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

using MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.Core.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Ide.Gui.Components;

namespace QyotoDevelop
{
	public class FormsFolderNodeBuilder : TypeNodeBuilder
	{
		public FormsFolderNodeBuilder()
		{
		}
	
		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			return "QtForms";
		}

		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, ref string label, ref Gdk.Pixbuf icon, ref Gdk.Pixbuf closedIcon)
		{
			label = "Qt Forms";
			icon = Context.GetIcon (Stock.OpenResourceFolder);
			closedIcon = Context.GetIcon (Stock.ClosedResourceFolder);
		}

		public override void BuildChildNodes (ITreeBuilder builder, object dataObject)
		{
			QyotoDesignInfo info = (QyotoDesignInfo)dataObject;
			foreach (QyotoForm form in info.Forms) {
				builder.AddChild(form);
			}
		}
		
		public override Type NodeDataType {
			get {
				return typeof(QyotoDesignInfo);
			}
		}

		public override bool HasChildNodes (ITreeBuilder builder, object dataObject)
		{
			return true;
		}
	}
}
