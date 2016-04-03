using CsScriptManaged.UI.CodeWindow;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CsScriptManaged.UI
{
    /// <summary>
    /// Helper class that represents UIElement to be created as result of executing interactive script.
    /// This class is needed since UI operations must be done in STA thread, so it will be lazily evaluated in UI thread.
    /// </summary>
    public class LazyUIResult
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
        private interface IResultTreeItem
        {
            string Name { get; }

            string Value { get; }

            string Type { get; }

            ImageSource Image { get; }

            IEnumerable<IResultTreeItem> Children { get; }
        }

        private class ResultTreeItem
        {
            public static IResultTreeItem Create(object obj, string name, ImageSource image)
            {
                Type type = obj.GetType();

                if (type.IsArray)
                    return new ArrayResultTreeItem((Array)obj, name, image);

                return new ObjectResultTreeItem(obj, name, image);
            }
        }

        private class ArrayResultTreeItem : IResultTreeItem
        {
            private Array array;

            public ArrayResultTreeItem(Array array, string name, ImageSource image)
            {
                this.array = array;
                Name = name;
                Image = image;
            }

            public IEnumerable<IResultTreeItem> Children
            {
                get
                {
                    yield return ResultTreeItem.Create(array.Length, "Length", CompletionData.GetImage(CompletionDataType.Property));
                    yield return ResultTreeItem.Create(array.LongLength, "LongLength", CompletionData.GetImage(CompletionDataType.Property));
                    for (int i = 0; i < array.Length; i++)
                        yield return ResultTreeItem.Create(array.GetValue(i), string.Format("[{0}]", i), CompletionData.GetImage(CompletionDataType.Variable));
                }
            }

            public ImageSource Image { get; private set; }

            public string Name { get; private set; }

            public string Type
            {
                get
                {
                    return array.GetType().FullName;
                }
            }

            public string Value
            {
                get
                {
                    return string.Format("{{ Length: {0} }}", array.Length);
                }
            }
        }

        private class ObjectResultTreeItem : IResultTreeItem
        {
            private object obj;

            public ObjectResultTreeItem(object obj, string name, ImageSource image)
            {
                this.obj = obj;
                Name = name;
                Image = image;
            }

            public IEnumerable<IResultTreeItem> Children
            {
                get
                {
                    Type type = obj.GetType();

                    if (!type.IsPrimitive)
                    {
                        // Non-static properties
                        var properties = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

                        foreach (var property in properties)
                            if (property.CanRead)
                                yield return ResultTreeItem.Create(property.GetValue(obj), property.Name, CompletionData.GetImage(CompletionDataType.Property));

                        // Static properties
                        var staticProperties = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                        foreach (var property in staticProperties)
                            if (property.CanRead)
                                yield return ResultTreeItem.Create(property.GetValue(obj), property.Name, CompletionData.GetImage(CompletionDataType.StaticProperty));

                        // Non-static fields
                        var fields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

                        foreach (var field in fields)
                            if (!field.IsStatic)
                                yield return ResultTreeItem.Create(field.GetValue(obj), field.Name, CompletionData.GetImage(CompletionDataType.Variable));

                        // Static fields
                        var staticFields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                        foreach (var field in staticFields)
                            if (field.IsStatic)
                                yield return ResultTreeItem.Create(field.GetValue(obj), field.Name, CompletionData.GetImage(CompletionDataType.StaticVariable));
                    }
                }
            }

            public ImageSource Image { get; private set; }

            public string Name { get; private set; }

            public string Type
            {
                get
                {
                    return obj.GetType().FullName;
                }
            }

            public string Value
            {
                get
                {
                    return obj.ToString();
                }
            }
        }

        public object Output(object obj)
        {
            // We don't visualize null
            if (obj == null)
                return null;

            // Primitive types and strings are visualized as ToString
            if (obj.GetType().IsPrimitive || obj is string)
            {
                return obj.ToString();
            }

            return new LazyUIResult(() => Visualize(obj));
        }

        private UIElement Visualize(object obj)
        {
            TreeView tree = new TreeView();
            IResultTreeItem resultTreeItem = ResultTreeItem.Create(obj, "result", null);

            tree.Items.Add(CreateTreeItem(resultTreeItem));
            return tree;
        }

        private TreeViewItem CreateTreeItem(IResultTreeItem resultTreeItem)
        {
            TreeViewItem item = new TreeViewItem();

            item.Header = string.Format("{0}\t{1}\t{2}", resultTreeItem.Name, resultTreeItem.Value, resultTreeItem.Type);
            item.Tag = resultTreeItem;
            item.Items.Add(0);
            item.Expanded += TreeViewItem_Expanded;
            return item;
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;

            if ((item.Items.Count == 1) && (item.Items[0] is int))
            {
                IResultTreeItem resultTreeItem = (IResultTreeItem)item.Tag;

                item.Items.Clear();
                foreach (var child in resultTreeItem.Children)
                    item.Items.Add(CreateTreeItem(child));
            }
        }
    }
}
