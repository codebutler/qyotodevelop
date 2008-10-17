// QyotoFormPropertyProvider.cs created with MonoDevelop
// User: eric at 11:09 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.ComponentModel;
using MonoDevelop.Projects;
using MonoDevelop.DesignerSupport;

namespace QyotoDevelop
{	
	public class QyotoFormPropertyProvider : IPropertyProvider
	{
		public object CreateProvider (object obj)
		{
			return new QyotoFormPropertyDescriptor((QyotoForm)obj);
		}

		public bool SupportsObject (object obj)
		{
			return obj is QyotoForm;
		}
	}
}
