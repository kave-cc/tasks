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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Appccelerate.StateMachine;
using JetBrains.Annotations;
using KaVE.Commons.Model.Events.Enums;
using KaVE.Tasks.Util;
using Newtonsoft.Json;

namespace KaVE.Tasks.Model
{
    public class Task : INotifyPropertyChanged, ICloneable
    {
        public string Id = Guid.NewGuid().ToString();

        private string _title = "";
        public string Title
        {
            get => _title;
            set => SetField(ref _title, value);
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        public ObservableCollection<Interval> Intervals { get; set; } = new TrulyObservableCollection<Interval>();
        public ObservableCollection<Task> SubTasks { get; set; } = new TrulyObservableCollection<Task>();

        public Task Parent { get; set; }
        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

        private bool _open = true;
        public bool IsOpen
        {
            get => _open;
            set => _open = value;
        }

        private bool _active = false;
        public bool IsActive
        {
            get => _active;
            set => _active = value;
        }

        private Likert5Point _importance = Likert5Point.Unknown;
        public Likert5Point Importance
        {
            get => _importance;
            set => SetField(ref _importance, value);
        }

        private Likert5Point _urgency = Likert5Point.Unknown;
        public Likert5Point Urgency
        {
            get => _urgency;
            set => SetField(ref _urgency, value);
        }

        private Likert5Point _annoyance = Likert5Point.Unknown;
        public Likert5Point Annoyance
        {
            get => _annoyance;
            set => SetField(ref _annoyance, value);
        }

        public Task(Task parent) : this()
        {
            this.Parent = parent;
        }

        public Task()
        {
            SubTasks.CollectionChanged += OnCollectionChanged;
        }

        [JsonIgnore]
        public TimeSpan TimeSpan => CalculateTotalTimeSpan();

        public void Close()
        {
            if (_active)
            {
                var activeInterval = Intervals[Intervals.Count - 1];
                activeInterval.EndTime = DateTimeOffset.Now;
                SetField(ref _active, false);
            }
            SetField(ref _open, false);
            SetField(ref _active, false);
        }

        public void Activate()
        {
            if (!_active)
            {
                Intervals.Add(new Interval());
            }
            SetField(ref _open, true);
            SetField(ref _active, true);
        }

        public void Open()
        {
            if (_active)
            {
                var activeInterval = Intervals[Intervals.Count - 1];
                activeInterval.EndTime = DateTimeOffset.Now;
                SetField(ref _active, false);
            }
            SetField(ref _open, true);
        }

        public void Pause()
        {
            if (_active)
            {
                var activeInterval = Intervals[Intervals.Count - 1];
                activeInterval.EndTime = DateTimeOffset.Now;
            }
        }

        public void UnPause()
        {
            var activeInterval = Intervals[Intervals.Count - 1];
            if (activeInterval.IsClosed)
            {
                Intervals.Add(new Interval());
            }
        }

        public TimeSpan CalculateTotalTimeSpan()
        {
            var timeSpan = new TimeSpan(0);

            Intervals.ForEach(interval =>
            {
                timeSpan += interval.GetTimeSpan();
            });

            SubTasks.ForEach(task =>
            {
                timeSpan += task.CalculateTotalTimeSpan();
            });

            return timeSpan;
        }

        public bool IsParentOf(Task task)
        {
            if (SubTasks.Contains(task))
            {
                return true;
            }

            foreach (var subTask in SubTasks)
            {
                if (subTask.IsParentOf(task))
                {
                    return true;
                }
            }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SubTasks));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public object Clone()
        {
            var task = new Task
            {
                Id = Id,
                Title = Title,
                Description = Description,
                CreationTime = CreationTime,
                IsOpen = IsOpen,
                Annoyance = Annoyance,
                Importance = Importance,
                Urgency = Urgency,
                Parent = Parent,
                Intervals = Intervals
            };
               
            return task;
        }

        public Task LightClone()
        {
            var task = new Task
            {
                Id = Id,
                Title = Title,
                Description = Description,
                CreationTime = CreationTime,
                IsOpen = IsOpen,
                Annoyance = Annoyance,
                Importance = Importance,
                Urgency = Urgency,
                Parent = null,
                SubTasks = null,
                Intervals = Intervals
            };

            return task;
        }
    }
}