// FormNodeBuilder.cs created with MonoDevelop
// User: eric at 11:29 PM 10/16/2008
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
	public class FormNodeBuilder : TypeNodeBuilder
	{
		public FormNodeBuilder()
		{
		}

		public override string GetNodeName(ITreeNavigator thisNode, object dataObject)
		{
			QyotoForm form = (QyotoForm)dataObject;
			return form.ClassName;
		}

		public override Type NodeDataType {
			get { return typeof(QyotoForm); }
		}

		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, ref string label, ref Gdk.Pixbuf icon, ref Gdk.Pixbuf closedIcon)
		{
			QyotoForm form = (QyotoForm)dataObject;
			label = form.ClassName;
			icon = IdeApp.Services.Resources.GetIcon ("md-gtkcore-widget", Gtk.IconSize.Menu);			
		}
	}
}
