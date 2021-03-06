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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using JetBrains.DataFlow;
using KaVE.Tasks.Model;
using KaVE.Tasks.Repository;
using KaVE.Tasks.UserControls;
using KaVE.Tasks.UserControls.NavigationControl;
using KaVE.Tasks.UserControls.NavigationControl.Settings;
using Moq;
using NUnit.Framework;

namespace KaVE.Tasks.Test.UserControls.NavigationControl
{
    [TestFixture, RequiresSTA]
    public class MenuBarTest
    {
        private MenuBar _menuBar;
        private List<string> _results;
        private TaskViewModel _viewModel;
        private MenuContext _menuContext;
        private Mock<IIconsSettingsRepository> _settingsRepoMock;
        private Task _root;
        private Task _bottomTask;
        private Task _topTask;

        private const string FileUri = "test.json";

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
            var repositoryMock = new Mock<ITaskRepository>();
            repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(_root.SubTasks);

            _viewModel = new TaskViewModel(repositoryMock.Object, Lifetimes.Define("Test.lifetime").Lifetime, _settingsRepoMock.Object);

            _menuContext = new MenuContext();
            _menuBar = new MenuBar(_menuContext);
            _results = new List<string>();
            _menuBar.MenuItemClicked += (sender, name) => _results.Add(name);
        }

        [TearDown]
        public void Cleanup()
        {
            File.Delete(FileUri);
        }

        private static void Click(MenuItem item)
        {
            item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        [Test]
        public void WhenMenuItemActivateIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemActivate);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemActivate", _results[0]);
        }

        [Test]
        public void WhenMenuItemAddIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemAdd);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemAdd", _results[0]);
        }

        [Test]
        public void WhenMenuItemCloseIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemClose);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemClose", _results[0]);
        }

        [Test]
        public void WhenMenuItemDeleteIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemDelete);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemDelete", _results[0]);
        }

        [Test]
        public void WhenMenuItemEditIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemEdit);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemEdit", _results[0]);
        }

        [Test]
        public void WhenMenuItemMoveDownIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemMoveDown);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemMoveDown", _results[0]);
        }

        [Test]
        public void WhenMenuItemMoveLeftIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemMoveLeft);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemMoveLeft", _results[0]);
        }

        [Test]
        public void WhenMenuItemMoveRightIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemMoveRight);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemMoveRight", _results[0]);
        }

        [Test]
        public void WhenMenuItemOpenIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemOpen);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemOpen", _results[0]);
        }

        [Test]
        public void WhenMenuItemPauseIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemPause);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemPause", _results[0]);
        }

        [Test]
        public void WhenMenuItemShowActiveIsClicked_EventIsTriggered()
        {
            Click(_menuBar.MenuItemShowActive);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("MenuItemShowActive", _results[0]);
        }

        [Test]
        public void WhenTaskIsActive_PauseTaskIsEnabled()
        {
            var activeTask = new Task {IsActive = true};
            _menuContext.Task = activeTask;

            Assert.IsTrue(_menuBar.MenuItemPause.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _menuBar.MenuItemPause.Visibility);
            Assert.IsFalse(_menuBar.MenuItemActivate.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _menuBar.MenuItemActivate.Visibility);
        }

        [Test]
        public void WhenTaskIsClosed_CloseIsEnabled()
        {
            var closedTask = new Task {IsOpen = false};
            _menuContext.Task = closedTask;

            Assert.IsTrue(_menuBar.MenuItemOpen.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _menuBar.MenuItemOpen.Visibility);
            Assert.IsFalse(_menuBar.MenuItemClose.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _menuBar.MenuItemClose.Visibility);
        }

        [Test]
        public void WhenTaskIsNotActive_PauseTaskIsEnabled()
        {
            var openTask = new Task {IsOpen = true};
            _menuContext.Task = openTask;

            Assert.IsTrue(_menuBar.MenuItemActivate.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _menuBar.MenuItemActivate.Visibility);
            Assert.IsFalse(_menuBar.MenuItemPause.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _menuBar.MenuItemPause.Visibility);
        }

        [Test]
        public void WhenTaskIsNotOnBottom_MoveDownIsEnabled()
        {
            _menuContext.Task = _topTask;

            Assert.IsTrue(_menuBar.MenuItemMoveDown.IsEnabled);
        }

        [Test]
        public void WhenTaskIsOnTop_MoveUpIsDisabled()
        {
            _menuContext.Task = _topTask;

            Assert.IsFalse(_menuBar.MenuItemMoveUp.IsEnabled);
        }

        [Test]
        public void WhenTaskIsNotOnTop_MoveUpIsEnabled()
        {
            _menuContext.Task = _bottomTask;

            Assert.IsTrue(_menuBar.MenuItemMoveUp.IsEnabled);
        }

        [Test]
        public void WhenTaskIsOnBottom_MoveDownIsDisabled()
        {
            _menuContext.Task = _bottomTask;
            
            Assert.IsFalse(_menuBar.MenuItemMoveDown.IsEnabled);
        }


        [Test]
        public void WhenTaskIsNull_AllAreDisabled()
        {
            _menuContext.Task = null;

            Assert.IsFalse(_menuBar.MenuItemEdit.IsEnabled);
            Assert.IsFalse(_menuBar.MenuItemDelete.IsEnabled);
        }

        [Test]
        public void WhenTaskIsOpen_CloseIsEnabled()
        {
            var openTask = new Task {IsOpen = true};
            _menuContext.Task = openTask;

            Assert.IsTrue(_menuBar.MenuItemClose.IsEnabled);
            Assert.AreEqual(Visibility.Visible, _menuBar.MenuItemClose.Visibility);
            Assert.IsFalse(_menuBar.MenuItemOpen.IsEnabled);
            Assert.AreEqual(Visibility.Collapsed, _menuBar.MenuItemOpen.Visibility);
        }
    }
}