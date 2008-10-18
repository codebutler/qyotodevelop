// QyotoStartupCommand.cs
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
using MonoDevelop.Components.Commands;

namespace QyotoDevelop
{
	public class QyotoStartupCommand : CommandHandler
	{		
		protected override void Run()
		{
			ReferenceManager.Initialize();
			QyotoDevelopService.Initialize();
		}
	}

	public static class QyotoDevelopService
	{
		public static void Initialize ()
		{
			IdeApp.Workspace.SolutionLoaded   += OnSolutionLoaded;
			IdeApp.Workspace.SolutionUnloaded += OnSolutionUnloaded;
		}

		static void OnSolutionLoaded (object o, SolutionEventArgs args)
		{
			args.Solution.SolutionItemAdded += OnSolutionItemAdded;
			foreach (SolutionItem item in args.Solution.Items) {
				if (item is DotNetProject) {
					OnProjectAdded((DotNetProject)item);
				}
			}
		}

		static void OnSolutionUnloaded (object o, SolutionEventArgs args)
		{
			args.Solution.SolutionItemAdded -= OnSolutionItemAdded;
		}

		static void OnSolutionItemAdded (object o, SolutionItemChangeEventArgs args)
		{
			if (args.SolutionItem is DotNetProject) {
				OnProjectAdded((DotNetProject)args.SolutionItem);
			}
		}

		static void OnProjectAdded (DotNetProject project)
		{
			QyotoDesignInfo info = (QyotoDesignInfo)project.ExtendedProperties["QyotoDesignInfo"];
			if (info != null) {
				info.Project = project;
			}
		}
	}
}
