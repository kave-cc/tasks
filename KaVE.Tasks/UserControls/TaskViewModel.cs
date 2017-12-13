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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.DataFlow;
using KaVE.Commons.Model.Events.Tasks;
using KaVE.Tasks.Events;
using KaVE.Tasks.Model;
using KaVE.Tasks.Repository;
using KaVE.Tasks.UserControls.NavigationControl.Settings;

namespace KaVE.Tasks.UserControls
{
    [ShellComponent]
    public class TaskViewModel : INotifyPropertyChanged
    {
        private readonly ITaskRepository _repository;
        private IIconsSettingsRepository _settingsRepository;
        public IconsSettings IconsSettings { get; set; }
        public readonly Lifetime Lifetime;

        public Task ActiveTask { get; private set; }

        public ObservableCollection<Task> OpenTasks { get; set; }
        public ObservableCollection<Task> ClosedTasks { get; set; }

        public event TaskChangeHandler TaskChange;
        public delegate void TaskChangeHandler(object sender, TaskEventArgs args);
        
        public TaskViewModel(ITaskRepository repository, Lifetime lifetime, IIconsSettingsRepository settingsRepository, TaskEventGenerator generator = null)
        {
            if (generator != null)
            {
                TaskChange += generator.FireTaskEvent;
            }

            _settingsRepository = settingsRepository;
            IconsSettings = _settingsRepository.Settings;
            IconsSettings.PropertyChanged += OnSettingsChanged;
            Lifetime = lifetime;
            Lifetime.AddAction(OnLifetimeTerminate);
            _repository = repository;
            _repository.PropertyChanged += OnRepositoryChanged;
            OnRepositoryChanged(null, null);
            SetActiveTask();
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            _settingsRepository.Settings = IconsSettings;
        }

        private void OnLifetimeTerminate()
        {
            ActiveTask?.Pause();
        }

        private void OnRepositoryChanged(object sender, PropertyChangedEventArgs e)
        {
            OpenTasks = _repository.GetOpenTasks();
            ClosedTasks = _repository.GetClosedTasks();
            OnPropertyChanged(nameof(OpenTasks));
            OnPropertyChanged(nameof(ClosedTasks));
        }

        private void SetActiveTask()
        {
            ActiveTask = FindActiveTask(OpenTasks);
            ActiveTask?.UnPause();
        }

        private static Task FindActiveTask(ObservableCollection<Task> tasks)
        {
            foreach (var task in tasks)
            {
                if (task.IsActive)
                {
                    return task;
                }

                Task subTaskResult = FindActiveTask(task.SubTasks);
                if (subTaskResult != null)
                {
                    return subTaskResult;
                }
            }

            return null;
        }

        public void AddTask(Task task)
        {
            _repository.AddTask(task);
            OnTaskCreated(task);
        }

        public void AddSubTask(Task parent, Task subtask)
        {
            _repository.AddSubTask(subtask, parent.Id);
            OnTaskCreated(subtask);
        }

        public void RemoveTask(Task task)
        {
            if (task == ActiveTask)
            {
                ActiveTask = null;
                OnPropertyChanged(nameof(ActiveTask));
            }

            OnTaskChanged(task, TaskAction.Delete);
            _repository.RemoveTask(task.Id);
        }

        public void ActivateTask(Task task)
        {
            if (Equals(task, ActiveTask)) return;

            ActiveTask?.Open();
            task.Activate();
            ActiveTask = task;
            OnPropertyChanged(nameof(ActiveTask));
            OnTaskChanged(task, TaskAction.Activate);
            _repository.UpdateTask(task.Id, task);
        }

        public void CompleteTask(Task task)
        {
            if (task == ActiveTask)
            {
                ActiveTask = null;
                OnPropertyChanged(nameof(ActiveTask));
            }
            task.Close();
            OnTaskChanged(task, TaskAction.Complete);
            UpdateTask(task);
        }

        public void UpdateTask(Task task)
        {
            _repository.UpdateTask(task.Id, task);
        }

        public void OpenTask(Task task)
        {
            var action = TaskAction.UndoComplete;
          
            if (ActiveTask == task)
            {
                ActiveTask = null;
                OnPropertyChanged(nameof(ActiveTask));
                action = TaskAction.Pause;
            }
            task.Open();
            OnTaskChanged(task, action);
            _repository.UpdateTask(task.Id, task);
        }

        public void MoveSubtask(Task source, Task target)
        {
            var parent = source.Parent;
            var parentParent = parent.Parent;

            if (parentParent != null && parentParent.Equals(target))
            {
                int parentPosition = GetPosition(parent);
                _repository.MoveTaskTo(source.Id, target.Id, parentPosition + 1);
            }
            else
            {
                _repository.MoveTask(source.Id, target.Id);
            }

            OnTaskMoved(source);
        }

        private int GetPosition(Task task)
        {
            var parent = task.Parent;
            return parent.SubTasks.IndexOf(task);
        }

        public void IncreasePriority(Task task)
        {
            _repository.IncreasePriority(task.Id);
        }

        public void DecreasePriority(Task task)
        {
            _repository.DecreasePriority(task.Id);
        }

        public void FinishActiveIntervalAt(DateTimeOffset dateTime)
        {
            _repository.CloseLatestInterval(ActiveTask.Id, dateTime);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnTaskCreated(Task task)
        {
            var args = new TaskEventArgs(task, TaskAction.Create);
            TaskChange?.Invoke(this, args);
        }

        protected virtual void OnTaskChanged(Task task, TaskAction action)
        {
            var args = new TaskEventArgs(task, action);
            TaskChange?.Invoke(this, args);
        }

        public virtual void OnTaskEdited(Task task)
        {
            var args = new TaskEventArgs(task, TaskAction.Edit);
            TaskChange?.Invoke(this, args);
        }

        protected virtual void OnTaskMoved(Task task)
        {
            var args = new TaskMovedEventArgs(task);
            TaskChange?.Invoke(this, args);
        }
    }
}