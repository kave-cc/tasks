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
using System.Windows;
using JetBrains.Annotations;
using TaskManagerPlugin.Model;
using TaskManagerPlugin.Repository;

namespace TaskManagerPlugin.UserControls.NavigationControl
{
    public class MenuContext : INotifyPropertyChanged
    {
        private Task _task;
        public Task Task
        {
            get => _task;
            set
            {
                _task = value;
                UpdateTaskValues();
            }
        }

        private bool _forwardEnabled;
        public bool ForwardEnabled
        {
            get => _forwardEnabled;
            set => SetField(ref _forwardEnabled, value);
        }


        private bool _backwardEnabled;

        public bool BackwardEnabled
        {
            get => _backwardEnabled;
            set => SetField(ref _backwardEnabled, value);
        }

        private void UpdateTaskValues()
        {
            if (_task == null)
            {
                SetAllPropertiesToFalse();
                return;
            }

            var relevantParentSubTasks = TaskRepository.GetRelevantParentSubTasks(_task);
            Task parent = _task.Parent;
            int index = relevantParentSubTasks.IndexOf(_task);
            
            MoveUpEnabled = parent !=  null && relevantParentSubTasks.Count > 0 && !_task.Equals(relevantParentSubTasks[0]);
            MoveDownEnabled = parent != null && relevantParentSubTasks.Count > 1 && !_task.Equals(relevantParentSubTasks[relevantParentSubTasks.Count - 1]);
            MoveLeftEnabled = parent != null && parent.Id != TaskRepository.RootTaskId;
            MoveRightEnabled = index > 0;

            EditEnabled = _task != null;
            DeleteEnabled = _task != null;

            ActivateEnabled = !_task.IsActive;
            PauseEnabled = _task.IsActive;
            OpenEnabled = !_task.IsOpen;
            CloseEnabled = _task.IsOpen;
        }

        private void SetAllPropertiesToFalse()
        {
            MoveUpEnabled = false;
            MoveDownEnabled = false;
            MoveLeftEnabled = false;
            MoveRightEnabled = false;

            EditEnabled = false;
            DeleteEnabled = false;

            ActivateEnabled = false;
            PauseEnabled = false;
            OpenEnabled = false;
            CloseEnabled = false;
        }

        private bool _moveUpEnabled;
        public bool MoveUpEnabled
        {
            get => _moveUpEnabled;
            set => SetField(ref _moveUpEnabled, value);
        }

        private bool _moveDownEnabled;
        public bool MoveDownEnabled
        {
            get => _moveDownEnabled;
            set => SetField(ref _moveDownEnabled, value);
        }

        private bool _moveLeftEnabled;
        public bool MoveLeftEnabled
        {
            get => _moveLeftEnabled;
            set => SetField(ref _moveLeftEnabled, value);
        }

        private bool _moveRightEnabled;
        public bool MoveRightEnabled
        {
            get => _moveRightEnabled;
            set => SetField(ref _moveRightEnabled, value);
        }

        private bool _editEnabled;
        public bool EditEnabled
        {
            get => _editEnabled;
            set => SetField(ref _editEnabled, value);
        }

        private bool _deleteEnabled;
        public bool DeleteEnabled
        {
            get => _deleteEnabled;
            set => SetField(ref _deleteEnabled, value);
        }

        private bool _activateEnabled;
        public bool ActivateEnabled
        {
            get => _activateEnabled;
            set => SetField(ref _activateEnabled, value);
        }

        private bool _pauseEnabled;
        public bool PauseEnabled
        {
            get => _pauseEnabled;
            set => SetField(ref _pauseEnabled, value);
        }

        private bool _openEnabled;
        public bool OpenEnabled
        {
            get => _openEnabled;
            set => SetField(ref _openEnabled, value);
        }

        private bool _closeEnabled;
        public bool CloseEnabled
        {
            get => _closeEnabled;
            set => SetField(ref _closeEnabled, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
