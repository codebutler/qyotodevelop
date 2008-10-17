// ProjectNodeBuilder.cs created with MonoDevelop
// User: eric at 4:19 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

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
			if (Util.HasDesignedObjects (project)) {
				QyotoDesignInfo designInfo = (QyotoDesignInfo)project.ExtendedProperties["QyotoDesignInfo"];
				if (designInfo == null) throw new InvalidOperationException("designInfo cant be null");
				builder.AddChild(designInfo);
			}
		}
		
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
