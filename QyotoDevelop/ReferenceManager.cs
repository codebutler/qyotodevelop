// ReferenceManager.cs
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

using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

namespace QyotoDevelop
{	
	public static class ReferenceManager
	{
		public static void Initialize ()
		{
			IdeApp.Workspace.ReferenceAddedToProject     += OnReferenceAdded;
			IdeApp.Workspace.ReferenceRemovedFromProject += OnReferenceRemoved;
		}
		
		static void OnReferenceAdded (object o, ProjectReferenceEventArgs args)
		{
			if (!Util.IsQyotoReference(args.ProjectReference))
				return;
			
			Project project = args.Project as DotNetProject;
			if (project != null && project.ExtendedProperties["QyotoDesignInfo"] == null)
				QyotoDesignInfo.EnableProject(project);
		}

		static void OnReferenceRemoved (object o, ProjectReferenceEventArgs args)
		{
			if (!Util.IsQyotoReference(args.ProjectReference))
			    return;

			DotNetProject dnp = args.Project as DotNetProject;

			if (dnp != null) {
				if (MonoDevelop.Core.Gui.MessageService.Confirm("Qyoto features will be disabled by removing the qt-dotnet reference.", new MonoDevelop.Core.Gui.AlertButton("Disable Qyoto")))
					QyotoDesignInfo.DisableProject(dnp);
				else
					dnp.References.Add(new ProjectReference(ReferenceType.Gac, args.ProjectReference.StoredReference));
			}
		}
	}
}
