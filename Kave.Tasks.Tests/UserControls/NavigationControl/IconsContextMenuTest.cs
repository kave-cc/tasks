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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.DataFlow;
using KaVE.Tasks.UserControls.NavigationControl;
using KaVE.Tasks.UserControls.NavigationControl.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskManagerPlugin.Test.UserControls.NavigationControl
{
    [TestClass]
    public class IconsContextMenuTest
    {
        private IconsSettings _iconsSettings;
        private IconsContextMenu _menu;
        private const string FileUri = "testSettings.json";

        [TestInitialize]
        public void SetUp()
        {
            _iconsSettings = new IconsSettings();
            _menu = new IconsContextMenu(_iconsSettings);
        }

        [TestCleanup]
        public void CleanUp()
        {
            File.Delete(FileUri);
        }

        [TestMethod]
        public void IconsAreShownCorrectly()
        {
            _iconsSettings.ShowAnnoyance = true;
            _iconsSettings.ShowImportance = true;
            _iconsSettings.ShowUrgency = true;

            Assert.IsTrue(_menu.MenuItemAnnoyance.IsChecked);
            Assert.IsTrue(_menu.MenuItemImportance.IsChecked);
            Assert.IsTrue(_menu.MenuItemUrgency.IsChecked);

            _iconsSettings.ShowAnnoyance = false;
            _iconsSettings.ShowImportance = false;
            _iconsSettings.ShowUrgency = false;

            Assert.IsFalse(_menu.MenuItemAnnoyance.IsChecked);
            Assert.IsFalse(_menu.MenuItemImportance.IsChecked);
            Assert.IsFalse(_menu.MenuItemUrgency.IsChecked);
        } 
    }
}
