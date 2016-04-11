using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.NRefactory;
using System.IO;
using System.Windows;

namespace CsScriptManaged.UI.CodeWindow
{
    internal class EntityWrapper<T>
        where T : IEntity
    {
        private CSharpAmbience ambience = new CSharpAmbience();

        public EntityWrapper(T entity)
        {
            Entity = entity;
            ambience.ConversionFlags = ConversionFlags.StandardConversionFlags;
        }

        public T Entity { get; private set; }

        public CompletionDataType CompletionDataType
        {
            get
            {
                switch (Entity.SymbolKind)
                {
                    default:
                    case SymbolKind.None:
                        return CompletionDataType.Unknown;
                    case SymbolKind.Property:
                    case SymbolKind.Accessor:
                        return Entity.IsStatic ? CompletionDataType.StaticProperty : CompletionDataType.Property;
                    case SymbolKind.Constructor:
                    case SymbolKind.Destructor:
                    case SymbolKind.Method:
                    case SymbolKind.Indexer:
                    case SymbolKind.Operator:
                        return Entity.IsStatic ? CompletionDataType.StaticMethod : CompletionDataType.Method;
                    case SymbolKind.Namespace:
                        return CompletionDataType.Namespace;
                    case SymbolKind.Parameter:
                    case SymbolKind.Variable:
                    case SymbolKind.Field:
                        return Entity.IsStatic ? CompletionDataType.StaticVariable : CompletionDataType.Variable;
                    case SymbolKind.TypeDefinition:
                    case SymbolKind.TypeParameter:
                        return Entity.IsStatic ? CompletionDataType.StaticClass : CompletionDataType.Class;
                    case SymbolKind.Event:
                        return CompletionDataType.Event;
                }
            }
        }

        public string AmbienceDescription
        {
            get
            {
                return ambience.ConvertSymbol(Entity);
            }
        }

        internal object GetDocumentationWithParameter(int currentParameter)
        {
            string xmlDocumentation = Entity.Documentation?.Xml?.Text;

            if (xmlDocumentation != null)
            {
                var panel = new StackPanel();
                panel.Orientation = Orientation.Vertical;

                var xml = ParseXml(xmlDocumentation);
                var documentationBlock = new TextBlock();
                var summary = xml.SelectSingleNode("//my_custom_xml_wrapper/summary");

                documentationBlock.Text = FixupXmlText(summary.InnerText);
                panel.Children.Add(documentationBlock);

                // Find parameter name
                var method = Entity as IMethod;

                if (currentParameter >= 0 && method != null && currentParameter < method.Parameters.Count)
                {
                    var parameterName = method.Parameters[currentParameter].Name;
                    var parameter = xml.SelectNodes("//my_custom_xml_wrapper/param").Cast<XmlNode>().Where(n => n.Attributes.Cast<XmlAttribute>().Where(a => a.Name == "name" && a.Value == parameterName).Any()).FirstOrDefault();

                    if (parameter != null)
                    {
                        // Add parameter comment to the panel
                        var parameterPanel = new StackPanel();
                        parameterPanel.Orientation = Orientation.Horizontal;
                        panel.Children.Add(parameterPanel);

                        var parameterNameBlock = new TextBlock();
                        parameterNameBlock.Text = string.Format("{0}: ", parameterName);
                        parameterNameBlock.FontStyle = FontStyles.Italic;
                        parameterNameBlock.FontWeight = FontWeights.Bold;
                        parameterPanel.Children.Add(parameterNameBlock);

                        var parameterDocumentationBlock = new TextBlock();
                        parameterDocumentationBlock.Text = FixupXmlText(parameter.InnerText);
                        parameterPanel.Children.Add(parameterDocumentationBlock);
                    }
                }

                return panel;
            }

            return null;
        }

        public object EntityDescription
        {
            get
            {
                return CreateEntityDescription(AmbienceDescription, Entity.Documentation?.Xml?.Text);
            }
        }

        public static object CreateEntityDescription(ISymbol symbol, string xmlDocumentation = null)
        {
            CSharpAmbience ambience = new CSharpAmbience();
            ambience.ConversionFlags = ConversionFlags.StandardConversionFlags;

            return CreateEntityDescription(ambience.ConvertSymbol(symbol), xmlDocumentation);
        }

        public object GetSyntaxWithParameterHighlighted(int currentParameter)
        {
            var codeControl = new CsTextEditor();
            codeControl.IsEnabled = false;
            codeControl.Text = AmbienceDescription;
            codeControl.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            codeControl.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            codeControl.Background = Brushes.Transparent;

            var finder = new FindParameterTokenWriter(currentParameter);
            ambience.ConvertSymbol(Entity, finder, FormattingOptionsFactory.CreateSharpDevelop());
            codeControl.Select(finder.ParameterStart, finder.ParameterEnd - finder.ParameterStart);
            return codeControl;
        }

        public static object CreateEntityDescription(string ambienceDescription, string xmlDocumentation = null)
        {
            try
            {
                var panel = new StackPanel();
                panel.Orientation = Orientation.Vertical;

                var codeControl = new CsTextEditor();
                codeControl.IsEnabled = false;
                codeControl.Text = ambienceDescription;
                codeControl.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                codeControl.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                codeControl.Background = Brushes.Transparent;
                panel.Children.Add(codeControl);

                if (xmlDocumentation != null)
                {
                    var xml = ParseXml(xmlDocumentation);
                    var documentationBlock = new TextBlock();
                    var summary = xml.SelectSingleNode("//my_custom_xml_wrapper/summary");

                    documentationBlock.Text = FixupXmlText(summary.InnerText);
                    panel.Children.Add(documentationBlock);
                }

                return panel;
            }
            catch (Exception)
            {
                return ambienceDescription;
            }
        }

        private static XmlDocument ParseXml(string xml)
        {
            XmlDocument document = new XmlDocument();

            document.LoadXml("<my_custom_xml_wrapper>" + xml + "</my_custom_xml_wrapper>");
            return document;
        }

        private static string FixupXmlText(string text)
        {
            string[] tokens = text.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            return string.Join("\n", tokens.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));
        }

        private class FindParameterTokenWriter : TextWriterTokenWriter
        {
            private int parameterIndex;
            private int currentParameter;
            private StringBuilder sb;

            public FindParameterTokenWriter(int parameterIntex)
                : this(new StringBuilder(), parameterIntex)
            {
            }

            private FindParameterTokenWriter(StringBuilder sb, int parameterIndex)
                : base(new StringWriter(sb))
            {
                this.sb = sb;
                this.parameterIndex = parameterIndex;
            }

            public int ParameterStart { get; private set; }

            public int ParameterEnd { get; private set; }

            public override void StartNode(AstNode node)
            {
                if (currentParameter == parameterIndex && node is ParameterDeclaration)
                    ParameterStart = sb.Length;
                base.StartNode(node);
            }

            public override void EndNode(AstNode node)
            {
                base.EndNode(node);
                if (node is ParameterDeclaration)
                {
                    if (parameterIndex == currentParameter)
                        ParameterEnd = sb.Length;
                    currentParameter++;
                }
            }
        }
    }
}
