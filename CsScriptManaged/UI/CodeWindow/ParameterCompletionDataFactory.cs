using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsScriptManaged.UI.CodeWindow
{
    internal class ParameterCompletionDataFactory : IParameterCompletionDataFactory
    {
        public IParameterDataProvider CreateConstructorProvider(int startOffset, IType type)
        {
            return CreateMethodDataProvider(startOffset, type.GetConstructors());
        }

        public IParameterDataProvider CreateConstructorProvider(int startOffset, IType type, AstNode thisInitializer)
        {
            return CreateMethodDataProvider(startOffset, type.GetConstructors());
        }

        public IParameterDataProvider CreateDelegateDataProvider(int startOffset, IType type)
        {
            return CreateMethodDataProvider(startOffset, new[] { TypeSystemExtensions.GetDelegateInvokeMethod(type) });
        }

        public IParameterDataProvider CreateIndexerParameterDataProvider(int startOffset, IType type, IEnumerable<IProperty> accessibleIndexers, AstNode resolvedNode)
        {
            return CreateMethodDataProvider(startOffset, accessibleIndexers.Select(ai => ai.Getter));
        }

        public IParameterDataProvider CreateMethodDataProvider(int startOffset, IEnumerable<IMethod> methods)
        {
            return new ParameterDataProvider(startOffset, methods);
        }

        public IParameterDataProvider CreateTypeParameterDataProvider(int startOffset, IEnumerable<IMethod> methods)
        {
            return CreateMethodDataProvider(startOffset, methods);
        }

        public IParameterDataProvider CreateTypeParameterDataProvider(int startOffset, IEnumerable<IType> types)
        {
            throw new NotImplementedException();
        }
    }
}
