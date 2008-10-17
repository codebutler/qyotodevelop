// QyotoForm.cs created with MonoDevelop
// User: eric at 10:37 PM 10/16/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

using MonoDevelop.Core.Serialization;

namespace QyotoDevelop
{
	public class QyotoForm
	{
		string m_ClassName;
		string m_UiFileName;
		string m_Namespace;
		
		public QyotoForm()
		{
			/* For the serializer */
		}

		public QyotoForm(string uiFileName, string className)
		{
			m_UiFileName = uiFileName;
			m_ClassName = className;
		}

		[ItemProperty("Namespace")]
		public string Namespace {
			get {
				return m_Namespace;
			}
			set {
				m_Namespace = value;
			}
		}

		[ItemProperty("ClassName")]
		public string ClassName {
			get {
				return m_ClassName;
			}
			set {
				m_ClassName = value;
			}
		}

		[ItemProperty("UiFileName")]
		public string UiFileName {
			get {
				return m_UiFileName;
			}
			set {
				m_UiFileName = value;
			}
		}
	}
}
