// QyotoFormNodeBuilder.cs
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
	public class QyotoFormNodeBuilder : TypeNodeBuilder
	{
		public QyotoFormNodeBuilder()
		{
		}
	
		public override Type CommandHandlerType {
			get { return typeof(QyotoFormCommandHandler); }
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

		public override string ContextMenuAddinPath {
			get { return "/QyotoDevelop/ContextMenu/ProjectPad/Form"; }
		}
	}

	class QyotoFormCommandHandler : NodeCommandHandler
	{
		public override void ActivateItem ()
		{
			QyotoForm form = (QyotoForm)CurrentNode.DataItem;
			if (form == null)
				return;

			if (form.SourceCodeFile != null)
				IdeApp.Workbench.OpenDocument(form.SourceCodeFile, true);
			// XXX: else show error?
		}

		public override void DeleteItem ()
		{
			QyotoForm form = (QyotoForm)CurrentNode.DataItem;
			if (form == null)
				return;

			// XXX: Add confirmation.

			ProjectFile file = form.Project.GetProjectFile(form.SourceCodeFile);
			if (file != null)
				form.Project.Files.Remove(file);
			
			QyotoDesignInfo info = QyotoDesignInfo.FromProject(form.Project);
			info.Forms.Remove(form);
		}
		
		[CommandHandler(QyotoDevelop.Commands.OpenInDesigner)]
		public void OpenInDesigner()
		{
			QyotoForm form = (QyotoForm)CurrentNode.DataItem;
			if (form == null)
				return;

			string guiPath = QyotoDesignInfo.FromProject(form.Project).QtGuiFolder;
			System.Diagnostics.Process.Start("designer-qt4", System.IO.Path.Combine(guiPath, form.UiFileName));
		}		
	}
}
