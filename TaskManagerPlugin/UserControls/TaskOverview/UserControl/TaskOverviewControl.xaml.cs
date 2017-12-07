/*
 * Copyright 2017 Nico Strebel
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *    http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JetBrains.DataFlow;
using KaVE.Tasks.Model;
using KaVE.Tasks.UserControls.NavigationControl;

namespace KaVE.Tasks.UserControls.TaskOverview.UserControl
{
    /// <summary>
    ///     Interaction logic for TaskOverviewControl.xaml
    /// </summary>
    public partial class TaskOverviewControl : IControlClosed
    {
        public delegate void SelectedTaskChangedHandler(Task task);

        public delegate void TaskDoubleClickedHandler(Task task);

        public delegate void TaskRightClickedHandler(object sender, MouseButtonEventArgs args);

        private readonly Lifetime _lifetime;
        private readonly TaskViewModel _viewModel;
        private Task _editTask;
        private TextBlock _lastSelectedTextBlock;
        private bool _openTabSelected = true;
        private string _textBoxCache = "";

        private Task SelectedTask => _openTabSelected ? (Task)OpenTasks.SelectedItem : (Task)ClosedTasks.SelectedItem;
        private bool Editing => _editTask != null;
        public event ControlClosedHandler ControlClosed;

        public event SelectedTaskChangedHandler SelectedTaskChanged;
        public event TaskDoubleClickedHandler TaskDoubleClicked;
        public event TaskRightClickedHandler TaskRightClicked;


        public TaskOverviewControl(TaskViewModel viewModel, MainNavigationControl navigation = null)
        {
            _viewModel = viewModel;
            _lifetime = _viewModel.Lifetime;
            DataContext = _viewModel;
            _viewModel.PropertyChanged += RefreshList;

            if (navigation != null)
                navigation.CreateTaskRequested += CreateTaskRequested;

            InitializeComponent();
        }

        private void CreateTaskRequested()
        {
            _viewModel.AddTask(new Task {Title = "New Task"});
        }

        private void RefreshList(object sender, EventArgs e)
        {
            OpenTasks.Items.Refresh();
            OnSelectedTaskChanged(SelectedTask);
        }

        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            OnSelectedTaskChanged(SelectedTask);
            if (!Equals(e.NewValue, _lastSelectedTextBlock?.DataContext))
            {
                _lastSelectedTextBlock = null;
            }
        }

        private void OverviewTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _openTabSelected = OpenTab.IsSelected;
            OnSelectedTaskChanged(SelectedTask);
        }

        private void TreeViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!Editing)
            {
                var task = (Task) (sender as TreeViewItem).DataContext;
                OnTaskDoubleClicked(task);
                e.Handled = true;
            }
        }

        private void TreeViewItem_RightClick(object sender, MouseButtonEventArgs e)
        {
            OnTaskRightClicked(sender, e);
        }

        protected virtual void OnSelectedTaskChanged(Task task)
        {
            SelectedTaskChanged?.Invoke(task);
        }

        protected virtual void OnTaskDoubleClicked(Task task)
        {
            TaskDoubleClicked?.Invoke(task);
        }

        protected virtual void OnControlClosed()
        {
            ControlClosed?.Invoke(this);
        }

        protected virtual void OnTaskRightClicked(object sender, MouseButtonEventArgs args)
        {
            TaskRightClicked?.Invoke(sender, args);
        }

        private void TextBox_Click(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (Equals(textBlock, _lastSelectedTextBlock))
            {
                var parent = textBlock.Parent as StackPanel;
                var textBox = parent?.Children.OfType<TextBox>().FirstOrDefault();
                if (textBox != null)
                {
                    textBlock.Visibility = Visibility.Collapsed;
                    textBox.Visibility = Visibility.Visible;
                    textBox.Focus();
                    e.Handled = true;
                }
            }
            else
            {
                _lastSelectedTextBlock = textBlock;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            EmitChangeEvent(textBox);
            _editTask = null;
            ShowTextBox(textBox);
        }

        private void EmitChangeEvent(TextBox textBox)
        {
            var textWasChanged = textBox.Text != _textBoxCache;
            if (!textWasChanged) return;

            var newVersion = _editTask;
            var isTitleBox = textBox.Name == "Title";
            var oldVersion = (Task) newVersion.Clone();
            if (isTitleBox)
                oldVersion.Title = _textBoxCache;
            else
                oldVersion.Description = _textBoxCache;

            _viewModel.OnTaskEdited(oldVersion, newVersion);
        }

        private static void ShowTextBox(TextBox textBox)
        {
            var parent = textBox?.Parent as StackPanel;
            var textBlock = parent?.Children.OfType<TextBlock>().FirstOrDefault();

            if (textBlock != null)
            {
                textBox.Visibility = Visibility.Collapsed;
                textBlock.Visibility = Visibility.Visible;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = sender as TextBox;
                MoveFocusToFocusableParent(textBox);
            }
            else if (e.Key == Key.Escape)
            {
                var textBox = sender as TextBox;
                textBox.Text = _textBoxCache;
                MoveFocusToFocusableParent(textBox);
            }
        }

        private void MoveFocusToFocusableParent(TextBox textBox)
        {
            var parent = (FrameworkElement) textBox.Parent;
            while (parent != null && parent is IInputElement && !((IInputElement) parent).Focusable)
                parent = (FrameworkElement) parent.Parent;

            var scope = FocusManager.GetFocusScope(textBox);
            FocusManager.SetFocusedElement(scope, parent);
        }

        private void TextBox_KeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.SelectAll();
            _textBoxCache = textBox.Text;
            _editTask = textBox.DataContext as Task;
        }

        private void TextBox_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = Editing;
        }
    }
}