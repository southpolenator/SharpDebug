using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.CSharp.Completion;

namespace CsScriptManaged.UI.CodeWindow
{
    internal class CompletionDataFactory : ICompletionDataFactory
    {
        CSharpAmbience ambience = new CSharpAmbience();

        public IEnumerable<ICompletionData> CreateCodeTemplateCompletionData()
        {
            // Do nothing
            yield break;
        }

        public ICompletionData CreateEntityCompletionData(IEntity entity)
        {
            CompletionDataType completionDataType;
            var ambience = new CSharpAmbience();
            ambience.ConversionFlags = entity is ITypeDefinition ? ConversionFlags.ShowTypeParameterList : ConversionFlags.None;

            switch (entity.SymbolKind)
            {
                default:
                case SymbolKind.None:
                    completionDataType = CompletionDataType.Unknown;
                    break;
                case SymbolKind.Property:
                case SymbolKind.Accessor:
                    completionDataType = entity.IsStatic ? CompletionDataType.StaticProperty : CompletionDataType.Property;
                    break;
                case SymbolKind.Constructor:
                case SymbolKind.Destructor:
                case SymbolKind.Method:
                case SymbolKind.Indexer:
                case SymbolKind.Operator:
                    completionDataType = entity.IsStatic ? CompletionDataType.StaticMethod : CompletionDataType.Method;
                    break;
                case SymbolKind.Namespace:
                    completionDataType = CompletionDataType.Namespace;
                    break;
                case SymbolKind.Parameter:
                case SymbolKind.Variable:
                case SymbolKind.Field:
                    completionDataType = entity.IsStatic ? CompletionDataType.StaticVariable : CompletionDataType.Variable;
                    break;
                case SymbolKind.TypeDefinition:
                case SymbolKind.TypeParameter:
                    completionDataType = entity.IsStatic ? CompletionDataType.StaticClass : CompletionDataType.Class;
                    break;
                case SymbolKind.Event:
                    completionDataType = CompletionDataType.Event;
                    break;
            }

            return new CompletionData(completionDataType, entity.Name, priority: 2, description: ambience.ConvertSymbol(entity))
            {
                CompletionText = ambience.ConvertSymbol(entity),
                DisplayText = entity.Name,
            };
        }

        public ICompletionData CreateEntityCompletionData(IEntity entity, string text)
        {
            throw new NotImplementedException();
        }

        public ICompletionData CreateEventCreationCompletionData(string delegateMethodName, IType delegateType, IEvent evt, string parameterDefinition, IUnresolvedMember currentMember, IUnresolvedTypeDefinition currentType)
        {
            throw new NotImplementedException();
        }

        public ICompletionData CreateFormatItemCompletionData(string format, string description, object example)
        {
            throw new NotImplementedException();
        }

        public ICompletionData CreateImportCompletionData(IType type, bool useFullName, bool addForTypeCreation)
        {
            throw new NotImplementedException();
        }

        public ICompletionData CreateLiteralCompletionData(string title, string description = null, string insertText = null)
        {
            return new CompletionData(CompletionDataType.Keyword, insertText ?? title, priority: 2, description: description ?? (insertText ?? title) + " Keyword");
        }

        public ICompletionData CreateMemberCompletionData(IType type, IEntity member)
        {
            throw new NotImplementedException();
        }

        public ICompletionData CreateNamespaceCompletionData(INamespace name)
        {
            return new CompletionData(CompletionDataType.Namespace, name.Name, description: "namespace " + name.Name);
        }

        public ICompletionData CreateNewOverrideCompletionData(int declarationBegin, IUnresolvedTypeDefinition type, IMember m)
        {
            throw new NotImplementedException();
        }

        public ICompletionData CreateNewPartialCompletionData(int declarationBegin, IUnresolvedTypeDefinition type, IUnresolvedMember m)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICompletionData> CreatePreProcessorDefinesCompletionData()
        {
            throw new NotImplementedException();
        }

        public ICompletionData CreateTypeCompletionData(IType type, bool showFullName, bool isInAttributeContext, bool addForTypeCreation)
        {
            var typeDef = type.GetDefinition();

            if (typeDef != null)
            {
                return CreateEntityCompletionData(typeDef);
            }
            else
            {
                string name = showFullName ? type.FullName : type.Name;

                return new CompletionData(CompletionDataType.Unknown, name);
            }
        }

        public ICompletionData CreateVariableCompletionData(ITypeParameter parameter)
        {
            return new CompletionData(CompletionDataType.Variable, parameter.Name, description: ambience.ConvertSymbol(parameter));
        }

        public ICompletionData CreateVariableCompletionData(IVariable variable)
        {
            return new CompletionData(CompletionDataType.Variable, variable.Name, description: ambience.ConvertVariable(variable));
        }

        public ICompletionData CreateXmlDocCompletionData(string tag, string description = null, string tagInsertionText = null)
        {
            throw new NotImplementedException();
        }
    }
}
