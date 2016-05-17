using CsDebugScript.UI.CodeWindow;
using System;
using System.Collections.Generic;
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
        private interface IResultTreeItem
        {
            string Name { get; }

            object Value { get; }

            string Type { get; }

            ImageSource Image { get; }

            IEnumerable<IResultTreeItem> Children { get; }
        }

        private class ResultTreeItem
        {
            public static IResultTreeItem Create(object obj, Type objType, string name, ImageSource image)
            {
                if (obj != null && objType.IsArray)
                    return new ArrayResultTreeItem((Array)obj, objType, name, image);
                return new ObjectResultTreeItem(obj, objType, name, image);
            }
        }

        private class ArrayResultTreeItem : ObjectResultTreeItem
        {
            private Array array;

            public ArrayResultTreeItem(Array array, Type objType, string name, ImageSource image)
                : base(array, objType, name, image)
            {
                this.array = array;
            }

            public override IEnumerable<IResultTreeItem> Children
            {
                get
                {
                    foreach (var child in base.Children)
                        yield return child;
                    for (int i = 0; i < array.Length; i++)
                        yield return ResultTreeItem.Create(GetValue(() => array.GetValue(i)), objType.GetElementType(), string.Format("[{0}]", i), CompletionData.GetImage(CompletionDataType.Variable));
                }
            }

            public override object Value
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
            protected Type objType;

            public ObjectResultTreeItem(object obj, Type objType, string name, ImageSource image)
            {
                this.obj = obj;
                this.objType = objType;
                Name = name;
                Image = image;
            }

            public virtual IEnumerable<IResultTreeItem> Children
            {
                get
                {
                    if (obj != null)
                    {
                        Type type = obj.GetType();

                        if (!type.IsPrimitive && !type.IsEnum)
                        {
                            // Non-static properties
                            var properties = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                            foreach (var property in properties)
                                if (property.CanRead)
                                    yield return ResultTreeItem.Create(GetValue(() => property.GetValue(obj)), property.PropertyType, property.Name, CompletionData.GetImage(CompletionDataType.Property));

                            // Static properties
                            var staticProperties = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                            foreach (var property in staticProperties)
                                if (property.CanRead)
                                    yield return ResultTreeItem.Create(GetValue(() => property.GetValue(obj)), property.PropertyType, property.Name, CompletionData.GetImage(CompletionDataType.StaticProperty));

                            // Non-static fields
                            var fields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                            foreach (var field in fields)
                                if (!field.IsStatic)
                                    yield return ResultTreeItem.Create(GetValue(() => field.GetValue(obj)), field.FieldType, field.Name, CompletionData.GetImage(CompletionDataType.Variable));

                            // Static fields
                            var staticFields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                            foreach (var field in staticFields)
                                if (field.IsStatic)
                                    yield return ResultTreeItem.Create(GetValue(() => field.GetValue(obj)), field.FieldType, field.Name, CompletionData.GetImage(CompletionDataType.StaticVariable));
                        }
                    }
                }
            }

            private static ImageSource ExceptionImage = CompletionData.CreateTextImage("", Brushes.Red);

            protected static object GetValue(Func<object> getValueFunction)
            {
                try
                {
                    return getValueFunction();
                }
                catch (Exception ex)
                {
                    return CreateTextWithIcon("Exception", ExceptionImage, ex.ToString());
                }
            }

            public virtual ImageSource Image { get; private set; }

            public virtual string Name { get; private set; }

            public virtual string Type
            {
                get
                {
                    return InteractiveExecution.GetCodeName(objType);
                }
            }

            public virtual object Value
            {
                get
                {
                    return obj != null ? obj : "null";
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

        TreeViewItem emptyListItem;

        private UIElement Visualize(object obj)
        {
            // Create top level table grid
            Grid tableGrid = new Grid();

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
            IResultTreeItem resultTreeItem = ResultTreeItem.Create(obj, obj.GetType(), "result", null);

            tree.PreviewKeyDown += Tree_PreviewKeyDown;
            tree.Items.Add(header);
            tree.Items.Add(CreateTreeItem(resultTreeItem, 0));
            ((TreeViewItem)tree.Items[1]).IsSelected = true;
            tableGrid.Children.Add(tree);
            return tableGrid;
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
                        parent = parent.Parent as TreeViewItem;
                    }
                    else
                    {
                        // We are not at the last item
                        return;
                    }
                }

                e.Handled = true;
                tree.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
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
                tree.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
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
            public IResultTreeItem ResultTreeItem { get; set; }
            public int Level { get; set; }
        }

        private static UIElement CreateTextWithIcon(string text, ImageSource icon, object tooltip = null)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            Grid.SetColumn(stackPanel, NameColumnIndex);
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            Image image = new Image();
            image.Width = image.Height = 16;
            image.Source = icon;
            image.ToolTip = tooltip;
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);
            return stackPanel;
        }

        private TreeViewItem CreateTreeItem(IResultTreeItem resultTreeItem, int level)
        {
            TreeViewItem item = new TreeViewItem();
            Grid grid = CreateTreeItemGrid(level);

            UIElement name = CreateTextWithIcon(resultTreeItem.Name, resultTreeItem.Image);
            grid.Children.Add(name);
            object itemValue = resultTreeItem.Value;
            if (itemValue is UIElement)
            {
                Grid.SetColumn((UIElement)itemValue, ValueColumnIndex);
                grid.Children.Add((UIElement)itemValue);
            }
            else
            {
                TextBlock value = new TextBlock();
                value.Text = itemValue.ToString();
                Grid.SetColumn(value, ValueColumnIndex);
                grid.Children.Add(value);
            }

            TextBlock type = new TextBlock();
            type.Text = resultTreeItem.Type;
            Grid.SetColumn(type, TypeColumnIndex);
            grid.Children.Add(type);
            item.Header = grid;
            item.Tag = new TreeViewItemTag()
            {
                ResultTreeItem = resultTreeItem,
                Level = level,
            };
            if (resultTreeItem.Children.Any())
                item.Items.Add(0);
            item.Expanded += TreeViewItem_Expanded;
            return item;
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                TreeViewItem item = e.Source as TreeViewItem;

                if ((item.Items.Count == 1) && (item.Items[0] is int))
                {
                    TreeViewItemTag tag = (TreeViewItemTag)item.Tag;
                    IResultTreeItem resultTreeItem = tag.ResultTreeItem;
                    int level = tag.Level;
                    TreeViewItem lastItem = null;

                    item.Items.Clear();
                    foreach (var child in resultTreeItem.Children.OrderBy(s => s.Name.StartsWith("[")).ThenBy(s => s.Name))
                        item.Items.Add(lastItem = CreateTreeItem(child, level + 1));

                    // Check if we need to fix empty list item width
                    if (lastItem != null && double.IsNaN(emptyListItem.Width))
                    {
                        item.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            emptyListItem.Width = item.ActualWidth - lastItem.ActualWidth;
                        }), System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
