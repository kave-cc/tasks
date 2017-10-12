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
using System.Windows;
using System.Windows.Controls;
using JetBrains.DataFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskManagerPlugin.UserControls.NavigationControl;
using KaVE.Commons.TestUtils.UserControls;
using Moq;
using TaskManagerPlugin.Model;
using TaskManagerPlugin.Repository;
using TaskManagerPlugin.UserControls;
using TaskManagerPlugin.UserControls.NavigationControl.Settings;

namespace TaskManagerPlugin.Test.UserControls.NavigationControl
{
    [TestClass]
    public class MenuBarTest
    {
        private MenuBar _menuBar;
        private List<string> _results;
        private TaskViewModel _viewModel;
        private MenuContext _menuContext;
        private Task _task;
        private Mock<IIconsSettingsRepository> _settingsRepoMock;

        private const string FileUri = "test.json";

        [TestInitialize]
        public void SetUp()
        {
            _settingsRepoMock = new Mock<IIconsSettingsRepository>();
            _settingsRepoMock.Setup(mock => mock.Settings).Returns(new IconsSettings());

            var repo = new TaskRepository(FileUri);
            _task = new Task()
            {
                Title = "Title"
            };

            _viewModel = new TaskViewModel(repo, Lifetimes.Define("Test.lifetime").Lifetime, _settingsRepoMock.Object);
            _viewModel.AddTask(_task);
            _menuContext = new MenuContext();
            _menuBar = new MenuBar(_menuContext);
            _results = new List<string>();
            _menuBar.MenuItemClicked += (sender, name) => _results.Add(name);
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(FileUri);
        }

        [TestMethod]
        public void WhenMenuItemActivateIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemActivate);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemActivate", _results[0]);
        }

        [TestMethod]
        public void WhenMenuItemCloseIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemClose);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemClose", _results[0]);
        }

        [TestMethod]
        public void WhenMenuItemDeleteIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemDelete);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemDelete", _results[0]);
        }

        [TestMethod]
        public void WhenMenuItemEditIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemEdit);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemEdit", _results[0]);
        }

        [TestMethod]
        public void WhenMenuItemMoveDownIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemMoveDown);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemMoveDown", _results[0]);
        }

        [TestMethod]
        public void WhenMenuItemMoveLeftIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemMoveLeft);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemMoveLeft", _results[0]);
        }

        [TestMethod]
        public void WhenMenuItemMoveRightIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemMoveRight);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemMoveRight", _results[0]);
        }

        [TestMethod]
        public void WhenMenuItemAddIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemAdd);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemAdd", _results[0]);
        }

        [TestMethod]
        public void WhenMenuItemOpenIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemOpen);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemOpen", _results[0]);
        }

        [TestMethod]
        public void WhenMenuItemPauseIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemPause);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemPause", _results[0]);
        }

        [TestMethod]
        public void WhenMenuItemShowActiveIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemShowActive);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemShowActive", _results[0]);
        }

        [TestMethod]
        public void WhenTaskIsActive_PauseTaskIsEnabled()
        {
            _viewModel.ActivateTask(_task);
            _menuContext.Task = _task;

            Assert.IsTrue(_menuBar.MenuItemPause.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _menuBar.MenuItemPause.Visibility);
            Assert.IsFalse(_menuBar.MenuItemActivate.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _menuBar.MenuItemActivate.Visibility);
        }

        [TestMethod]
        public void WhenTaskIsNotActive_PauseTaskIsEnabled()
        {
            _viewModel.OpenTask(_task);
            _menuContext.Task = _task;

            Assert.IsTrue(_menuBar.MenuItemActivate.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _menuBar.MenuItemActivate.Visibility);
            Assert.IsFalse(_menuBar.MenuItemPause.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _menuBar.MenuItemPause.Visibility);
        }

        [TestMethod]
        public void WhenTaskIsOpen_CloseIsEnabled()
        {
            _viewModel.OpenTask(_task);
            _menuContext.Task = _task;

            Assert.IsTrue(_menuBar.MenuItemClose.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _menuBar.MenuItemClose.Visibility);
            Assert.IsFalse(_menuBar.MenuItemOpen.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _menuBar.MenuItemOpen.Visibility);
        }

        [TestMethod]
        public void WhenTaskIsClosed_CloseIsEnabled()
        {
            _viewModel.CompleteTask(_task);
            _menuContext.Task = _task;

            Assert.IsTrue(_menuBar.MenuItemOpen.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _menuBar.MenuItemOpen.Visibility);
            Assert.IsFalse(_menuBar.MenuItemClose.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _menuBar.MenuItemClose.Visibility);
        }

        [TestMethod]
        public void WhenTaskIsNotOnTop_MoveUpIsEnabled()
        {
            _viewModel.OpenTask(_task);

            var task = new Task()
            {
                Title = "Title"
            };

            _viewModel.AddTask(task);

            _menuContext.Task = task;

            Assert.IsTrue(_menuBar.MenuItemMoveUp.IsEnabled);
            Assert.IsFalse(_menuBar.MenuItemMoveDown.IsEnabled);
        }

        [TestMethod]
        public void WhenTaskIsNotOnBottom_MoveDownIsEnabled()
        {
            var task = new Task()
            {
                Title = "Title"
            };

            _viewModel.AddTask(task);

            _menuContext.Task = _task;

            Assert.IsTrue(_menuBar.MenuItemMoveDown.IsEnabled);
            Assert.IsFalse(_menuBar.MenuItemMoveUp.IsEnabled);
        }

        [TestMethod]
        public void WhenTaskIsNull_AllAreDisabled()
        {
            _menuContext.Task = null;

            Assert.IsFalse(_menuBar.MenuItemEdit.IsEnabled);
            Assert.IsFalse(_menuBar.MenuItemDelete.IsEnabled);
        }

        private static void Click(MenuItem item)
        {
            item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }
    }
}
