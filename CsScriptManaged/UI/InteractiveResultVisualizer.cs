using CsScriptManaged.UI.CodeWindow;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private class ArrayResultTreeItem : ObjectResultTreeItem
        {
            private Array array;

            public ArrayResultTreeItem(Array array, string name, ImageSource image)
                : base(array, name, image)
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
                        yield return ResultTreeItem.Create(GetValue(() => array.GetValue(i)), string.Format("[{0}]", i), CompletionData.GetImage(CompletionDataType.Variable));
                }
            }

            public override string Value
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

            public virtual IEnumerable<IResultTreeItem> Children
            {
                get
                {
                    Type type = obj.GetType();

                    if (!type.IsPrimitive && !type.IsEnum)
                    {
                        // Non-static properties
                        var properties = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        foreach (var property in properties)
                            if (property.CanRead)
                                yield return ResultTreeItem.Create(GetValue(() => property.GetValue(obj)), property.Name, CompletionData.GetImage(CompletionDataType.Property));

                        // Static properties
                        var staticProperties = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                        foreach (var property in staticProperties)
                            if (property.CanRead)
                                yield return ResultTreeItem.Create(GetValue(() => property.GetValue(obj)), property.Name, CompletionData.GetImage(CompletionDataType.StaticProperty));

                        // Non-static fields
                        var fields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        foreach (var field in fields)
                            if (!field.IsStatic)
                                yield return ResultTreeItem.Create(GetValue(() => field.GetValue(obj)), field.Name, CompletionData.GetImage(CompletionDataType.Variable));

                        // Static fields
                        var staticFields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                        foreach (var field in staticFields)
                            if (field.IsStatic)
                                yield return ResultTreeItem.Create(GetValue(() => field.GetValue(obj)), field.Name, CompletionData.GetImage(CompletionDataType.StaticVariable));
                    }
                }
            }

            protected static object GetValue(Func<object> getValueFunction)
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

            public virtual ImageSource Image { get; private set; }

            public virtual string Name { get; private set; }

            public virtual string Type
            {
                get
                {
                    return obj.GetType().FullName;
                }
            }

            public virtual string Value
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
            headerGrid.Children.Add(emptyListItem);
            header.Focusable = false;
            header.Header = headerGrid;

            // Create table tree
            TreeView tree = new TreeView();
            IResultTreeItem resultTreeItem = ResultTreeItem.Create(obj, "result", null);

            tree.Items.Add(header);
            tree.Items.Add(CreateTreeItem(resultTreeItem, 0));
            tableGrid.Children.Add(tree);
            return tableGrid;
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

        private TreeViewItem CreateTreeItem(IResultTreeItem resultTreeItem, int level)
        {
            TreeViewItem item = new TreeViewItem();
            Grid grid = CreateTreeItemGrid(level);

            StackPanel nameStackPanel = new StackPanel();
            nameStackPanel.Orientation = Orientation.Horizontal;
            Grid.SetColumn(nameStackPanel, NameColumnIndex);
            TextBlock name = new TextBlock();
            name.Text = resultTreeItem.Name;
            Image image = new Image();
            image.Width = image.Height = 16;
            image.Source = resultTreeItem.Image;
            nameStackPanel.Children.Add(image);
            nameStackPanel.Children.Add(name);
            grid.Children.Add(nameStackPanel);
            TextBlock value = new TextBlock();
            value.Text = resultTreeItem.Value;
            Grid.SetColumn(value, ValueColumnIndex);
            grid.Children.Add(value);
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
