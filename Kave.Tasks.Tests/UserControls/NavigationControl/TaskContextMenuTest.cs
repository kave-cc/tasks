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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using JetBrains.DataFlow;
using JetBrains.ReSharper.PsiGen.Util;
using KaVE.Tasks.Model;
using KaVE.Tasks.Repository;
using KaVE.Tasks.UserControls;
using KaVE.Tasks.UserControls.NavigationControl;
using KaVE.Tasks.UserControls.NavigationControl.Settings;
using Moq;
using NUnit.Framework;

namespace TaskManagerPlugin.Test.UserControls.NavigationControl
{
    [TestFixture]
    [RequiresSTA]
    public class TaskContextMenuTest
    {
        [SetUp]
        public void SetUp()
        {
            _settingsRepoMock = new Mock<IIconsSettingsRepository>();
            _settingsRepoMock.Setup(mock => mock.Settings).Returns(new IconsSettings());

            _root = new Task();
            _bottomTask = new Task(_root);
            _topTask = new Task(_root);

            _root.SubTasks.Add(_topTask);
            _root.SubTasks.Add(_bottomTask);
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(mock => mock.GetOpenTasks()).Returns(_root.SubTasks);

            _viewModel = new TaskViewModel(repoMock.Object, Lifetimes.Define("Test.lifetime").Lifetime,
                _settingsRepoMock.Object);
            _menuContext = new MenuContext();
            _contextMenu = new TaskContextMenu(_menuContext);
            _results = new List<string>();
            _contextMenu.MenuItemClicked += (sender, name) => _results.Add(name);
        }

        [TearDown]
        public void Cleanup()
        {
            File.Delete(FileUri);
        }

        private Task _root;
        private Task _bottomTask;
        private Task _topTask;

        private TaskContextMenu _contextMenu;
        private List<string> _results;
        private TaskViewModel _viewModel;
        private MenuContext _menuContext;
        private Mock<IIconsSettingsRepository> _settingsRepoMock;

        private const string FileUri = "test.json";

        private static void Click(MenuItem item)
        {
            item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        [Test]
        public void WhenMenuItemActivateIsClicked_EventIsTriggered()
        {
            Click(_contextMenu.MenuItemActivate);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemActivate", _results[0]);
        }

        [Test]
        public void WhenMenuItemCloseIsClicked_EventIsTriggered()
        {
            Click(_contextMenu.MenuItemClose);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemClose", _results[0]);
        }

        [Test]
        public void WhenMenuItemDeleteIsClicked_EventIsTriggered()
        {
            Click(_contextMenu.MenuItemDelete);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemDelete", _results[0]);
        }

        [Test]
        public void WhenMenuItemEditIsClicked_EventIsTriggered()
        {
            Click(_contextMenu.MenuItemEdit);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemEdit", _results[0]);
        }

        [Test]
        public void WhenMenuItemMoveDownIsClicked_EventIsTriggered()
        {
            Click(_contextMenu.MenuItemMoveDown);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemMoveDown", _results[0]);
        }

        [Test]
        public void WhenMenuItemMoveLeftIsClicked_EventIsTriggered()
        {
            Click(_contextMenu.MenuItemMoveLeft);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemMoveLeft", _results[0]);
        }

        [Test]
        public void WhenMenuItemMoveRightIsClicked_EventIsTriggered()
        {
            Click(_contextMenu.MenuItemMoveRight);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemMoveRight", _results[0]);
        }

        [Test]
        public void WhenMenuItemOpenIsClicked_EventIsTriggered()
        {
            Click(_contextMenu.MenuItemOpen);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemOpen", _results[0]);
        }

        [Test]
        public void WhenMenuItemPauseIsClicked_EventIsTriggered()
        {
            Click(_contextMenu.MenuItemPause);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemPause", _results[0]);
        }

        [Test]
        public void WhenTaskIsActive_PauseTaskIsEnabled()
        {
            var activeTask = new Task {IsActive = true};
            _menuContext.Task = activeTask;

            Assert.IsTrue(_contextMenu.MenuItemPause.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _contextMenu.MenuItemPause.Visibility);
            Assert.IsFalse(_contextMenu.MenuItemActivate.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _contextMenu.MenuItemActivate.Visibility);
        }

        [Test]
        public void WhenTaskIsClosed_OpenIsEnabled()
        {
            var closedTask = new Task {IsOpen = false};
            _menuContext.Task = closedTask;

            Assert.IsTrue(_contextMenu.MenuItemOpen.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _contextMenu.MenuItemOpen.Visibility);
            Assert.IsFalse(_contextMenu.MenuItemClose.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _contextMenu.MenuItemClose.Visibility);
        }

        [Test]
        public void WhenTaskIsNotActive_PauseTaskIsEnabled()
        {
            var task = new Task {IsActive = false};
            _menuContext.Task = task;

            Assert.IsTrue(_contextMenu.MenuItemActivate.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _contextMenu.MenuItemActivate.Visibility);
            Assert.IsFalse(_contextMenu.MenuItemPause.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _contextMenu.MenuItemPause.Visibility);
        }

        [Test]
        public void WhenTaskIsNotOnBottom_MoveDownIsEnabled()
        {
            _menuContext.Task = _topTask;

            Assert.IsTrue(_contextMenu.MenuItemMoveDown.IsEnabled);
        }

        [Test]
        public void WhenTaskIsOnTop_MoveUpIsDisabled()
        {
            _menuContext.Task = _topTask;

            Assert.IsFalse(_contextMenu.MenuItemMoveUp.IsEnabled);
        }

        [Test]
        public void WhenTaskIsNotOnTop_MoveUpIsEnabled()
        {
            _menuContext.Task = _bottomTask;

            Assert.IsTrue(_contextMenu.MenuItemMoveUp.IsEnabled);
        }

        [Test]
        public void WhenTaskIsOnBottom_MoveDownIsDisabled()
        {
            _menuContext.Task = _bottomTask;
;
            Assert.IsFalse(_contextMenu.MenuItemMoveDown.IsEnabled);
        }

        [Test]
        public void WhenTaskIsNull_AllAreDisabled()
        {
            _menuContext.Task = null;

            Assert.IsFalse(_contextMenu.MenuItemEdit.IsEnabled);
            Assert.IsFalse(_contextMenu.MenuItemDelete.IsEnabled);
        }

        [Test]
        public void WhenTaskIsOpen_CloseIsEnabled()
        {
            var openTask = new Task {IsOpen = true};
            _menuContext.Task = openTask;

            Assert.IsTrue(_contextMenu.MenuItemClose.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _contextMenu.MenuItemClose.Visibility);
            Assert.IsFalse(_contextMenu.MenuItemOpen.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _contextMenu.MenuItemOpen.Visibility);
        }
    }
}