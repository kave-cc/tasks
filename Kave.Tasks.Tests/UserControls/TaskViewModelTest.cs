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
using System.Configuration.Internal;
using System.IO;
using JetBrains.DataFlow;
using JetBrains.Threading;
using KaVE.Commons.Model.Events.Tasks;
using KaVE.Commons.Utils;
using KaVE.Tasks;
using KaVE.Tasks.Events;
using KaVE.Tasks.Model;
using KaVE.Tasks.Repository;
using KaVE.Tasks.UserControls;
using KaVE.Tasks.UserControls.NavigationControl.Settings;
using KaVE.VS.Commons;
using KaVE.VS.Commons.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TaskManagerPlugin.Test.UserControls
{
    [TestClass]
    public class TaskViewModelTest
    {
        private const string FileUri = "test.json";
        private TaskRepository _repository;
        private TaskEventGenerator _generator;
        private Mock<IIconsSettingsRepository> _settingsRepoMock;
        private TaskEventArgs _eventArgs;

        [TestInitialize]
        public void SetUp()
        {
            _eventArgs = null;

            _settingsRepoMock = new Mock<IIconsSettingsRepository>();
            _settingsRepoMock.Setup(mock => mock.Settings).Returns(new IconsSettings());

            _repository = new TaskRepository(FileUri);
            var messageBus = new Mock<IMessageBus>();
            var env = new Mock<IRSEnv>();
            var dateUtils = new Mock<IDateUtils>();
            var threading = new Mock<IThreading>();
            _generator = new TaskEventGenerator(env.Object, messageBus.Object, dateUtils.Object, threading.Object, new TasksVersionUtil());
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(FileUri);
        }

        [TestMethod]
        public void WhenActiveTaskIsStored_ShouldSetActiveTask()
        {
            var openTask = new Task()
            {
                Title = "Title"
            };
            openTask.Open();

            var activeTask = new Task()
            {
                Title = "Title"
            };
            activeTask.Activate();

            _repository.AddTask(openTask);
            _repository.AddSubTask(activeTask, openTask.Id);

            var viewModel = new TaskViewModel(_repository, Lifetimes.Define("Test.lifetime").Lifetime,
                _settingsRepoMock.Object, _generator);

            Assert.AreEqual(activeTask, viewModel.ActiveTask);
        }

        [TestMethod]
        public void WhenNoActiveTaskIsStored_ShouldNullActiveTask()
        {
            var openTask = new Task()
            {
                Title = "Title"
            };
            openTask.Open();

            _repository.AddTask(openTask);

            var viewModel = new TaskViewModel(_repository, Lifetimes.Define("Test.lifetime").Lifetime,
                _settingsRepoMock.Object, _generator);

            Assert.IsNull(viewModel.ActiveTask);
        }

        [TestMethod]
        public void WhenLifetimeIsTerminated_ShouldCloseActiveTask()
        {
            var activeTask = new Task()
            {
                Title = "Title"
            };
            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repository, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);

            _repository.AddTask(activeTask);
            viewModel.ActivateTask(activeTask);

            lifetimeDefinition.Terminate();

            var interval = activeTask.Intervals[0];
            Assert.IsTrue(interval.IsClosed);
        }

        [TestMethod]
        public void WhenRepositoryIsRestarted_ShouldCreateNewIntervalOnActiveTask()
        {
            var task = new Task()
            {
                Title = "Title"
            };
            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repository, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.AddTask(task);
            viewModel.ActivateTask(task);

            lifetimeDefinition.Terminate();

            Assert.AreEqual(1, task.Intervals.Count);

            var viewModel2 = new TaskViewModel(_repository, Lifetimes.Define("Test.lifetime").Lifetime,
                _settingsRepoMock.Object);
            var activeTask = viewModel2.ActiveTask;

            Assert.AreEqual(2, activeTask.Intervals.Count);
            Assert.IsTrue(activeTask.Intervals[1].IsActive);

        }

        [TestMethod]
        public void WhenTaskIsMoved_FiresTaskMoveEvent()
        {
            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repository, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;
            var task = new Task() { Title = "Title" };
            var task2 = new Task() { Title = "Title" };
            viewModel.AddTask(task);
            viewModel.AddTask(task2);

            viewModel.MoveSubtask(task2, task);

            Assert.AreEqual(TaskAction.Move, _eventArgs.Action);
        }

        [TestMethod]
        public void WhenTaskIsCreated_FiresTaskCreateEvent()
        {
            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repository, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;
            var task = new Task() { Title = "Title" };

            viewModel.AddTask(task);

            Assert.AreEqual(TaskAction.Create, _eventArgs.Action);
        }

        [TestMethod]
        public void WhenTaskIsActivated_FiresTaskActivateEvent()
        {
            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repository, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;
            var task = new Task() { Title = "Title" };

            viewModel.AddTask(task);
            viewModel.ActivateTask(task);

            Assert.AreEqual(TaskAction.Activate, _eventArgs.Action);
        }

        [TestMethod]
        public void WhenTaskIsPaused_FiresTaskPauseEvent()
        {
            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repository, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;
            var task = new Task() {Title = "Title"};

            viewModel.AddTask(task);
            viewModel.ActivateTask(task);
            viewModel.OpenTask(task);

            Assert.AreEqual(TaskAction.Pause, _eventArgs.Action);
        }

        [TestMethod]
        public void WhenTaskIsCompleted_FiresTaskCompleteEvent()
        {
            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repository, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;
            var task = new Task() { Title = "Title" };

            viewModel.AddTask(task);
            viewModel.CompleteTask(task);

            Assert.AreEqual(TaskAction.Complete, _eventArgs.Action);
        }

        [TestMethod]
        public void WhenTaskIsReopened_FiresTaskUndoCompleteEvent()
        {
            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repository, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;
            var task = new Task() { Title = "Title" };

            viewModel.AddTask(task);
            viewModel.CompleteTask(task);
            viewModel.OpenTask(task);

            Assert.AreEqual(TaskAction.UndoComplete, _eventArgs.Action);
        }
        private void OnReceiveEvent(object sender, TaskEventArgs args)
        {
            _eventArgs = args;
        }
    }
}
