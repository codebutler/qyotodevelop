// QyotoProjectServiceExtension.cs
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
using System.IO;
using System.Threading;
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace QyotoDevelop
{
	public class QyotoProjectServiceExtension : ProjectServiceExtension
	{
		protected override BuildResult Build (IProgressMonitor monitor, SolutionEntityItem entry, ConfigurationSelector configuration)
		{
			DotNetProject project = (DotNetProject)entry;

			if (Util.HasDesignedObjects(project)) {
				QyotoDesignInfo info = QyotoDesignInfo.FromProject(project);
				
				monitor.Log.WriteLine(GettextCatalog.GetString("Generating GUI code for project '{0}'...", project.Name));

				foreach (QyotoForm form in info.Forms) {
				 	QyotoCodeGenerator.GenerateFormCodeFile(form);
					if (!project.IsFileInProject(form.GeneratedSourceCodeFile)) {
						project.AddFile(form.GeneratedSourceCodeFile, BuildAction.Compile);
					}
				}
			}
			
			return base.Build(monitor, entry, configuration);
		}
	}
}
