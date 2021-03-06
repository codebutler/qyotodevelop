// ProjectNodeBuilder.cs
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
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Components;

namespace QyotoDevelop
{
	public class ProjectNodeBuilder : NodeBuilderExtension
	{
		static ProjectNodeBuilder instance;

		public override bool CanBuildNode (Type dataType)
		{
			return typeof(DotNetProject).IsAssignableFrom (dataType);
		}
		
		protected override void Initialize ()
		{
			lock (typeof (ProjectNodeBuilder))
				instance = this;
		}
		
		public override void Dispose ()
		{
			lock (typeof (ProjectNodeBuilder))
				instance = null;
		}
		
		public override void BuildChildNodes (ITreeBuilder builder, object dataObject)
		{
			Project project = (Project)dataObject;
			if (Util.SupportsDesigner(project)) {
				QyotoDesignInfo designInfo = (QyotoDesignInfo)project.ExtendedProperties["QyotoDesignInfo"];
				if (designInfo == null) throw new InvalidOperationException("designInfo cant be null");
				builder.AddChild(designInfo);
			}
		}

		// XXX: This currently has to be manually called any time 
		// QyotoDesignInfo.Files changes. That should be done automatically.
		public static void FilesChanged (Project p)
		{
			if (p == null)
				throw new ArgumentNullException("p");
			
			if (instance == null)
				return;

			ITreeBuilder tb = instance.Context.GetTreeBuilder (p);
			if (tb != null)
				tb.UpdateAll ();
		}	
	}
}
