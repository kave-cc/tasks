﻿/*
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
using System.IO;
using KaVE.Tasks.UserControls.NavigationControl.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskManagerPlugin.Test.UserControls.NavigationControl.Settings
{
    [TestClass]
    public class IconsSettingsRepositoryTest
    {
        private const string FileUri = "testSettings.json";
        private IIconsSettingsRepository _settingsRepository;

        [TestInitialize]
        public void SetUp()
        {
            _settingsRepository = new IconsSettingsRepository(FileUri);
        }

        [TestCleanup]
        public void CleanUp()
        {
            File.Delete(FileUri);
        }

        [TestMethod]
        public void WhenPropertyChanged_IsPersisted()
        {
            var settings = _settingsRepository.Settings;

            settings.ShowAnnoyance = false;
            settings.ShowImportance = false;
            settings.ShowUrgency = false;

            var readRepository = new IconsSettingsRepository(FileUri);
            var readSettings = readRepository.Settings;

            Assert.IsFalse(readSettings.ShowAnnoyance);
            Assert.IsFalse(readSettings.ShowImportance);
            Assert.IsFalse(readSettings.ShowUrgency);
        }
    }
}
