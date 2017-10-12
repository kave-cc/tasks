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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using TaskManagerPlugin.Model;
using TaskManagerPlugin.Repository;

namespace TaskManagerPlugin.UserControls.TaskDetail
{
    class TaskDetailContext : INotifyPropertyChanged
    {
        private Task _task;
        public Task Task
        {
            get => _task;
            set
            {
                _task = value;
                OnPropertyChanged(string.Empty);
            }
        }

        public TaskDetailContext(Task task)
        {
            Task = task;
        }

        public int SubTaskCount => Task.SubTasks.Count;
        public int DoneSubTasksCount => Task.SubTasks.Count(task => !task.IsOpen);
        public bool ShowSubTasks => SubTaskCount != 0;
        public bool AllSubTasksDone => SubTaskCount == DoneSubTasksCount;
        public bool ShowParentTask => _task.Parent != null && _task.Parent.Id != TaskRepository.RootTaskId;
        public event PropertyChangedEventHandler PropertyChanged;
        

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
