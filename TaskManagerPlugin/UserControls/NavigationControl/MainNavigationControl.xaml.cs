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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JetBrains.ActionManagement;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Resources.Shell;
using TaskManagerPlugin.Repository;
using TaskManagerPlugin.UserControls.ActiveTask;
using TaskManagerPlugin.UserControls.EditTask;
using TaskManagerPlugin.UserControls.NavigationControl.Settings;
using TaskManagerPlugin.UserControls.TaskDetail;
using TaskManagerPlugin.UserControls.TaskOverview.UserControl;
using Task = TaskManagerPlugin.Model.Task;

namespace TaskManagerPlugin.UserControls.NavigationControl
{
    /// <summary>
    /// Interaction logic for NavigationControl.xaml
    /// </summary>
    public partial class MainNavigationControl : UserControl
    {
        private readonly List<IControlClosed> _controls;
        private readonly TaskViewModel _taskViewModel;
        private Task _selectedTask;
        private readonly Lifetime _lifetime;
        private int _currentIndex = 0;
        private readonly MenuContext _menuContext;
        private readonly TaskContextMenu _contextMenu;

        public MenuBar Menu;
        public TaskOverviewControl TaskOverviewControl;

        public event TaskOverviewControl.SelectedTaskChangedHandler SelectedTaskChanged;
        public event CreateTaskRequestedHandler CreateTaskRequested;

        public delegate void ContentChangedHandler(List<IControlClosed> controls, int currentIndex);
        public delegate void CreateTaskRequestedHandler();


        public MainNavigationControl(TaskViewModel taskViewModel)
        {
            InitializeComponent();
            _controls = new List<IControlClosed>();
            _menuContext = new MenuContext();

            Menu = new MenuBar(_menuContext);
            Menu.MenuItemClicked += MenuItemClicked;
            MenuBar.Content = Menu;

            _contextMenu = new TaskContextMenu(_menuContext);
            _contextMenu.MenuItemClicked += MenuItemClicked;

            TaskOverviewControl = new TaskOverviewControl(taskViewModel, this);
            TaskOverviewControl.SelectedTaskChanged += UpdateSelectedTask;
            TaskOverviewControl.TaskDoubleClicked += ShowTaskDetails;
            TaskOverviewControl.TaskRightClicked += TaskRightClicked;
            InsertAndShowControl(TaskOverviewControl);

            _lifetime = taskViewModel.Lifetime;
            _taskViewModel = taskViewModel;

        }

        private void ShowTaskDetails(Task task)
        {
            var control = new TaskDetailControl(task);
            InsertAndShowControl(control);
        }

        private void InsertAndShowControl(IControlClosed control)
        {
            if (_currentIndex < _controls.Count - 1)
            {
                _controls.RemoveRange(_currentIndex + 1, _controls.Count - (_currentIndex + 1));
            }

            control.ControlClosed += RemoveControl;
            _controls.Add(control);
            _currentIndex = _controls.Count - 1;
            ShowCurrentControl();
        }

        private void RemoveControl(object sender)
        {
            var control = (IControlClosed) sender;
            var index = _controls.IndexOf(control);
            if (_currentIndex == index)
            {
                NavigateBackward();
            } else if (_currentIndex > index)
            {
                _currentIndex--;
            }

            _controls.RemoveAt(index);
            ShowCurrentControl();
        }

        private void MenuItemClicked(object sender, string name)
        {
            switch (name)
            {
                case "MenuItemAdd":
                    OnCreateTaskRequested();
                    break;
                case "MenuItemMoveUp":
                    _taskViewModel.IncreasePriority(_selectedTask);
                    break;
                case "MenuItemMoveDown":
                    _taskViewModel.DecreasePriority(_selectedTask);
                    break;
                case "MenuItemMoveLeft":
                    Task newParent = _selectedTask.Parent.Parent;
                    _taskViewModel.MoveSubtask(_selectedTask, newParent);
                    break;
                case "MenuItemMoveRight":
                    MoveRightTask(_selectedTask);
                    break;
                case "MenuItemOpen":
                    _taskViewModel.OpenTask(_selectedTask);
                    break;
                case "MenuItemClose":
                    _taskViewModel.CompleteTask(_selectedTask);
                    break;
                case "MenuItemActivate":
                    _taskViewModel.ActivateTask(_selectedTask);
                    break;
                case "MenuItemPause":
                    _taskViewModel.OpenTask(_selectedTask);
                    break;
                case "MenuItemEdit":
                    InsertAndShowControl(new TaskEditControl(_taskViewModel, _selectedTask));
                    break;
                case "MenuItemDelete":
                    _taskViewModel.RemoveTask(_selectedTask);
                    break;
                case "MenuItemShowActive":
                    var actionManager = Shell.Instance.GetComponent<IActionManager>();
                    actionManager.ExecuteActionGuarded<ActiveTaskWindowAction>(_lifetime);
                    break;
                case "MenuItemAddSubTask":
                    InsertAndShowControl(new TaskEditControl(_taskViewModel, _selectedTask, true)); 
                    break;
                case "Backward":
                    NavigateBackward();
                    break;
                case "Forward":
                    NavigateForward();
                    break;
                case "MenuItemIconMenu":
                    ShowMenuSettingsMenu();
                    break;
                default:
                    break;
            }
        }

        private void ShowMenuSettingsMenu()
        {
            var iconsMenu = new IconsContextMenu(_taskViewModel.IconsSettings) {IsOpen = true};
        }

        private void TaskRightClicked(object sender, MouseButtonEventArgs e)
        {
            _contextMenu.PlacementTarget = sender as TreeViewItem;
            _contextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void NavigateBackward()
        {
            if (_currentIndex <= 0) return;

            _currentIndex--;
            ShowCurrentControl();
        }

        private void ShowCurrentControl()
        {
            ContentControl.Content = _controls[_currentIndex];
            _menuContext.BackwardEnabled = _currentIndex != 0;
            _menuContext.ForwardEnabled = _currentIndex < _controls.Count - 1;
        }

        private void NavigateForward()
        {
            if (_currentIndex >= _controls.Count - 1) return;

            _currentIndex++;
            ShowCurrentControl();
        }

        private void MoveRightTask(Task selectedTask)
        {
            Task parent = _selectedTask.Parent;
            var relevantParentSubTasks = TaskRepository.GetRelevantParentSubTasks(_selectedTask);
            int index = relevantParentSubTasks.IndexOf(_selectedTask);
            Task newParent = relevantParentSubTasks[index - 1];
            _taskViewModel.MoveSubtask(_selectedTask, newParent);
        }

        private void UpdateSelectedTask(Task task)
        {
            _selectedTask = task;
            _menuContext.Task = task;
            OnSelectedTaskChanged(task);
        }

        protected virtual void OnSelectedTaskChanged(Task task)
        {
            SelectedTaskChanged?.Invoke(task);
        }

        protected virtual void OnCreateTaskRequested()
        {
            CreateTaskRequested?.Invoke();
        }
    }

}
