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
            EntityWrapper<IEntity> entityWrapper = new EntityWrapper<IEntity>(entity);

            return new CompletionData(entityWrapper.CompletionDataType, entity.Name, priority: 2, description: entityWrapper.EntityDescription)
            {
                CompletionText = entityWrapper.AmbienceDescription,
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
            return new CompletionData(CompletionDataType.Keyword, insertText ?? title, priority: 2, description: EntityWrapper<IEntity>.CreateEntityDescription(title, "<summary>" + (description ?? (insertText ?? title) + " Keyword") + "</summary>"));
        }

        public ICompletionData CreateMemberCompletionData(IType type, IEntity member)
        {
            throw new NotImplementedException();
        }

        public ICompletionData CreateNamespaceCompletionData(INamespace name)
        {
            return new CompletionData(CompletionDataType.Namespace, name.Name, description: EntityWrapper<IEntity>.CreateEntityDescription("namespace " + name.Name));
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
            return new CompletionData(CompletionDataType.Variable, parameter.Name, description: EntityWrapper<IEntity>.CreateEntityDescription(parameter));
        }

        public ICompletionData CreateVariableCompletionData(IVariable variable)
        {
            return new CompletionData(CompletionDataType.Variable, variable.Name, description: EntityWrapper<IEntity>.CreateEntityDescription(variable));
        }

        public ICompletionData CreateXmlDocCompletionData(string tag, string description = null, string tagInsertionText = null)
        {
            throw new NotImplementedException();
        }
    }
}
