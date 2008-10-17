// QyotoFormPropertyDescriptor.cs created with MonoDevelop
// User: eric at 11:11 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.ComponentModel;
using MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.DesignerSupport;
using System.Reflection;

namespace QyotoDevelop
{
	public class QyotoFormPropertyDescriptor : CustomDescriptor
	{
		QyotoForm m_Form;
		
		public QyotoFormPropertyDescriptor(QyotoForm form)
		{
			m_Form = form;
		}

		[DisplayName("Namespace")]
		[Description("Namespace for widget class")]
		public string Namespace {
			get {
				return m_Form.Namespace;
			}
			set {
				m_Form.Namespace = value;
			}
		}
	}
}
