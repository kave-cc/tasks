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
using JetBrains.Application;

namespace TaskManagerPlugin.UserControls.NavigationControl.Settings
{
    public class IconsSettings : INotifyPropertyChanged, ICloneable
    {
        private bool _showAnnoyance = true;

        public bool ShowAnnoyance
        {
            get => _showAnnoyance;
            set => SetField(ref _showAnnoyance, value);
        }

        private bool _showImportance = true;

        public bool ShowImportance
        {
            get => _showImportance;
            set => SetField(ref _showImportance, value);
        }

        private bool _showUrgency = true;

        public bool ShowUrgency
        {
            get => _showUrgency;
            set => SetField(ref _showUrgency, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            return new IconsSettings()
            {
                ShowAnnoyance = _showAnnoyance,
                ShowImportance = _showImportance,
                ShowUrgency = _showUrgency
            };
        }
    }
}