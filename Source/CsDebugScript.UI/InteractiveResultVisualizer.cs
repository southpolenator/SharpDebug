using CsDebugScript.Engine.Utility;
using CsDebugScript.UI.CodeWindow;
using CsDebugScript.UI.ResultVisualizers;
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
            IResultVisualizer resultTreeItem = ResultVisualizer.Create(obj, obj.GetType(), "result", null, this);

            resultTreeItem.Initialize();
            return new LazyUIResult(() => Visualize(resultTreeItem));
        }

        TreeViewItem emptyListItem;
        System.Windows.Threading.Dispatcher dispatcher;

        private UIElement Visualize(IResultVisualizer resultTreeItem)
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

        internal UIElement CreateTextWithIcon(string text, ImageSource icon, object tooltip = null, bool italic = false, bool bold = false)
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

        private TreeViewItem CreateTreeItem(IResultVisualizer resultTreeItem, int level)
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
                        IResultVisualizer resultTreeItem = tag.ResultTreeItem as IResultVisualizer;
                        IEnumerable<IResultVisualizer> children = tag.ResultTreeItem as IEnumerable<IResultVisualizer>;

                        try
                        {
                            if (resultTreeItem != null)
                            {
                                List<Tuple<string, IEnumerable<IResultVisualizer>>> customChildren = new List<Tuple<string, IEnumerable<IResultVisualizer>>>();

                                foreach (Tuple<string, IEnumerable<IResultVisualizer>> customChild in resultTreeItem.Children)
                                {
                                    if (customChild.Item2.Any())
                                    {
                                        if (customChild.Item1 == "[Expanded]")
                                        {
                                            List<IResultVisualizer> cachedItems = customChild.Item2.ToList();

                                            customChildren.Add(Tuple.Create(customChild.Item1, (IEnumerable<IResultVisualizer>)cachedItems));
                                            foreach (IResultVisualizer child in cachedItems)
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
                                        foreach (Tuple<string, IEnumerable<IResultVisualizer>> customChild in customChildren)
                                        {
                                            if (customChild.Item1 == "[Expanded]")
                                            {
                                                foreach (IResultVisualizer child in customChild.Item2)
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
                                List<IResultVisualizer> cachedItems = children.ToList();

                                foreach (IResultVisualizer child in children)
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
                                        foreach (IResultVisualizer child in cachedItems)
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
