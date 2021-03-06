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

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using JetBrains.DataFlow;
using KaVE.Tasks.Model;
using KaVE.Tasks.Repository;
using KaVE.Tasks.UserControls;
using KaVE.Tasks.UserControls.ActiveTask;
using KaVE.Tasks.UserControls.NavigationControl.Settings;
using Moq;
using NUnit.Framework;

namespace KaVE.Tasks.Test.UserControls.ActiveTask
{
    [TestFixture, RequiresSTA]
    public class ActiveTaskWindowTest
    {
        [SetUp]
        public void SetUp()
        {
            var settingsMock = new Mock<IIconsSettingsRepository>();
            settingsMock.Setup(mock => mock.Settings).Returns(new IconsSettings());

            _task = new Task();
            _repositoryMock = new Mock<ITaskRepository>();
            _repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(new ObservableCollection<Task> {_task});
            _viewModel = new TaskViewModel(_repositoryMock.Object, Lifetimes.Define("Lifetime.test").Lifetime,
                settingsMock.Object);
            _window = new ActiveTaskWindow(_viewModel);
        }

        [TearDown]
        public void CleanUp()
        {
            File.Delete(FileUri);
        }

        private Mock<ITaskRepository> _repositoryMock;
        private ActiveTaskWindow _window;
        private TaskViewModel _viewModel;
        private Task _task;
        private const string FileUri = "test.json";

        [Test]
        public void WhenActivateTaskIsClicked_ShouldActivateTask()
        {
            _window.OpenTasksCombo.SelectedIndex = 0;
            _window.Activate.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            Assert.AreEqual(_task, _viewModel.ActiveTask);
        }

        [Test]
        public void WhenActiveTaskExists_ShouldDisplayTimeSpan()
        {
            _task.Intervals.Add(new Interval
            {
                StartTime = DateTimeOffset.Now.AddMinutes(-10),
                EndTime = DateTimeOffset.Now
            });
            _viewModel.ActivateTask(_task);

            Assert.AreEqual("Active Time: 10 minutes", _window.ActiveTime.Text);
        }
    }
}