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
using System.Collections.Generic;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Xml;

namespace QyotoDevelop
{
	public static class QyotoCodeGenerator
	{		
		public static void GenerateFormCodeFile(QyotoForm form)
		{
			CodeCompileUnit unit = new CodeCompileUnit();

			CodeNamespace nspace = new CodeNamespace(form.Namespace);
			nspace.Imports.Add(new CodeNamespaceImport("System"));
			nspace.Imports.Add(new CodeNamespaceImport("Qyoto"));
			unit.Namespaces.Add(nspace);
			
			CodeTypeDeclaration type = new CodeTypeDeclaration(form.ClassName);
			type.IsClass = true;
			type.IsPartial = true;
			type.TypeAttributes = TypeAttributes.Public;
			type.BaseTypes.Add(new CodeTypeReference(form.BaseTypeName));

			nspace.Types.Add(type);

			CodeMemberMethod setupUiMethod = new CodeMemberMethod();
			setupUiMethod.Name = "SetupUi";
			setupUiMethod.Attributes = MemberAttributes.Family | MemberAttributes.Final;
			type.Members.Add(setupUiMethod);

			setupUiMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), "ObjectName"), 
			                                                     new CodePrimitiveExpression(form.ClassName)));

			XmlDocument doc = new XmlDocument();
			doc.Load(form.UiFile);			

			XmlElement widgetNode = (XmlElement)doc.SelectSingleNode("/ui/widget");
			ParseWidget(widgetNode, setupUiMethod, type, null, null);

			foreach (XmlElement node in doc.SelectNodes("/ui/connections/connection")) {
				string sender   = node.SelectSingleNode("sender").InnerText;
				string signal   = node.SelectSingleNode("signal").InnerText;
				string receiver = node.SelectSingleNode("receiver").InnerText;
				string slot     = node.SelectSingleNode("slot").InnerText;

				CodeExpression senderExpression = null;
				if (sender == type.Name)
					senderExpression = new CodeThisReferenceExpression();
				else
					senderExpression = new CodeVariableReferenceExpression(sender);

				CodeExpression receiverExpression = null;
				if (receiver == type.Name)
					receiverExpression = new CodeThisReferenceExpression();
				else
					receiverExpression = new CodeVariableReferenceExpression(receiver);
						
				setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("QObject"),
				                                                            "Connect", 
				                                                            senderExpression,
				                                                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Qt"), "SIGNAL", new CodePrimitiveExpression(signal)),
				                                                            receiverExpression, 
				                                                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Qt"), "SLOT", new CodePrimitiveExpression(slot))));
			}

			setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("QMetaObject"),
			                                                            "ConnectSlotsByName",
			                                                            new CodeThisReferenceExpression()));

			using (TextWriter writer = new StreamWriter(form.GeneratedSourceCodeFile)) {
				CodeDomProvider provider = form.Project.LanguageBinding.GetCodeDomProvider();
				provider.GenerateCodeFromCompileUnit(unit, writer, new CodeGeneratorOptions());
			}
		}

		static void ParseWidget(XmlElement widgetNode, CodeMemberMethod setupUiMethod, CodeTypeDeclaration formClass, CodeExpression parentWidgetReference, CodeExpression parentLayoutReference)
		{
			ParseWidget(widgetNode, setupUiMethod, formClass, parentWidgetReference, parentLayoutReference, null);
		}
		
		static void ParseWidget(XmlElement widgetNode, CodeMemberMethod setupUiMethod, CodeTypeDeclaration formClass, CodeExpression parentWidgetReference, CodeExpression parentLayoutReference, XmlElement parentItemNode)
		{
			string widgetClass = widgetNode.GetAttribute("class");
			string widgetName  = widgetNode.GetAttribute("name");
			
			CodeExpression widgetReference;
			
			if (parentWidgetReference == null) {
				widgetReference = new CodeThisReferenceExpression();
			} else {
				if (widgetClass == "Line")
					widgetClass = "QFrame";
				
				CodeMemberField widgetField = new CodeMemberField(widgetClass, widgetName);
				widgetField.Attributes = MemberAttributes.Family;
				formClass.Members.Add(widgetField);
				widgetReference = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), widgetName);

				setupUiMethod.Statements.Add(new CodeAssignStatement(widgetReference, new CodeObjectCreateExpression(widgetClass, parentWidgetReference)));
				setupUiMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(widgetReference, "ObjectName"), new CodePrimitiveExpression(widgetName)));
			}

			ParseProperties(widgetNode, widgetReference, setupUiMethod, widgetReference);

			XmlElement layoutNode = (XmlElement)widgetNode.SelectSingleNode("layout");
			if (layoutNode != null) {
				ParseLayout(layoutNode, formClass, setupUiMethod, widgetReference, null);
			}

			if (parentWidgetReference != null) {
				if (parentItemNode == null || parentItemNode.Attributes["row"] == null || parentItemNode.Attributes["column"] == null) {
					if (parentLayoutReference != null) {
						setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(parentLayoutReference, "AddWidget", widgetReference));
					} else {
						var parentWidgetNode = (XmlElement)widgetNode.ParentNode;
						if (parentWidgetNode.GetAttribute("class") == "QTabWidget") {
							string tabLabel = widgetNode.SelectSingleNode("attribute[@name='title']/string").InnerText;
							setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(parentWidgetReference, "AddTab", widgetReference, 
						                                                                new CodePrimitiveExpression(tabLabel)));
						} else if (parentWidgetNode.GetAttribute("class") == "QScrollArea") {
							setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(parentWidgetReference, "SetWidget", widgetReference));
						} else {
							setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(parentWidgetReference, "AddWidget", widgetReference));
						}
					}
				} else {					
					int row    = Convert.ToInt32(parentItemNode.GetAttribute("row"));
					int column = Convert.ToInt32(parentItemNode.GetAttribute("column"));
					
					var parentLayoutNode = (XmlElement)parentItemNode.ParentNode;
					if (parentLayoutNode.GetAttribute("class") == "QFormLayout") {
						
						var roleName = (column == 0) ? "LabelRole" : "FieldRole";
						var roleExpression = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("QFormLayout.ItemRole"), roleName);
						setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(parentLayoutReference, "SetWidget",
						                                                            new CodePrimitiveExpression(row),
						                                                            roleExpression,
						                                                            widgetReference));		
					} else {
						var colSpan = parentItemNode.Attributes["colspan"] != null ? Convert.ToInt32(parentItemNode.GetAttribute("colspan")) : 1;
						var rowSpan = parentItemNode.Attributes["rowspan"] != null ? Convert.ToInt32(parentItemNode.GetAttribute("rowspan")) : 1;
						setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(parentLayoutReference, 
							                                                        "AddWidget",
							                                                         widgetReference,
							                                                         new CodePrimitiveExpression(row),
							                                                         new CodePrimitiveExpression(column),
							                                                         new CodePrimitiveExpression(rowSpan),
							                                                         new CodePrimitiveExpression(colSpan)));
					}
				}
			}

			foreach (XmlElement childWidgetNode in widgetNode.SelectNodes("widget")) {
				ParseWidget(childWidgetNode, setupUiMethod, formClass, widgetReference, null);
			}
		}

		static void ParseLayout(XmlElement layoutNode, CodeTypeDeclaration formClass, CodeMemberMethod setupUiMethod, CodeExpression widgetReference, CodeExpression parentLayout)
		{
			string layoutName  = layoutNode.GetAttribute("name");
			string layoutClass = layoutNode.GetAttribute("class");

			CodeVariableReferenceExpression layoutReference = new CodeVariableReferenceExpression(layoutName);

			setupUiMethod.Statements.Add(new CodeVariableDeclarationStatement(layoutClass, layoutName));
			if (parentLayout != null) {
				setupUiMethod.Statements.Add(new CodeAssignStatement(layoutReference, new CodeObjectCreateExpression(layoutClass)));

				var parentItemNode = (XmlElement)layoutNode.ParentNode;
				var parentLayoutNode = (XmlElement)parentItemNode.ParentNode;
				if (parentLayoutNode.GetAttribute("class") == "QGridLayout") {
					int row     = Convert.ToInt32(parentItemNode.GetAttribute("row"));
					int column  = Convert.ToInt32(parentItemNode.GetAttribute("column"));
					var colSpan = parentItemNode.Attributes["colspan"] != null ? Convert.ToInt32(parentItemNode.GetAttribute("colspan")) : 1;
					var rowSpan = parentItemNode.Attributes["rowspan"] != null ? Convert.ToInt32(parentItemNode.GetAttribute("rowspan")) : 1;
					setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(parentLayout, "AddLayout", layoutReference,
												    new CodePrimitiveExpression(row),
												    new CodePrimitiveExpression(column),
												    new CodePrimitiveExpression(rowSpan),
												    new CodePrimitiveExpression(colSpan)));
				} else {
					setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(parentLayout, "AddLayout", layoutReference));
				}
			} else {
				setupUiMethod.Statements.Add(new CodeAssignStatement(layoutReference, new CodeObjectCreateExpression(layoutClass, widgetReference)));
			}
			
			ParseProperties(layoutNode, layoutReference, setupUiMethod, layoutReference);

			foreach (XmlElement itemNode in layoutNode.SelectNodes("item")) {
				XmlElement itemChildNode = (XmlElement)itemNode.FirstChild;
				if (itemChildNode.Name == "widget")
					ParseWidget(itemChildNode, setupUiMethod, formClass, widgetReference, layoutReference, itemNode);
				else if (itemChildNode.Name == "layout")
					ParseLayout(itemChildNode, formClass, setupUiMethod, widgetReference, layoutReference);
				else if (itemChildNode.Name == "spacer") {
					string spacerName = itemChildNode.GetAttribute("name");
					int width  = Convert.ToInt32(itemChildNode.SelectSingleNode("property[@name='sizeHint']/size/width").InnerText);
					int height = Convert.ToInt32(itemChildNode.SelectSingleNode("property[@name='sizeHint']/size/height").InnerText);
					string orientation = itemChildNode.SelectSingleNode("property[@name='orientation']/enum").InnerText;
					string hpolicy, vpolicy = null;
					if (orientation == "Qt::Vertical") {
						hpolicy = "Minimum";
						vpolicy = "Expanding";
					} else if (orientation == "Qt::Horizontal") {
						hpolicy = "Expanding";
						vpolicy = "Minimum";						
					} else {
						throw new Exception("Unknown orientation: " + orientation);
					}
					CodeVariableReferenceExpression spacerReference = new CodeVariableReferenceExpression(spacerName);
					setupUiMethod.Statements.Add(new CodeVariableDeclarationStatement("QSpacerItem", spacerName));
					setupUiMethod.Statements.Add(new CodeAssignStatement(spacerReference,
					                                                     new CodeObjectCreateExpression("QSpacerItem",
					                                                                                    new CodePrimitiveExpression(width), 
					                                                                                    new CodePrimitiveExpression(height),
					                                                                                    new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("QSizePolicy.Policy"), hpolicy), 
					                                                                                    new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("QSizePolicy.Policy"), vpolicy))));
					setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(layoutReference, "AddItem", spacerReference));
				} else
					throw new Exception(String.Format("Failed to generate {0}. Expected <widget> or <layout>. Got: {1}", formClass.Name, itemChildNode.Name));
			}
		}

		static void ParseProperties(XmlElement parentNode, CodeExpression parentObjectReference, CodeMemberMethod setupUiMethod, CodeExpression itemReference)
		{
			int leftMargin = 0, rightMargin = 0, topMargin = 0, bottomMargin = 0;
			
			foreach (XmlElement propertyNode in parentNode.SelectNodes("property")) {
				string name = TranslatePropertyName(propertyNode.GetAttribute("name"), false);
				XmlElement propertyValueNode = (XmlElement)propertyNode.FirstChild;
				if (propertyValueNode.Name == "sizepolicy") {
					string hpolicy = propertyValueNode.GetAttribute("hsizetype");
					string vpolicy = propertyValueNode.GetAttribute("vsizetype");
					int hstretch   = Convert.ToInt32(propertyValueNode.SelectSingleNode("horstretch").InnerText);
					int vstretch   = Convert.ToInt32(propertyValueNode.SelectSingleNode("verstretch").InnerText);
					
					CodeExpression hpolicyExpr = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("QSizePolicy.Policy"), hpolicy);
					CodeExpression vpolicyExpr = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("QSizePolicy.Policy"), vpolicy);
					
					//CodeExpression sizePolicyReference = new CodePropertyReferenceExpression(itemReference, "SizePolicy");
					//setupUiMethod.Statements.Add(new CodeAssignStatement(sizePolicyReference, new CodeObjectCreateExpression("QSizePolicy", hpolicyExpr, vpolicyExpr)));
					
					string objectName = parentNode.GetAttribute("name");
					setupUiMethod.Statements.Add(new CodeVariableDeclarationStatement("QSizePolicy", objectName + "_sizePolicy"));
					CodeExpression sizePolicyReference = new CodeVariableReferenceExpression(objectName + "_sizePolicy");
					setupUiMethod.Statements.Add(new CodeAssignStatement(sizePolicyReference, new CodeObjectCreateExpression("QSizePolicy", hpolicyExpr, vpolicyExpr)));
					
					setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(sizePolicyReference, "SetVerticalStretch", new CodePrimitiveExpression(vstretch)));
					setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(sizePolicyReference, "SetHorizontalStretch", new CodePrimitiveExpression(hstretch)));

					setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(sizePolicyReference,"SetHeightForWidth", 
					                                                            new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(parentObjectReference, "SizePolicy"), "HasHeightForWidth")));

					setupUiMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(parentObjectReference, "SizePolicy"),
					                                                     sizePolicyReference));
				} else if (name == "LeftMargin") {
					leftMargin = Convert.ToInt32(propertyValueNode.InnerText);
				} else if (name == "RightMargin") {
					rightMargin = Convert.ToInt32(propertyValueNode.InnerText);
				} else if (name == "TopMargin") {
					topMargin = Convert.ToInt32(propertyValueNode.InnerText);
				} else if (name == "BottomMargin") {
					bottomMargin = Convert.ToInt32(propertyValueNode.InnerText);
				} else if (name == "Orientation" && parentNode.GetAttribute("class") == "Line") {

					CodeExpression valueExpr = null;
					if (propertyNode.InnerText == "Qt::Vertical")
						valueExpr = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("QFrame.Shape"), 
						                                             "VLine");
					else
						valueExpr = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("QFrame.Shape"),
						                                             "HLine");
					
					setupUiMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(itemReference, "FrameShape"),
                                                     valueExpr));

					setupUiMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(itemReference, "FrameShadow"),
                                                     new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("QFrame.Shadow"), "Sunken")));

				} else if (name == "Buddy") {
					setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(itemReference, "SetBuddy"), 
					                                                            TranslatePropertyValue(propertyNode)));
				} else {
					setupUiMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(itemReference, name), 
					                                                     TranslatePropertyValue(propertyNode)));
				}
			}

			if (leftMargin != 0 || topMargin != 0 || bottomMargin != 0 || rightMargin != 0) {
				setupUiMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(itemReference, "SetContentsMargins"),
				                                                            new CodePrimitiveExpression(leftMargin),
				                                                            new CodePrimitiveExpression(topMargin),
				                                                            new CodePrimitiveExpression(rightMargin),
				                                                            new CodePrimitiveExpression(bottomMargin)));
			}
		}
		
		static string TranslatePropertyName (string cname, bool alwaysUppercaseFirstLetter)
		{
			List<string> lowerCasePropertyNames = new List<string>();
			lowerCasePropertyNames.AddRange(new string[] {
			    "acceptMode",
			    "buttonSymbols",
			    "cacheMode",
			    "completionMode",
			    "correctionMode",
			    "curveShape",
			    "direction",
			    "dragDropMode",
			    "dragMode",
			    "echoMode",
			    "fieldGrowthPolicy",
			    "fileMode",
			    "flow",
			    "horizontalHeaderFormat",
			    "icon",
			    "insertPolicy",
			    "itemIndexMethod",
			    "layoutMode",
			    "lineWrapMode",
			    "menuRole",
			    "mode",
			    "modelSorting",
			    "movement",
			    "notation",
			    "resizeMode",
			    "segmentStyle",
			    "selectionBehavior",
			    "selectionMode",
			    "shape",
			    "sizeAdjustPolicy",
			    "sizeConstraint",
			    "submitPolicy",
			    "tabPosition",
			    "tabShape",
			    "tickPosition",
			    "verticalHeaderFormat",
			    "viewMode",
			    "viewportUpdateMode",
			    "wizardStyle"
			});

			if (!alwaysUppercaseFirstLetter && lowerCasePropertyNames.Contains(cname))
					return cname;
			else
				return cname.Substring(0,1).ToUpper() + cname.Substring(1);
		}
	
		static CodeExpression TranslatePropertyValue (XmlElement propertyNode)
		{
			XmlElement propertyValueNode = (XmlElement)propertyNode.FirstChild;
			
			switch (propertyValueNode.Name) {
			case "string":
				return new CodePrimitiveExpression(propertyValueNode.InnerText);
			case "bool":
				return new CodePrimitiveExpression(propertyValueNode.InnerText == "true");
			case "number":
				return new CodePrimitiveExpression(Convert.ToInt32(propertyValueNode.InnerText));
			case "url":
				return new CodeObjectCreateExpression("QUrl", new CodePrimitiveExpression(propertyValueNode.InnerText));
			case "rect":
				int rectX      = Convert.ToInt32(propertyValueNode.SelectSingleNode("x").InnerText);
				int rectY      = Convert.ToInt32(propertyValueNode.SelectSingleNode("y").InnerText);
				int rectWidth  = Convert.ToInt32(propertyValueNode.SelectSingleNode("width").InnerText);
				int rectHeight = Convert.ToInt32(propertyValueNode.SelectSingleNode("height").InnerText);
				return new CodeObjectCreateExpression("QRect", new CodePrimitiveExpression(rectX), new CodePrimitiveExpression(rectY),
				                                      new CodePrimitiveExpression(rectWidth), new CodePrimitiveExpression(rectHeight));
			case "size":
				int sizeWidth  = Convert.ToInt32(propertyValueNode.SelectSingleNode("width").InnerText);
				int sizeHeight = Convert.ToInt32(propertyValueNode.SelectSingleNode("height").InnerText);
				return new CodeObjectCreateExpression("QSize", new CodePrimitiveExpression(sizeWidth), new CodePrimitiveExpression(sizeHeight));
			case "iconset":
				return new CodeObjectCreateExpression("QIcon", new CodePrimitiveExpression(propertyValueNode.InnerText));
			case "enum":
					string val = propertyValueNode.InnerText;
					if (val.Contains("::")) {
						string className = val.Substring(0, val.IndexOf("::"));
						string enumName = TranslatePropertyName(propertyNode.GetAttribute("name"), true);
						if (enumName == "FrameShadow")
							enumName = "Shadow";
						else if (enumName == "FrameShape")
							enumName = "Shape";
						else if (enumName == "HorizontalScrollBarPolicy")
							enumName = "ScrollBarPolicy";
						else if (enumName == "VerticalScrollBarPolicy")
							enumName = "ScrollBarPolicy";
						string enumValue = val.Substring(val.IndexOf("::") + 2);
				 		return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(String.Format("{0}.{1}", className, enumName)), enumValue);
					} else {
						throw new NotSupportedException();
					}
			case "set":
				CodeExpression result = null;				
				foreach (string cenum in propertyValueNode.InnerText.Split('|')) {
					string enumName = cenum.Substring(0, cenum.IndexOf("::"));
					string enumValue = cenum.Substring(cenum.IndexOf("::") + 2);
					CodeExpression thisExpr = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("global::Qyoto.Qyoto"), 
						                                                     "GetCPPEnumValue",
					                                                         new CodePrimitiveExpression(enumName), 
					                                                         new CodePrimitiveExpression(enumValue));
					if (result == null)
						result = thisExpr;
					else
						result = new CodeBinaryOperatorExpression(result, CodeBinaryOperatorType.BitwiseOr, thisExpr);
				}
				return result;
			case "cstring":
				return new CodeVariableReferenceExpression(propertyValueNode.InnerText);
			}
			throw new Exception("Unsupported property type: " + propertyValueNode.Name);
		}
	}
}
