using CsDebugScript.Engine.Utility;
using CsDebugScript.UI.CodeWindow;
using Dynamitey;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CsDebugScript.UI
{
    /// <summary>
    /// Helper class that represents UIElement to be created as result of executing interactive script.
    /// This class is needed since UI operations must be done in STA thread, so it will be lazily evaluated in UI thread.
    /// </summary>
    internal class LazyUIResult
    {
        private Func<UIElement> creator;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyUIResult"/> class.
        /// </summary>
        /// <param name="creator">The lazy creator.</param>
        public LazyUIResult(Func<UIElement> creator)
        {
            this.creator = creator;
        }

        /// <summary>
        /// Gets the UI element.
        /// </summary>
        /// <remarks>
        /// This should be called from UI thread. Otherwise, it needs to be called from STA thread.
        /// </remarks>
        public UIElement UIElement
        {
            get
            {
                return creator();
            }
        }
    }

    internal class InteractiveResultVisualizer : IObjectWriter
    {
        private const int ArrayElementsVisualized = 100;

        private interface IResultTreeItem
        {
            string Name { get; }

            object Value { get; }

            string Type { get; }

            ImageSource Image { get; }

            bool IsExpandable { get; }

            IEnumerable<Tuple<string, IEnumerable<IResultTreeItem>>> Children { get; }

            string ValueString { get; }

            void Initialize();
        }

        private class ResultTreeItem
        {
            public static IResultTreeItem Create(object obj, Type objType, string name, ImageSource image, InteractiveResultVisualizer interactiveResultVisualizer)
            {
                if (obj != null)
                {
                    if (obj.GetType().IsArray)
                    {
                        return new ArrayResultTreeItem((Array)obj, objType, name, image, interactiveResultVisualizer);
                    }
                    else if (typeof(IDictionary).IsAssignableFrom(obj.GetType()))
                    {
                        return new DictionaryResultTreeItem((IDictionary)obj, objType, name, image, interactiveResultVisualizer);
                    }
                    else if (obj.GetType() == typeof(Variable))
                    {
                        return new VariableResultTreeItem(((Variable)obj).DowncastInterface(), objType, name, image, interactiveResultVisualizer);
                    }
                    else if (typeof(Variable).IsAssignableFrom(obj.GetType()))
                    {
                        // TODO: This is custom user type and we should visualize it differently
                        return new VariableResultTreeItem((Variable)obj, objType, name, image, interactiveResultVisualizer);
                    }
                    else if (obj.GetType() == typeof(VariableCollection))
                    {
                        return new VariableCollectionResultTreeItem((VariableCollection)obj, objType, name, image, interactiveResultVisualizer);
                    }
                }
                return new ObjectResultTreeItem(obj, objType, name, image, interactiveResultVisualizer);
            }
        }

        private class ArrayResultTreeItem : CustomObjectResultTreeItem
        {
            private Array array;

            public ArrayResultTreeItem(Array array, Type objType, string name, ImageSource image, InteractiveResultVisualizer interactiveResultVisualizer)
                : base(array, objType, name, image, interactiveResultVisualizer)
            {
                this.array = array;
            }

            public override IEnumerable<IResultTreeItem> ExpandedChildren
            {
                get
                {
                    yield return ResultTreeItem.Create(array.Length, null, "Length", CompletionData.GetImage(CompletionDataType.Property), interactiveResultVisualizer);
                    if (array.Length <= ArrayElementsVisualized)
                    {
                        foreach (IResultTreeItem element in GetElements(0, array.Length))
                        {
                            yield return element;
                        }
                    }
                }
            }

            public override IEnumerable<Tuple<string, IEnumerable<IResultTreeItem>>> Children
            {
                get
                {
                    bool elementsReturned = array.Length <= ArrayElementsVisualized;

                    foreach (Tuple<string, IEnumerable<IResultTreeItem>> children in base.Children)
                    {
                        yield return children;
                        if (!elementsReturned && children.Item1 == "[Expanded]")
                        {
                            elementsReturned = true;
                            for (int j = 0; j < array.Length; j += ArrayElementsVisualized)
                            {
                                int end = Math.Min(j + ArrayElementsVisualized, array.Length) - 1;

                                yield return Tuple.Create($"[{j}-{end}]", GetElements(j, end));
                            }
                        }
                    }
                }
            }

            public override object Value
            {
                get
                {
                    return $"{{ Length: {array.Length} }}";
                }
            }

            private IEnumerable<IResultTreeItem> GetElements(int start, int end)
            {
                for (int i = start; i < end; i++)
                {
                    yield return ResultTreeItem.Create(GetValue(() => array.GetValue(i)), objType.GetElementType(), $"[{i}]", CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                }
            }
        }

        private class DictionaryResultTreeItem : CustomObjectResultTreeItem
        {
            private IDictionary dictionary;

            public DictionaryResultTreeItem(IDictionary dictionary, Type objType, string name, ImageSource image, InteractiveResultVisualizer interactiveResultVisualizer)
                : base(dictionary, objType, name, image, interactiveResultVisualizer)
            {
                this.dictionary = dictionary;
            }

            public override IEnumerable<IResultTreeItem> ExpandedChildren
            {
                get
                {
                    yield return ResultTreeItem.Create(dictionary.Count, null, "Count", CompletionData.GetImage(CompletionDataType.Property), interactiveResultVisualizer);
                    foreach (IResultTreeItem item in OrderItems(ExtractItems()))
                    {
                        yield return item;
                    }
                }
            }

            public override object Value
            {
                get
                {
                    return $"{{ Elements: {dictionary.Count} }}";
                }
            }

            private IEnumerable<IResultTreeItem> ExtractItems()
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    yield return ResultTreeItem.Create(GetValue(() => entry.Value), objType.GetElementType(), entry.Key.ToString(), CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                }
            }
        }

        private class VariableCollectionResultTreeItem : CustomObjectResultTreeItem
        {
            private VariableCollection variableCollection;

            public VariableCollectionResultTreeItem(VariableCollection variableCollection, Type objType, string name, ImageSource image, InteractiveResultVisualizer interactiveResultVisualizer)
                : base(variableCollection, objType, name, image, interactiveResultVisualizer)
            {
                this.variableCollection = variableCollection;
            }

            public override IEnumerable<IResultTreeItem> ExpandedChildren
            {
                get
                {
                    yield return ResultTreeItem.Create(variableCollection.Count, null, "Count", CompletionData.GetImage(CompletionDataType.Property), interactiveResultVisualizer);
                    for (int i = 0; i < variableCollection.Count; i++)
                    {
                        yield return ResultTreeItem.Create(GetValue(() => variableCollection[i]), typeof(Variable), variableCollection[i].GetName(), CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                    }
                }
            }

            public override object Value
            {
                get
                {
                    return $"{{ Variables: {variableCollection.Count} }}";
                }
            }
        }

        private class VariableResultTreeItem : CustomObjectResultTreeItem
        {
            private Variable variable;
            private bool extractUsingClasses;

            public VariableResultTreeItem(Variable variable, Type objType, string name, ImageSource image, InteractiveResultVisualizer interactiveResultVisualizer)
                : base(variable, objType, name, image, interactiveResultVisualizer)
            {
                this.variable = variable;
                try
                {
                    extractUsingClasses = variable.GetCodeType().ClassFieldNames != null;
                }
                catch
                {
                    extractUsingClasses = false;
                }
            }

            public override bool IsExpandable
            {
                get
                {
                    CodeType codeType = variable.GetCodeType();

                    return !codeType.IsSimple && !codeType.IsEnum && !variable.IsNullPointer();
                }
            }

            public override string Type
            {
                get
                {
                    return variable.GetCodeType().ToString();
                }
            }

            public override IEnumerable<IResultTreeItem> ExpandedChildren
            {
                get
                {
                    CodeType codeType = variable.GetCodeType();

                    if (codeType.IsPointer && (codeType.ElementType.IsSimple || codeType.ElementType.IsPointer || codeType.ElementType.IsArray || codeType.ElementType.IsEnum))
                    {
                        yield return ResultTreeItem.Create(GetValue(() => variable.DereferencePointer()), typeof(Variable), "*", CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                    }
                    else if (codeType.IsArray)
                    {
                        yield return ResultTreeItem.Create(variable.GetArrayLength(), typeof(int), "Length", CompletionData.GetImage(CompletionDataType.Property), interactiveResultVisualizer);
                        if (variable.GetArrayLength() <= ArrayElementsVisualized)
                        {
                            foreach (IResultTreeItem element in GetArrayElements(0, variable.GetArrayLength()))
                            {
                                yield return element;
                            }
                        }
                    }
                    // TODO: Continue specializing on variable types
                    else
                    {
                        foreach (IResultTreeItem item in OrderItems(ExtractFields()))
                        {
                            yield return item;
                        }
                        yield return ResultTreeItem.Create(codeType, codeType.GetType(), "CodeType", CompletionData.GetImage(CompletionDataType.Property), interactiveResultVisualizer);
                    }
                }
            }

            public override object Value
            {
                get
                {
                    CodeType codeType = variable.GetCodeType();

                    if (codeType.IsEnum)
                    {
                        return $"{variable} ({variable.Data:X}) [0x{variable.Data:X}]";
                    }
                    else if (codeType.IsArray)
                    {
                        return $"{{ Length: {variable.GetArrayLength()} }}";
                    }

                    return variable;
                }
            }

            public override IEnumerable<Tuple<string, IEnumerable<IResultTreeItem>>> Children
            {
                get
                {
                    bool elementsReturned = !variable.GetCodeType().IsArray || variable.GetArrayLength() <= ArrayElementsVisualized;

                    foreach (Tuple<string, IEnumerable<IResultTreeItem>> children in base.Children)
                    {
                        yield return children;
                        if (!elementsReturned && children.Item1 == "[Expanded]")
                        {
                            elementsReturned = true;
                            for (int j = 0; j < variable.GetArrayLength(); j += ArrayElementsVisualized)
                            {
                                int end = Math.Min(j + ArrayElementsVisualized, variable.GetArrayLength());

                                yield return Tuple.Create($"[{j}-{end - 1}]", GetArrayElements(j, end));
                            }
                        }
                    }
                }
            }

            private IEnumerable<IResultTreeItem> GetArrayElements(int start, int end)
            {
                for (int i = start; i < end; i++)
                {
                    yield return ResultTreeItem.Create(GetValue(() => variable.GetArrayElement(i)), typeof(Variable), $"[{i}]", CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                }
            }

            private IEnumerable<IResultTreeItem> ExtractFields()
            {
                CodeType codeType = variable.GetCodeType();

                if (extractUsingClasses)
                {
                    foreach (string baseClass in codeType.InheritedClasses.Keys)
                    {
                        object baseClassValue = GetValue(() => variable.GetBaseClass(baseClass));
                        Variable baseClassVariable = baseClassValue as Variable;

                        if (baseClassVariable != null)
                        {
                            yield return new VariableResultTreeItem(baseClassVariable, typeof(Variable), $"[{baseClass}]", CompletionData.GetImage(CompletionDataType.Class), interactiveResultVisualizer);
                        }
                        else
                        {
                            yield return ResultTreeItem.Create(baseClassValue, typeof(Variable), $"[{baseClass}]", CompletionData.GetImage(CompletionDataType.Class), interactiveResultVisualizer);
                        }
                    }

                    foreach (string fieldName in codeType.ClassFieldNames)
                    {
                        yield return ResultTreeItem.Create(GetValue(() => variable.GetClassField(fieldName)), typeof(Variable), fieldName, CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                    }
                }
                else
                {
                    foreach (string fieldName in codeType.FieldNames)
                    {
                        yield return ResultTreeItem.Create(GetValue(() => variable.GetField(fieldName)), typeof(Variable), fieldName, CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                    }
                }
            }
        }

        private class CustomObjectResultTreeItem : ObjectResultTreeItem
        {
            public CustomObjectResultTreeItem(object obj, Type objType, string name, ImageSource image, InteractiveResultVisualizer interactiveResultVisualizer)
                : base(obj, objType, name, image, interactiveResultVisualizer)
            {
            }

            public override bool IsExpandable
            {
                get
                {
                    return true;
                }
            }

            public override IEnumerable<Tuple<string, IEnumerable<IResultTreeItem>>> Children
            {
                get
                {
                    bool publicsReturned = false;

                    foreach (Tuple<string, IEnumerable<IResultTreeItem>> children in base.Children)
                    {
                        yield return children;
                        if (!publicsReturned && children.Item1 == "[Expanded]")
                        {
                            publicsReturned = true;
                            yield return Tuple.Create("[Public]", base.ExpandedChildren);
                        }
                    }
                }
            }
        }

        private class ObjectResultTreeItem : IResultTreeItem
        {
            private object obj;
            private SimpleCache<string> valueString;
            protected Type objType;
            protected InteractiveResultVisualizer interactiveResultVisualizer;

            public ObjectResultTreeItem(object obj, Type objType, string name, ImageSource image, InteractiveResultVisualizer interactiveResultVisualizer)
            {
                this.obj = obj;
                this.objType = objType;
                this.interactiveResultVisualizer = interactiveResultVisualizer;
                Name = name;
                Image = image;
                valueString = SimpleCache.Create(() =>
                {
                    try
                    {
                        return Value.ToString();
                    }
                    catch
                    {
                        return "<Exception during evaluation>";
                    }
                });
            }

            public virtual bool IsExpandable
            {
                get
                {
                    if (obj != null && objType != null)
                    {
                        Type type = obj.GetType();

                        return !type.IsEnum && !type.IsPrimitive;
                    }

                    return false;
                }
            }

            public virtual IEnumerable<Tuple<string, IEnumerable<IResultTreeItem>>> Children
            {
                get
                {
                    if (ExpandedChildren.Any())
                    {
                        yield return Tuple.Create("[Expanded]", ExpandedChildren);
                    }
                    if (NonPublicChildren.Any())
                    {
                        yield return Tuple.Create("[Internal]", OrderItems(NonPublicChildren));
                    }
                    if (StaticChildren.Any())
                    {
                        yield return Tuple.Create("[Static]", OrderItems(StaticChildren));
                    }
                    if (DynamicChildren.Any())
                    {
                        yield return Tuple.Create("[Dynamic]", OrderItems(DynamicChildren));
                    }
                    if (EnumerationChildren.Any())
                    {
                        yield return Tuple.Create("[Enumeration]", OrderItems(EnumerationChildren));
                    }
                }
            }

            public virtual IEnumerable<IResultTreeItem> ExpandedChildren
            {
                get
                {
                    return OrderItems(PublicChildren);
                }
            }

            public virtual IEnumerable<IResultTreeItem> PublicChildren
            {
                get
                {
                    if (obj != null)
                    {
                        Type type = obj.GetType();

                        if (!type.IsPrimitive && !type.IsEnum)
                        {
                            // Non-static properties
                            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                            foreach (var property in properties)
                            {
                                if (property.CanRead && property.GetIndexParameters().Length == 0)
                                {
                                    yield return ResultTreeItem.Create(GetValue(() => property.GetValue(obj)), property.PropertyType, property.Name, CompletionData.GetImage(CompletionDataType.Property), interactiveResultVisualizer);
                                }
                            }

                            // Non-static fields
                            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                            foreach (var field in fields)
                            {
                                if (!field.IsStatic && !field.Name.EndsWith(">k__BackingField"))
                                {
                                    yield return ResultTreeItem.Create(GetValue(() => field.GetValue(obj)), field.FieldType, field.Name, CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                                }
                            }
                        }
                    }
                }
            }

            public virtual IEnumerable<IResultTreeItem> NonPublicChildren
            {
                get
                {
                    if (obj != null)
                    {
                        Type type = obj.GetType();

                        if (!type.IsPrimitive && !type.IsEnum)
                        {
                            // Non-static properties
                            var properties = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                            foreach (var property in properties)
                            {
                                if (property.CanRead && property.GetIndexParameters().Length == 0)
                                {
                                    yield return ResultTreeItem.Create(GetValue(() => property.GetValue(obj)), property.PropertyType, property.Name, CompletionData.GetImage(CompletionDataType.Property), interactiveResultVisualizer);
                                }
                            }

                            // Non-static fields
                            var fields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                            foreach (var field in fields)
                            {
                                if (!field.IsStatic && !field.Name.EndsWith(">k__BackingField"))
                                {
                                    yield return ResultTreeItem.Create(GetValue(() => field.GetValue(obj)), field.FieldType, field.Name, CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                                }
                            }
                        }
                    }
                }
            }

            public virtual IEnumerable<IResultTreeItem> StaticChildren
            {
                get
                {
                    if (obj != null)
                    {
                        Type type = obj.GetType();

                        if (!type.IsPrimitive && !type.IsEnum)
                        {
                            // Static properties
                            var staticProperties = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                            foreach (var property in staticProperties)
                            {
                                if (property.CanRead && property.GetIndexParameters().Length == 0)
                                {
                                    yield return ResultTreeItem.Create(GetValue(() => property.GetValue(obj)), property.PropertyType, property.Name, CompletionData.GetImage(CompletionDataType.StaticProperty), interactiveResultVisualizer);
                                }
                            }

                            // Static fields
                            var staticFields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                            foreach (var field in staticFields)
                            {
                                if (field.IsStatic && !field.Name.EndsWith(">k__BackingField"))
                                {
                                    yield return ResultTreeItem.Create(GetValue(() => field.GetValue(obj)), field.FieldType, field.Name, CompletionData.GetImage(CompletionDataType.StaticVariable), interactiveResultVisualizer);
                                }
                            }
                        }
                    }
                }
            }

            public virtual IEnumerable<IResultTreeItem> DynamicChildren
            {
                get
                {
                    if (obj != null)
                    {
                        if (typeof(DynamicObject).IsAssignableFrom(obj.GetType()))
                        {
                            DynamicObject dynamicObject = (DynamicObject)obj;

                            foreach (string memberName in dynamicObject.GetDynamicMemberNames())
                            {
                                yield return ResultTreeItem.Create(GetValue(() => Dynamic.InvokeGet(dynamicObject, memberName)), typeof(DynamicObject), memberName, CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                            }
                        }
                    }
                }
            }

            public virtual IEnumerable<IResultTreeItem> EnumerationChildren
            {
                get
                {
                    if (obj != null)
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(obj.GetType()))
                        {
                            IEnumerable enumeration = (IEnumerable)obj;
                            int index = 0;

                            foreach (object value in enumeration)
                            {
                                yield return ResultTreeItem.Create(value, value?.GetType(), $"[{index++}]", CompletionData.GetImage(CompletionDataType.Variable), interactiveResultVisualizer);
                            }
                        }
                    }
                }
            }

            private static ImageSource ExceptionImage = CompletionData.CreateTextImage("", Brushes.Red);

            protected object GetValue(Func<object> getValueFunction)
            {
                try
                {
                    return getValueFunction();
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }

            public void Initialize()
            {
                try
                {
                    string name = Name;
                }
                catch
                {
                }

                if (!(Value is UIElement))
                {
                    try
                    {
                        string value = ValueString;
                    }
                    catch
                    {
                    }
                }

                try
                {
                    string type = Type;
                }
                catch
                {
                }
            }

            public virtual ImageSource Image { get; private set; }

            public virtual string Name { get; private set; }

            public virtual string Type
            {
                get
                {
                    return objType != null ? InteractiveExecution.GetCodeName(obj?.GetType() ?? objType) : "";
                }
            }

            public virtual object Value
            {
                get
                {
                    if (obj != null && obj.GetType() == typeof(ulong))
                    {
                        return $"0x{obj:X} ({obj})";
                    }
                    else if (obj != null && typeof(Exception).IsAssignableFrom(obj.GetType()))
                    {
                        return interactiveResultVisualizer.CreateTextWithIcon("Exception", ExceptionImage, obj.ToString(), bold: true);
                    }

                    return obj != null ? obj : "null";
                }
            }

            public string ValueString
            {
                get
                {
                    return valueString.Value;
                }
            }
        }

        internal const string ExpandingItemText = "Loading...";

        private InteractiveWindowContent interactiveWindowContent;

        public InteractiveResultVisualizer(InteractiveWindowContent interactiveWindowContent)
        {
            this.interactiveWindowContent = interactiveWindowContent;
        }


        public object Output(object obj)
        {
            // We don't visualize null
            if (obj == null)
            {
                return null;
            }

            // Primitive types and strings are visualized as ToString
            if (obj.GetType().IsPrimitive || obj is string)
            {
                return obj.ToString();
            }

            // UI elements should be resurfaced back.
            if (obj is UIElement)
            {
                return obj;
            }

            // All other should be visualized in a table
            IResultTreeItem resultTreeItem = ResultTreeItem.Create(obj, obj.GetType(), "result", null, this);

            resultTreeItem.Initialize();
            return new LazyUIResult(() => Visualize(resultTreeItem));
        }

        TreeViewItem emptyListItem;
        System.Windows.Threading.Dispatcher dispatcher;

        private UIElement Visualize(IResultTreeItem resultTreeItem)
        {
            // Create top level table grid
            Grid tableGrid = new Grid();
            dispatcher = tableGrid.Dispatcher;

            Grid.SetIsSharedSizeScope(tableGrid, true);
            tableGrid.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(1, GridUnitType.Auto),
            });

            // Create table header
            TreeViewItem header = new TreeViewItem();
            Grid headerGrid = CreateTreeItemGrid(0);
            TextBlock name = new TextBlock();
            name.Text = "Name";
            name.FontWeight = FontWeights.Bold;
            Grid.SetColumn(name, NameColumnIndex);
            headerGrid.Children.Add(name);
            TextBlock value = new TextBlock();
            value.Text = "Value";
            value.FontWeight = FontWeights.Bold;
            Grid.SetColumn(value, ValueColumnIndex);
            headerGrid.Children.Add(value);
            TextBlock type = new TextBlock();
            type.Text = "Type";
            type.FontWeight = FontWeights.Bold;
            Grid.SetColumn(type, TypeColumnIndex);
            headerGrid.Children.Add(type);
            emptyListItem = new TreeViewItem();
            emptyListItem.Padding = new Thickness(0);
            emptyListItem.Focusable = false;
            Grid.SetColumn(emptyListItem, 1);
            emptyListItem.Focusable = false;
            headerGrid.Children.Add(emptyListItem);
            header.Focusable = false;
            header.Header = headerGrid;

            // Create table tree
            TreeView tree = new TreeView();
            TreeViewItem resultItem = CreateTreeItem(resultTreeItem, 0);

            tree.PreviewMouseWheel += Tree_PreviewMouseWheel;
            tree.PreviewKeyDown += Tree_PreviewKeyDown;
            tree.LostFocus += (a, b) =>
            {
                var item = tree.SelectedItem as TreeViewItem;

                if (item != null)
                {
                    item.IsSelected = false;
                }
            };
            tree.GotFocus += (a, b) =>
            {
                var item = tree.SelectedItem as TreeViewItem;

                if (item == null)
                {
                    if (interactiveWindowContent.TraverseDirection.HasValue)
                    {
                        if (interactiveWindowContent.TraverseDirection == FocusNavigationDirection.Next)
                        {
                            item = tree.Items[1] as TreeViewItem;
                        }
                        else
                        {
                            item = tree.Items[tree.Items.Count - 1] as TreeViewItem;
                            while (item.IsExpanded)
                            {
                                item = item.Items[item.Items.Count - 1] as TreeViewItem;
                            }
                        }
                    }
                    else
                    {
                        item = tree.Items[1] as TreeViewItem;
                    }

                    if (item != null)
                    {
                        item.IsSelected = true;
                    }
                }
            };
            tree.Items.Add(header);
            tree.Items.Add(resultItem);
            resultItem.IsExpanded = true;
            tableGrid.Children.Add(tree);
            return tableGrid;
        }

        private void Tree_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Since TreeView is going to eat our event, we need to re-raise it again yo regain scrolling ability.
            if (sender is TreeView && !e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void Tree_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                TreeView tree = (TreeView)sender;
                TreeViewItem item = e.OriginalSource as TreeViewItem;

                // if item is expanded and has items, then this item is not the last one :)
                if (item.HasItems && item.IsExpanded)
                    return;

                // Check inside the parent
                TreeViewItem parent = item.Parent as TreeViewItem;

                while (parent != null)
                {
                    if (parent.Items.IndexOf(item) == parent.Items.Count - 1)
                    {
                        item = parent;
                        parent = parent.Parent as TreeViewItem;
                    }
                    else
                    {
                        // We are not at the last item
                        return;
                    }
                }

                e.Handled = true;
                interactiveWindowContent.TraverseNext(tree);
            }
            else if (e.Key == Key.Up && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                TreeView tree = (TreeView)sender;
                TreeViewItem item = e.OriginalSource as TreeViewItem;

                // Check inside the parent
                TreeViewItem parent = item.Parent as TreeViewItem;

                if (parent != null || tree.Items.IndexOf(item) > 1) // 1 is because of the header
                {
                    // We are not at the first item
                    return;
                }

                e.Handled = true;
                interactiveWindowContent.TraversePrevious(tree);
            }
        }

        const int NameColumnIndex = 0;
        const int SpacingColumns = 10;
        const int ValueColumnIndex = SpacingColumns + 1;
        const int TypeColumnIndex = SpacingColumns + 2;

        private static Grid CreateTreeItemGrid(int level)
        {
            Grid grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Name = "Name",
                Width = new GridLength(0, GridUnitType.Auto),
                MinWidth = 100,
                SharedSizeGroup = "Name",
            });
            for (int i = 0; i < level; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = level; i < SpacingColumns; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Name = "Spacing",
                    Width = new GridLength(0, GridUnitType.Auto),
                    SharedSizeGroup = "Spacing",
                });
            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Name = "Value",
                Width = new GridLength(0, GridUnitType.Auto),
                MinWidth = 100,
                SharedSizeGroup = "Value",
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Name = "Type",
                Width = new GridLength(0, GridUnitType.Auto),
                MinWidth = 100,
                SharedSizeGroup = "Type",
            });
            return grid;
        }

        private class TreeViewItemTag
        {
            public object ResultTreeItem { get; set; }

            public int Level { get; set; }
        }

        private UIElement CreateTextWithIcon(string text, ImageSource icon, object tooltip = null, bool italic = false, bool bold = false)
        {
            return dispatcher.Invoke(() =>
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                Grid.SetColumn(stackPanel, NameColumnIndex);
                TextBlock textBlock = new TextBlock();
                if (italic)
                {
                    textBlock.FontStyle = FontStyles.Italic;
                }
                if (bold)
                {
                    textBlock.FontWeight = FontWeights.Bold;
                }
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.Text = text;
                Image image = new Image();
                image.Width = image.Height = 16;
                image.Source = icon;
                image.ToolTip = tooltip;
                stackPanel.Children.Add(image);
                stackPanel.Children.Add(textBlock);
                return stackPanel;
            });
        }

        private TreeViewItem CreateSimpleTreeItem(string name, ImageSource image, int level, bool italic)
        {
            Grid grid;

            return CreateSimpleTreeItem(name, image, level, out grid, italic: italic);
        }

        private TreeViewItem CreateSimpleTreeItem(string name, ImageSource image, int level, out Grid grid, bool italic = false)
        {
            TreeViewItem item = new TreeViewItem();

            grid = CreateTreeItemGrid(level);
            grid.Children.Add(CreateTextWithIcon(name, image, italic: italic));
            item.Header = grid;
            return item;
        }

        private TreeViewItem CreateTreeItem(IResultTreeItem resultTreeItem, int level)
        {
            Grid grid;
            TreeViewItem item = CreateSimpleTreeItem(resultTreeItem.Name, resultTreeItem.Image, level, out grid);
            object itemValue = resultTreeItem.Value;
            if (itemValue is UIElement)
            {
                Grid.SetColumn((UIElement)itemValue, ValueColumnIndex);
                grid.Children.Add((UIElement)itemValue);
            }
            else
            {
                TextBlock value = new TextBlock();
                value.Text = resultTreeItem.ValueString;
                Grid.SetColumn(value, ValueColumnIndex);
                grid.Children.Add(value);
            }

            TextBlock type = new TextBlock();
            type.Text = resultTreeItem.Type;
            Grid.SetColumn(type, TypeColumnIndex);
            grid.Children.Add(type);
            item.Tag = new TreeViewItemTag()
            {
                ResultTreeItem = resultTreeItem,
                Level = level,
            };
            if (resultTreeItem.IsExpandable)
            {
                item.Items.Add(ExpandingItemText);
                item.Expanded += TreeViewItem_Expanded;
            }
            return item;
        }

        private static IEnumerable<IResultTreeItem> OrderItems(IEnumerable<IResultTreeItem> items)
        {
            return items
                .OrderBy(s =>
                {
                    if (s.Name.StartsWith("[") && s.Name.EndsWith("]"))
                    {
                        int value;

                        if (int.TryParse(s.Name.Substring(1, s.Name.Length - 2), out value))
                        {
                            return value;
                        }
                    }
                    return int.MaxValue;
                })
                .ThenBy(s => s.Name);
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                TreeViewItem item = e.Source as TreeViewItem;

                if ((item.Items.Count == 1) && (item.Items[0].ToString() == ExpandingItemText))
                {
                    TreeViewItemTag tag = item.Tag as TreeViewItemTag;

                    System.Threading.Tasks.Task.Run(() =>
                    {
                        IResultTreeItem resultTreeItem = tag.ResultTreeItem as IResultTreeItem;
                        IEnumerable<IResultTreeItem> children = tag.ResultTreeItem as IEnumerable<IResultTreeItem>;

                        try
                        {
                            if (resultTreeItem != null)
                            {
                                List<Tuple<string, IEnumerable<IResultTreeItem>>> customChildren = new List<Tuple<string, IEnumerable<IResultTreeItem>>>();

                                foreach (Tuple<string, IEnumerable<IResultTreeItem>> customChild in resultTreeItem.Children)
                                {
                                    if (customChild.Item2.Any())
                                    {
                                        if (customChild.Item1 == "[Expanded]")
                                        {
                                            List<IResultTreeItem> cachedItems = customChild.Item2.ToList();

                                            customChildren.Add(Tuple.Create(customChild.Item1, (IEnumerable<IResultTreeItem>)cachedItems));
                                            foreach (IResultTreeItem child in cachedItems)
                                            {
                                                child.Initialize();
                                            }
                                        }
                                        else
                                        {
                                            customChildren.Add(customChild);
                                        }
                                    }
                                }

                                item.Dispatcher.InvokeAsync(() =>
                                {
                                    try
                                    {
                                        int level = tag.Level;
                                        TreeViewItem lastItem = null;

                                        item.Items.Clear();
                                        foreach (Tuple<string, IEnumerable<IResultTreeItem>> customChild in customChildren)
                                        {
                                            if (customChild.Item1 == "[Expanded]")
                                            {
                                                foreach (IResultTreeItem child in customChild.Item2)
                                                {
                                                    item.Items.Add(lastItem = CreateTreeItem(child, level + 1));
                                                }
                                            }
                                            else
                                            {
                                                TreeViewItem customItem = CreateSimpleTreeItem(customChild.Item1, CompletionData.GetImage(CompletionDataType.Namespace), level + 1, italic: true);

                                                customItem.Tag = new TreeViewItemTag()
                                                {
                                                    Level = level + 1,
                                                    ResultTreeItem = customChild.Item2,
                                                };
                                                customItem.Items.Add(ExpandingItemText);
                                                customItem.Expanded += TreeViewItem_Expanded;
                                                item.Items.Add(lastItem = customItem);
                                            }
                                        }

                                        // Check if we need to fix empty list item width
                                        if (lastItem != null && double.IsNaN(emptyListItem.Width))
                                        {
                                            item.Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                emptyListItem.Width = item.ActualWidth - lastItem.ActualWidth;
                                            }), System.Windows.Threading.DispatcherPriority.Background);
                                        }
                                    }
                                    catch (Exception ex3)
                                    {
                                        MessageBox.Show(ex3.ToString());
                                    }
                                });
                            }
                            else if (children != null)
                            {
                                List<IResultTreeItem> cachedItems = children.ToList();

                                foreach (IResultTreeItem child in children)
                                {
                                    child.Initialize();
                                }

                                item.Dispatcher.InvokeAsync(() =>
                                {
                                    try
                                    {
                                        int level = tag.Level;
                                        TreeViewItem lastItem = null;

                                        item.Items.Clear();
                                        foreach (IResultTreeItem child in cachedItems)
                                        {
                                            item.Items.Add(lastItem = CreateTreeItem(child, level + 1));
                                        }

                                        // Check if we need to fix empty list item width
                                        if (lastItem != null && double.IsNaN(emptyListItem.Width))
                                        {
                                            item.Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                emptyListItem.Width = item.ActualWidth - lastItem.ActualWidth;
                                            }), System.Windows.Threading.DispatcherPriority.Background);
                                        }
                                    }
                                    catch (Exception ex3)
                                    {
                                        MessageBox.Show(ex3.ToString());
                                    }
                                });
                            }
                        }
                        catch (Exception ex2)
                        {
                            MessageBox.Show(ex2.ToString());
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
