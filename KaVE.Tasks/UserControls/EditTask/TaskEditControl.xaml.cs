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
using System.Windows.Controls;
using KaVE.Tasks.Model;

namespace KaVE.Tasks.UserControls.EditTask
{
    /// <summary>
    /// Interaction logic for TaskEditControl.xaml
    /// </summary>
    public partial class TaskEditControl : IControlClosed
    {
        public Task Task { get; set; }
        private readonly TaskViewModel _viewModel;
        private Task _parentTask;

        private readonly bool _editMode = false;

        public Task ParentTask
        {
            get => _parentTask;
            set => _parentTask = value;
        }

        public TaskEditControl(TaskViewModel viewModel)
        {
            _viewModel = viewModel;
            Task = new Task();
            
            DataContext = Task;
            InitializeComponent();
        }

        public TaskEditControl(TaskViewModel viewModel, Task task, bool createSubTask = false)
        {
            if (!createSubTask)
            {
                Task = task;
                _editMode = true;
            }
            else
            {
                Task = new Task();
                ParentTask = task;
            }

            _viewModel = viewModel;
            DataContext = Task;
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveTask();
            if (_editMode)
            {
                _viewModel.OnTaskEdited(Task);
            }
            if (!_editMode)
            {
                AddTask();
            }
            OnControlClosed();
        }

        private void SaveTask()
        {
            TaskTitle.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            Description.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            Annoyance.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            Importance.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
            Urgency.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
        }

        private void AddTask()
        {
            if (_parentTask != null)
            {
                _viewModel.AddSubTask(_parentTask, Task);
            }
            else
            {
                _viewModel.AddTask(Task);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            OnControlClosed();
        }

        public event ControlClosedHandler ControlClosed;

        protected virtual void OnControlClosed()
        {
            ControlClosed?.Invoke(this);
        }
    }
}
