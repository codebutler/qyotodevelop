// QyotoCommandHandler.cs
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
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Gui.Components;

namespace QyotoDevelop
{	
	public class QyotoCommandHandler : NodeCommandHandler
	{
		[CommandHandler(QyotoDevelop.Commands.AddNewDialog)]
		public void AddNewDialogToProject()
		{
			AddNewWindow("QDialogFileTemplate");
		}

		[CommandUpdateHandler(QyotoDevelop.Commands.AddNewDialog)]
		public void UpdateAddNewDialogToProject(CommandInfo cinfo)
		{
			cinfo.Visible = CanAddWindow();
		}
		
		[CommandHandler(QyotoDevelop.Commands.AddNewMainWindow)]
		public void AddNewMainWindowToProject()
		{
			AddNewWindow("QMainWindowFileTemplate");
		}

		[CommandUpdateHandler(QyotoDevelop.Commands.AddNewMainWindow)]
		public void UpdateAddNewMainWindowToProject(CommandInfo cinfo)
		{
			cinfo.Visible = CanAddWindow();
		}
		
		[CommandHandler(QyotoDevelop.Commands.AddNewWidget)]
		public void AddNewWidgetToProject()
		{
			AddNewWindow("QWidgetFileTemplate");
		}
		
		[CommandUpdateHandler(QyotoDevelop.Commands.AddNewWidget)]
		public void UpdateAddNewWidgetToProject(CommandInfo cinfo)
		{
			cinfo.Visible = CanAddWindow();
		}
		
		// XXX: Can this be moved into monodevelop? AddFileFromTemplate or something.
		private void AddNewWindow (string id)
		{
			DotNetProject project = CurrentNode.GetParentDataItem (typeof(Project), true) as DotNetProject;
			if (project == null)
				return;
			
			object dataItem = CurrentNode.DataItem;
			
			ProjectFolder folder = CurrentNode.GetParentDataItem (typeof(ProjectFolder), true) as ProjectFolder;
						
			string path;
			if (folder != null)
				path = folder.Path;
			else
				path = project.BaseDirectory;

			IdeApp.ProjectOperations.CreateProjectFile (project, path, id);
			
			IdeApp.ProjectOperations.Save (project);
			
			ITreeNavigator nav = Tree.GetNodeAtObject (dataItem);
			if (nav != null)
				nav.Expanded = true;
		}

		private bool CanAddWindow ()
		{
			DotNetProject project = CurrentNode.GetParentDataItem(typeof(Project), true) as DotNetProject;
			return Util.SupportsDesigner(project);
		}
	}
}
