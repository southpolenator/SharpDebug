using CsDebugScript.Drawing.Interfaces;
using CsDebugScript.UI.CodeWindow;
using CsDebugScript.UI.Drawing;
using CsDebugScript.UI.ResultVisualizers;
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
        internal const string ExpandingItemText = "Loading...";

        private InteractiveWindowContent interactiveWindowContent;
        System.Windows.Threading.Dispatcher dispatcher;

        public InteractiveResultVisualizer(InteractiveWindowContent interactiveWindowContent)
        {
            this.interactiveWindowContent = interactiveWindowContent;
            dispatcher = interactiveWindowContent?.Dispatcher ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }


        public object Output(object obj)
        {
            // We don't visualize null
            if (obj == null)
                return null;

            // Primitive types and strings are visualized as ToString
            if (obj.GetType().IsPrimitive || obj is string)
                return obj.ToString();

            // UI elements should be resurfaced back.
            if (obj is UIElement)
                return obj;

            // Drawing objects should be resurfaced back.
            IDrawing drawing = obj as IDrawing;

            if (drawing != null)
                return new LazyUIResult(() => new DrawingViewer(drawing));

            // All other should be visualized in a table
            IResultVisualizer resultTreeItem = ResultVisualizer.Create(obj, obj.GetType(), "result", CompletionDataType.Unknown, this);

            resultTreeItem.Initialize();

            // Check if we can also represent resulting object as a drawing
            IDrawingVisualizerObject drawingVisualizerObject = obj as IDrawingVisualizerObject;

            if (drawingVisualizerObject != null && drawingVisualizerObject.CanVisualize())
            {
                Graphics graphics = new Graphics(dispatcher);

                drawing = drawingVisualizerObject.CreateDrawing(graphics);
            }

            if (drawing != null)
            {
                // Create panel that will hold both elements.
                return new LazyUIResult(() =>
                {
                    StackPanel panel = new StackPanel();

                    panel.Orientation = Orientation.Vertical;
                    panel.Children.Add(Visualize(resultTreeItem));
                    panel.Children.Add(new DrawingViewer(drawing));
                    return panel;
                });
            }

            return new LazyUIResult(() => Visualize(resultTreeItem));
        }

        private static FrameworkElementFactory CreateStackPanelFactory(string name)
        {
            FrameworkElementFactory stackPanel = new FrameworkElementFactory(typeof(StackPanel));

            stackPanel.SetValue(FrameworkElement.NameProperty, name);

            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));

            border.AppendChild(stackPanel);
            border.SetValue(Border.BorderBrushProperty, Brushes.LightGray);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 1, 0));
            border.SetValue(Border.MarginProperty, new Thickness(-6, 0, -6, 0));
            stackPanel.SetValue(Border.MarginProperty, new Thickness(6, 0, 0, 0));
            return border;
        }

        private UIElement Visualize(IResultVisualizer resultVisualizer)
        {
            // Create tree and its columns
            TreeListView tree = new TreeListView();
            tree.Columns.Add(new GridViewColumn()
            {
                Header = "Name",
                CellTemplate = new DataTemplate() { VisualTree = CreateStackPanelFactory("Name") },
                Width = 200,
            });
            tree.Columns.Add(new GridViewColumn()
            {
                Header = "Value",
                CellTemplate = new DataTemplate() { VisualTree = CreateStackPanelFactory("Value") },
                Width = 600,
            });
            tree.Columns.Add(new GridViewColumn()
            {
                Header = "Type",
                CellTemplate = new DataTemplate() { VisualTree = CreateStackPanelFactory("Type") },
                Width = 200,
            });

            // Create header row
            TreeViewItem header = new TreeViewItem();
            header.Header = new GridViewHeaderRowPresenter()
            {
                Columns = tree.Columns,
            };
            tree.Items.Add(header);

            // Create result item
            TreeViewItem resultItem = CreateTreeItem(tree, resultVisualizer, 0);
            tree.Items.Add(resultItem);
            resultItem.IsExpanded = true;

            // Initialize tree events
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
            return tree;
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
                {
                    return;
                }

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
                TreeListView tree = (TreeListView)sender;
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

        private class TreeViewItemTag
        {
            public object ResultTreeItem { get; set; }

            public int Level { get; set; }
        }

        private static childItem FindVisualChild<childItem>(DependencyObject obj, string name = null)
            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is childItem)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        FrameworkElement fe = child as FrameworkElement;

                        if (fe == null || fe.Name != name)
                        {
                            continue;
                        }
                    }
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child, name);

                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }

        internal UIElement CreateTextWithIcon(string text, ImageSource icon, object tooltip = null, bool italic = false, bool bold = false)
        {
            return dispatcher.Invoke(() =>
            {
                StackPanel stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                stackPanel.Children.Add(new Image()
                {
                    Width = 16,
                    Height = 16,
                    Source = icon,
                    ToolTip = tooltip,
                });
                stackPanel.Children.Add(new TextBlock()
                {
                    FontStyle = italic ? FontStyles.Italic : FontStyles.Normal,
                    FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = text,
                    ToolTip = tooltip,
                });
                return stackPanel;
            });
        }

        private TreeViewItem CreateTreeItem(TreeListView tree, string name, ImageSource imageSource, int level, UIElement value = null, string typeString = null, bool nameItalic = false)
        {
            TreeViewItem item = new TreeViewItem();
            bool alreadyLoaded = false;
            GridViewRowPresenter rowPresenter = new GridViewRowPresenter()
            {
                Columns = tree.Columns,
                Content = level,
            };
            item.Header = rowPresenter;
            item.Loaded += (sender, e) =>
            {
                if (alreadyLoaded)
                {
                    return;
                }
                alreadyLoaded = true;

                FrameworkElement expander = item.Template.FindName("Expander", item) as FrameworkElement;
                StackPanel nameStackPanel = FindVisualChild<StackPanel>(rowPresenter, "Name");
                StackPanel valueStackPanel = FindVisualChild<StackPanel>(rowPresenter, "Value");
                StackPanel typeStackPanel = FindVisualChild<StackPanel>(rowPresenter, "Type");

                if (expander != null && nameStackPanel != null)
                {
                    Panel expanderParent = expander.Parent as Panel;

                    if (expanderParent != null)
                    {
                        if (level > 0)
                        {
                            expanderParent.Margin = new Thickness(-19, 0, 0, 0);
                            nameStackPanel.Margin = new Thickness(10 * (level - 1), 0, 0, 0);
                        }
                        else
                        {
                            nameStackPanel.Margin = new Thickness(-19, 0, 0, 0);
                        }
                        nameStackPanel.Orientation = Orientation.Horizontal;
                        expanderParent.Children.Remove(expander);
                        nameStackPanel.Children.Add(expander);
                        nameStackPanel.Children.Add(CreateTextWithIcon(name, imageSource, italic: nameItalic));
                    }
                }
                if (valueStackPanel != null && value != null)
                {
                    valueStackPanel.Orientation = Orientation.Horizontal;
                    valueStackPanel.Children.Add(value);
                }
                if (typeStackPanel != null && typeString != null)
                {
                    typeStackPanel.Orientation = Orientation.Horizontal;
                    typeStackPanel.Children.Add(new TextBlock()
                    {
                        Text = typeString,
                    });
                }
            };
            return item;
        }

        private TreeViewItem CreateTreeItem(TreeListView tree, IResultVisualizer resultVisualizer, int level)
        {
            UIElement value = resultVisualizer.Value as UIElement;

            if (value == null)
            {
                value = new TextBlock()
                {
                    Text = resultVisualizer.ValueString,
                };
            }

            TreeViewItem item = CreateTreeItem(tree, resultVisualizer.Name, resultVisualizer.Image, level, value, resultVisualizer.Type);
            item.Tag = new TreeViewItemTag()
            {
                ResultTreeItem = resultVisualizer,
                Level = level,
            };
            if (resultVisualizer.IsExpandable)
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
                TreeListView tree = null;
                FrameworkElement parent = item?.Parent as FrameworkElement;

                while (tree == null && parent != null)
                {
                    tree = parent as TreeListView;
                    parent = parent.Parent as FrameworkElement;
                }

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

                                foreach (Tuple<string, IEnumerable<IResultVisualizer>> customChild in resultTreeItem.ChildrenGroups)
                                {
                                    if (customChild.Item2.Any())
                                    {
                                        if (customChild.Item1 == ResultVisualizer.ExpandedGroupName)
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

                                        item.Items.Clear();
                                        foreach (Tuple<string, IEnumerable<IResultVisualizer>> customChild in customChildren)
                                        {
                                            if (customChild.Item1 == ResultVisualizer.ExpandedGroupName)
                                            {
                                                foreach (IResultVisualizer child in customChild.Item2)
                                                {
                                                    item.Items.Add(CreateTreeItem(tree, child, level + 1));
                                                }
                                            }
                                            else
                                            {
                                                TreeViewItem customItem = CreateTreeItem(tree, customChild.Item1, CompletionData.GetImage(CompletionDataType.Namespace), level + 1, nameItalic: true);

                                                customItem.Tag = new TreeViewItemTag()
                                                {
                                                    Level = level + 1,
                                                    ResultTreeItem = customChild.Item2,
                                                };
                                                customItem.Items.Add(ExpandingItemText);
                                                customItem.Expanded += TreeViewItem_Expanded;
                                                item.Items.Add(customItem);
                                            }
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

                                        item.Items.Clear();
                                        foreach (IResultVisualizer child in cachedItems)
                                        {
                                            item.Items.Add(CreateTreeItem(tree, child, level + 1));
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

    internal class TreeListView : TreeView
    {
        private GridViewColumnCollection columns = new GridViewColumnCollection();

        public GridViewColumnCollection Columns => columns;
    }
}
