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
using System.Windows.Controls.Primitives;
using JetBrains.DataFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskManagerPlugin.Model;
using TaskManagerPlugin.Repository;
using TaskManagerPlugin.UserControls;
using TaskManagerPlugin.UserControls.ActiveTask;
using TaskManagerPlugin.UserControls.NavigationControl.Settings;

namespace TaskManagerPlugin.Test.UserControls.ActiveTask
{
    [TestClass]
    public class ActiveTaskWindowTest
    {
        private TaskRepository _repository;
        private ActiveTaskWindow _window;
        private TaskViewModel _viewModel;
        private const string FileUri = "test.json";

        [TestInitialize]
        public void SetUp()
        {
            var settingsMock = new Mock<IIconsSettingsRepository>();
            settingsMock.Setup(mock => mock.Settings).Returns(new IconsSettings());
            _repository = new TaskRepository(FileUri);
            _viewModel = new TaskViewModel(_repository, Lifetimes.Define("Lifetime.test").Lifetime, settingsMock.Object);
            _window = new ActiveTaskWindow(_viewModel);
        }

        [TestCleanup]
        public void CleanUp()
        {
            File.Delete(FileUri);
        }

        [TestMethod]
        public void WhenActivateTaskIsClicked_ShouldActivateTask()
        {
            _repository.AddTask(new Task() {Title = "Title"});
            _window.OpenTasksCombo.SelectedIndex = 0;
            _window.Activate.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            Assert.AreEqual(_repository.GetOpenTasks()[0], _viewModel.ActiveTask);
        }

        [TestMethod]
        public void WhenActiveTaskExists_ShouldDisplayTimeSpan()
        {
            var task = new Task() {Title = "Title"};
            _repository.AddTask(task);
            task.Intervals.Add(new Interval
            {
                StartTime = DateTimeOffset.Now.AddMinutes(-10),
                EndTime = DateTimeOffset.Now
            });
            _viewModel.ActivateTask(task);

            Assert.AreEqual("Active Time: 10 minutes", _window.ActiveTime.Text);
        }
    }
}
