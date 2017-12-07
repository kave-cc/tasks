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
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace KaVE.Tasks.Model
{
    public class Interval : INotifyPropertyChanged
    {
        private DateTimeOffset _startTime = DateTimeOffset.Now;
        public DateTimeOffset StartTime {
            get => _startTime;
            set => SetField(ref _startTime, value);
        }

        private DateTimeOffset _endTime;
        public DateTimeOffset EndTime
        {
            get => _endTime;
            set => SetField(ref _endTime, value);
        }

        public bool IsActive => StartTime != DateTimeOffset.MinValue && EndTime == DateTimeOffset.MinValue;
        public bool IsClosed => StartTime != DateTimeOffset.MinValue && EndTime != DateTimeOffset.MinValue;

        public TimeSpan GetTimeSpan()
        {
            if (IsClosed)
            {
                return EndTime
                        - StartTime;
            }

            if (IsActive)
            {
                return DateTimeOffset.Now
                        - StartTime;
            }

            return new TimeSpan(0);
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