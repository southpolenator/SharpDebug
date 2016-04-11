using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CsScriptManaged.UI.CodeWindow
{
    internal class ParameterDataProvider : ICSharpCode.NRefactory.Completion.IParameterDataProvider, ICSharpCode.AvalonEdit.CodeCompletion.IOverloadProvider
    {
        /// <summary>
        /// The currently selected index
        /// </summary>
        private int selectedIndex;

        /// <summary>
        /// The currently selected parameter
        /// </summary>
        private int currentParameter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDataProvider"/> class.
        /// </summary>
        /// <param name="startOffset">The start offset.</param>
        /// <param name="methods">The methods.</param>
        public ParameterDataProvider(int startOffset, IEnumerable<ICSharpCode.NRefactory.TypeSystem.IMethod> methods)
        {
            StartOffset = startOffset;
            Methods = methods.Select(m => new EntityWrapper<ICSharpCode.NRefactory.TypeSystem.IMethod>(m)).OrderBy(m => m.Entity.Parameters.Count).ThenBy(m => m.AmbienceDescription).ToArray();
        }

        /// <summary>
        /// Gets the methods.
        /// </summary>
        public EntityWrapper<ICSharpCode.NRefactory.TypeSystem.IMethod>[] Methods { get; private set; }

        /// <summary>
        /// Gets the currently selected method.
        /// </summary>
        private EntityWrapper<ICSharpCode.NRefactory.TypeSystem.IMethod> CurrentMethod
        {
            get
            {
                return Methods[SelectedIndex];
            }
        }

        /// <summary>
        /// Gets the overload count.
        /// </summary>
        public int Count
        {
            get
            {
                return Methods.Length;
            }
        }

        /// <summary>
        /// Gets the start offset of the parameter expression node.
        /// </summary>
        public int StartOffset { get; private set; }

        /// <summary>
        /// Gets the current content.
        /// </summary>
        public object CurrentContent
        {
            get
            {
                return CurrentMethod.GetDocumentationWithParameter(currentParameter);
            }
        }

        /// <summary>
        /// Gets the current header.
        /// </summary>
        public object CurrentHeader
        {
            get
            {
                return CurrentMethod.GetSyntaxWithParameterHighlighted(currentParameter);
            }
        }

        /// <summary>
        /// Gets the text 'SelectedIndex of Count'.
        /// </summary>
        public string CurrentIndexText
        {
            get
            {
                return string.Format("{0} of {1}", SelectedIndex + 1, Count);
            }
        }

        /// <summary>
        /// Gets/Sets the selected index.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                selectedIndex = value;
                while (selectedIndex < 0)
                    selectedIndex += Count;
                while (selectedIndex >= Count)
                    selectedIndex -= Count;
                CallPropertyChanged(nameof(SelectedIndex));
                CallPropertyChanged(nameof(CurrentIndexText));
                CallPropertyChanged(nameof(CurrentHeader));
                CallPropertyChanged(nameof(CurrentContent));
            }
        }

        public int CurrentParameter
        {
            get
            {
                return currentParameter;
            }

            set
            {
                currentParameter = value;
                CallPropertyChanged(nameof(SelectedIndex));
                CallPropertyChanged(nameof(CurrentIndexText));
                CallPropertyChanged(nameof(CurrentHeader));
                CallPropertyChanged(nameof(CurrentContent));
                if (currentParameter >= CurrentMethod.Entity.Parameters.Count)
                    for (int i = selectedIndex; i < Count; i++)
                        if (currentParameter < Methods[i].Entity.Parameters.Count)
                        {
                            SelectedIndex = i;
                            break;
                        }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool AllowParameterList(int overload)
        {
            throw new NotImplementedException();
        }

        public string GetDescription(int overload, int currentParameter)
        {
            throw new NotImplementedException();
        }

        public string GetHeading(int overload, string[] parameterDescription, int currentParameter)
        {
            throw new NotImplementedException();
        }

        public int GetParameterCount(int overload)
        {
            throw new NotImplementedException();
        }

        public string GetParameterDescription(int overload, int paramIndex)
        {
            throw new NotImplementedException();
        }

        public string GetParameterName(int overload, int currentParameter)
        {
            throw new NotImplementedException();
        }

        private void CallPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
