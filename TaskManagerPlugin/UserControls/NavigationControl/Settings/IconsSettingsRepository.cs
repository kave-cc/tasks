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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using KaVE.Commons.Utils.Json;
using Newtonsoft.Json;
using TaskManagerPlugin.Util;
using JetBrains.Application;

namespace TaskManagerPlugin.UserControls.NavigationControl.Settings
{
    [ShellComponent]
    public class IconsSettingsRepository : IIconsSettingsRepository, INotifyPropertyChanged
    {
        private readonly string _fileUri;
        private IconsSettings _settings;

        public IconsSettingsRepository() : this(GetDefaultLocation()) { }

        public IconsSettingsRepository(string fileUri)
        {
            _fileUri = fileUri;

            if (File.Exists(_fileUri))
            {
                string json = File.ReadAllText(_fileUri);
                try
                {
                    _settings = json.ParseJsonTo<IconsSettings>();
                }
                catch (JsonSerializationException e)
                {
                    _settings = new IconsSettings();
                }
            }
            else
            {
                _settings = new IconsSettings();
            }

            _settings.PropertyChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            Persist();
        }

        private static string GetDefaultLocation()
        {
            return Path.Combine(PersistenceConstants.AppFolder, "taskSettings.json");
        }
     
        private void Persist()
        {
            string json = _settings.ToCompactJson();
            File.WriteAllText(_fileUri, json);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IconsSettings Settings
        {
            get => _settings;
            set
            {
                 _settings = value;
                Persist();
            }
        }
    }
}
