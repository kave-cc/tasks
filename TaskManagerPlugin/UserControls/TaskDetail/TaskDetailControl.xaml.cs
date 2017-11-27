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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Appccelerate.StateMachine;
using JetBrains.Application;
using JetBrains.Util;
using TaskManagerPlugin.Model;

namespace TaskManagerPlugin.UserControls.TaskDetail
{
    /// <summary>
    /// Interaction logic for TaskDetailControl.xaml
    /// </summary>
    public partial class TaskDetailControl : IControlClosed
    {

        private TaskDetailContext _dataContext;

        public TaskDetailControl(Task task)
        {
            _dataContext = new TaskDetailContext(task);
            DataContext = _dataContext;
            InitializeComponent();
        }

        public event ControlClosedHandler ControlClosed;

        protected virtual void OnControlClosed()
        {
            ControlClosed?.Invoke(this);
        }
    }
}
