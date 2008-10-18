// QyotoFormsFolderNodeBuilder.cs
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
	public class QyotoFormsFolderNodeBuilder : TypeNodeBuilder
	{
		public QyotoFormsFolderNodeBuilder()
		{
		}

		public override Type CommandHandlerType {
			get { return typeof(QyotoCommandHandler); }
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
		
		public override string ContextMenuAddinPath {
			get { return "/QyotoDevelop/ContextMenu/ProjectPad/FormsFolder"; }
		}
	}
}
