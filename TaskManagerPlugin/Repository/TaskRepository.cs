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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Appccelerate.StateMachine;
using Ionic.Zip;
using JetBrains.Annotations;
using JetBrains.Application;
using KaVE.Commons.Utils.Json;
using KaVE.Tasks.Model;
using KaVE.Tasks.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KaVE.Tasks.Repository
{
    public interface ITaskRepository
    {
        string AddTask(Task task);
        string AddSubTask(Task task, string parentId);
        Task GetTaskById(string id);
        void RemoveTask(string id);
        void UpdateTask(string id, Task task);
        void MoveTask(string taskId, string newParentId);
        void MoveTaskTo(string taskId, string newParentId, int position);
        ObservableCollection<Task> GetOpenTasks();
        ObservableCollection<Task> GetClosedTasks();
        void IncreasePriority(Task task);
        void DecreasePriority(Task task);
        void CloseLatestInterval(Task task, DateTimeOffset endTime);
        event PropertyChangedEventHandler PropertyChanged;
    }

    [ShellComponent]
    public class TaskRepository : INotifyPropertyChanged, ITaskRepository, IDisposable
    {
        public static string RootTaskId = "Root";
        private readonly string _fileUri;
        private FileSystemWatcher _fileWatcher;

        private Task _rootTask;

        public TaskRepository() : this(GetDefaultFileLocation())
        {
        }

        public TaskRepository(string fileUri)
        {
            _fileUri = fileUri;

            SetJsonConvertSettings();
            InitFileWatcher();
            InitRootTask();
        }

        ~TaskRepository()
        {
            Dispose(false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string AddTask(Task task)
        {
            _rootTask.SubTasks.Add(task);
            task.Parent = _rootTask;
            Persist();

            return task.Id;
        }

        public string AddSubTask(Task task, string parentId)
        {
            if (string.IsNullOrEmpty(task.Title))
                throw new InvalidOperationException("Title must not be empty!");

            var parent = GetTaskById(parentId);
            task.Parent = parent ?? throw new BadStateException("No parent found with id: " + parentId);
            parent.SubTasks.Add(task);
            Persist();

            return task.Id;
        }

        public Task GetTaskById(string id)
        {
            if (id == RootTaskId)
                return _rootTask;

            return GetTaskById(_rootTask, id);
        }

        public void RemoveTask(string id)
        {
            var task = GetTaskById(id);

            if (task != null)
            {
                var parent = task.Parent;
                task.Intervals.ForEach(interval => parent.Intervals.Add(interval));
                parent.SubTasks.Remove(task);
                Persist();
            }
        }

        public void UpdateTask(string id, Task task)
        {
            var child = GetTaskById(id);
            var parent = child.Parent;
            var index = parent.SubTasks.IndexOf(child);
            parent.SubTasks.RemoveAt(index);
            parent.SubTasks.Insert(index, task);
            Persist();
        }

        public void MoveTask(string taskId, string newParentId)
        {
            var newParent = GetTaskById(newParentId);
            MoveTaskTo(taskId, newParentId, newParent.SubTasks.Count);
        }

        public void MoveTaskTo(string taskId, string newParentId, int position)
        {
            var task = GetTaskById(taskId);
            var oldParent = task.Parent;
            var newParent = GetTaskById(newParentId);

            oldParent.SubTasks.Remove(task);
            newParent.SubTasks.Insert(position, task);
            task.Parent = newParent;
            Persist();
        }

        public ObservableCollection<Task> GetOpenTasks()
        {
            var openTasks = new ObservableCollection<Task>();
            _rootTask.SubTasks.ForEach(task =>
            {
                if (task.IsOpen)
                    openTasks.Add(task);
            });

            return openTasks;
        }

        public ObservableCollection<Task> GetClosedTasks()
        {
            var closedTasks = new ObservableCollection<Task>();
            _rootTask.SubTasks.ForEach(task =>
            {
                if (!task.IsOpen)
                    closedTasks.Add(task);
            });

            return closedTasks;
        }

        public void IncreasePriority(Task task)
        {
            var parent = task.Parent;

            if (parent != null)
            {
                ChangePosition(task, true);
                Persist();
            }
        }

        public void DecreasePriority(Task task)
        {
            var parent = task.Parent;

            if (parent != null)
            {
                ChangePosition(task, false);
                Persist();
            }
        }

        public void CloseLatestInterval(Task task, DateTimeOffset endTime)
        {
            var latestInterval = task.Intervals[task.Intervals.Count - 1];
            if (latestInterval.IsActive)
                latestInterval.EndTime = endTime;

            if (task.IsActive)
                task.Intervals.Add(new Interval
                {
                    StartTime = DateTimeOffset.Now
                });

            Persist();
        }

        private static string GetDefaultFileLocation()
        {
            return Path.Combine(PersistenceConstants.AppFolder, "tasks.json");
        }

        private void InitFileWatcher()
        {
            var fileInfo = new FileInfo(_fileUri);
            if (fileInfo.Directory != null)
            {
                var directory = fileInfo.Directory.FullName;
                _fileWatcher = new FileSystemWatcher(directory);
                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.EnableRaisingEvents = true;
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {

            InitRootTask();
        }

        private void InitRootTask()
        {
            if (File.Exists(_fileUri))
            {
                var json = File.ReadAllText(_fileUri);
                try
                {
                    _rootTask = json.ParseJsonTo<Task>();
                }
                catch (JsonSerializationException e)
                {
                    _rootTask = null;
                }
            }
            if (_rootTask == null)
                _rootTask = new Task
                {
                    Id = RootTaskId,
                    Title = "RootTask"
                };
            _rootTask.PropertyChanged += OnRootTaskChanged;
        }

        private void OnRootTaskChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(_rootTask));
        }

        private static void SetJsonConvertSettings()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
        }

        private Task GetTaskById(Task parent, string id)
        {
            foreach (var subTask in parent.SubTasks)
                if (subTask.Id == id)
                    return subTask;

            foreach (var subTask in parent.SubTasks)
            {
                var task = GetTaskById(subTask, id);
                if (task != null)
                    return task;
            }

            return null;
        }

        private void ChangePosition(Task task, bool upwards)
        {
            var parent = task.Parent;
            var step = upwards ? -1 : 1;
            var currentPosition = parent.SubTasks.IndexOf(task);
            var newPosition = currentPosition;

            if (parent.Id == RootTaskId)
                for (var i = newPosition + step; i >= 0 && i < parent.SubTasks.Count; i += step)
                    if (parent.SubTasks[i].IsOpen == task.IsOpen)
                    {
                        newPosition = i;
                        break;
                    }
            else if (currentPosition + step >= 0 && currentPosition + step < parent.SubTasks.Count)
                newPosition = currentPosition + step;

            parent.SubTasks.Move(currentPosition, newPosition);
        }

        public static ObservableCollection<Task> GetRelevantParentSubTasks(Task task)
        {
            if (task.Parent == null)
                return new ObservableCollection<Task>();

            if (task.Parent.Id == RootTaskId)
                return new ObservableCollection<Task>(task.Parent.SubTasks
                    .Where(subTask => subTask.IsOpen == task.IsOpen));
            return task.Parent.SubTasks;
        }

        private void Persist()
        {
            var json = _rootTask.ToCompactJson();
            File.WriteAllText(_fileUri, json);
         
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            
            return false;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileWatcher?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}