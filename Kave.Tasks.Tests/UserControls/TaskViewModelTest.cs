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
using System.Collections.ObjectModel;
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
using Moq;
using NUnit.Framework;

namespace TaskManagerPlugin.Test.UserControls
{
    [TestFixture]
    public class TaskViewModelTest
    {
        private const string FileUri = "test.json";
        private Mock<ITaskRepository> _repositoryMock;
        private TaskEventGenerator _generator;
        private Mock<IIconsSettingsRepository> _settingsRepoMock;
        private TaskEventArgs _eventArgs;

        [SetUp]
        public void SetUp()
        {
            _eventArgs = null;

            _settingsRepoMock = new Mock<IIconsSettingsRepository>();
            _settingsRepoMock.Setup(mock => mock.Settings).Returns(new IconsSettings());

            _repositoryMock = new Mock<ITaskRepository>();
            var messageBus = new Mock<IMessageBus>();
            var env = new Mock<IRSEnv>();
            var dateUtils = new Mock<IDateUtils>();
            var threading = new Mock<IThreading>();
            _generator = new TaskEventGenerator(env.Object, messageBus.Object, dateUtils.Object, threading.Object, new TasksVersionUtil());
        }

        [TearDown]
        public void Cleanup()
        {
            File.Delete(FileUri);
        }

        [Test]
        public void WhenActiveTaskIsStored_ShouldSetActiveTask()
        {
            var activeTask = new Task {IsActive = true};
            activeTask.Intervals.Add(new Interval());
            var tasks = new ObservableCollection<Task> {activeTask};
            _repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(tasks);

            var viewModel = new TaskViewModel(_repositoryMock.Object, Lifetimes.Define("Test.lifetime").Lifetime,
                _settingsRepoMock.Object, _generator);

            Assert.AreEqual(activeTask, viewModel.ActiveTask);
        }

        [Test]
        public void WhenNoActiveTaskIsStored_ShouldNullActiveTask()
        {
            var task = new Task();
            var tasks = new ObservableCollection<Task> { task };
            _repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(tasks);

            var viewModel = new TaskViewModel(_repositoryMock.Object, Lifetimes.Define("Test.lifetime").Lifetime,
                _settingsRepoMock.Object, _generator);

            Assert.IsNull(viewModel.ActiveTask);
        }

        [Test]
        public void WhenLifetimeIsTerminated_ShouldCloseActiveTask()
        {
            var activeTask = new Task {IsActive = true};
            activeTask.Intervals.Add(new Interval());
            var tasks = new ObservableCollection<Task> { activeTask };
            _repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(tasks);
            
            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repositoryMock.Object, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            
            viewModel.ActivateTask(activeTask);

            lifetimeDefinition.Terminate();

            var interval = activeTask.Intervals[0];
            Assert.IsTrue(interval.IsClosed);
        }

       [Test]
        public void WhenRepositoryIsRestarted_ShouldCreateNewIntervalOnActiveTask()
        {
            var task = new Task { IsActive = true };
            task.Intervals.Add(new Interval());
            var tasks = new ObservableCollection<Task> { task };
            _repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(tasks);

            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repositoryMock.Object, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.AddTask(task);
            viewModel.ActivateTask(task);

            lifetimeDefinition.Terminate();

            Assert.AreEqual(1, task.Intervals.Count);

            var viewModel2 = new TaskViewModel(_repositoryMock.Object, Lifetimes.Define("Test.lifetime").Lifetime,
                _settingsRepoMock.Object);
            var activeTask = viewModel2.ActiveTask;

            Assert.AreEqual(2, activeTask.Intervals.Count);
            Assert.IsTrue(activeTask.Intervals[1].IsActive);

        }

        [Test]
        public void WhenTaskIsMoved_FiresTaskMoveEvent()
        {
            var task = new Task() { Title = "Title" };
            var task2 = new Task() { Title = "Title2" };
            var root = new Task() { SubTasks = new ObservableCollection<Task>() { task, task2 }};

            task.Parent = root;
            task2.Parent = root;

            _repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(root.SubTasks);
            _repositoryMock.Setup(mock => mock.MoveTaskTo(task.Id, task2.Id, It.IsAny<int>()));
            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");

            var viewModel = new TaskViewModel(_repositoryMock.Object, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;

            viewModel.MoveSubtask(task2, task);

            Assert.AreEqual(TaskAction.Move, _eventArgs.Action);
        }

        [Test]
        public void WhenTaskIsCreated_FiresTaskCreateEvent()
        {
            _repositoryMock.Setup(mock => mock.AddTask(It.IsAny<Task>())).Returns("ID");
            _repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(new ObservableCollection<Task>());

            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repositoryMock.Object, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;
            var task = new Task() { Title = "Title" };

            viewModel.AddTask(task);

            Assert.AreEqual(TaskAction.Create, _eventArgs.Action);
        }

        [Test]
        public void WhenTaskIsActivated_FiresTaskActivateEvent()
        {
            var task = new Task();
            _repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(new ObservableCollection<Task>());

            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repositoryMock.Object, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;
            
            viewModel.ActivateTask(task);

            Assert.AreEqual(TaskAction.Activate, _eventArgs.Action);
        }

        [Test]
        public void WhenTaskIsPaused_FiresTaskPauseEvent()
        {
            var task = new Task();
            _repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(new ObservableCollection<Task>());

            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repositoryMock.Object, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;
            viewModel.ActivateTask(task);
            viewModel.OpenTask(task);

            Assert.AreEqual(TaskAction.Pause, _eventArgs.Action);
        }

        [Test]
        public void WhenTaskIsCompleted_FiresTaskCompleteEvent()
        {
            _repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(new ObservableCollection<Task>());

            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repositoryMock.Object, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;
            viewModel.CompleteTask(new Task());

            Assert.AreEqual(TaskAction.Complete, _eventArgs.Action);
        }

        [Test]
        public void WhenTaskIsReopened_FiresTaskUndoCompleteEvent()
        {
            var task = new Task() { IsOpen = false };
            var openTasks = new ObservableCollection<Task> { task };
            _repositoryMock.Setup(mock => mock.GetOpenTasks()).Returns(new ObservableCollection<Task>());

            LifetimeDefinition lifetimeDefinition = Lifetimes.Define("Test.lifetime");
            var viewModel = new TaskViewModel(_repositoryMock.Object, lifetimeDefinition.Lifetime, _settingsRepoMock.Object);
            viewModel.TaskChange += OnReceiveEvent;
            
            viewModel.OpenTask(task);

            Assert.AreEqual(TaskAction.UndoComplete, _eventArgs.Action);
        }

        private void OnReceiveEvent(object sender, TaskEventArgs args)
        {
            _eventArgs = args;
        }
    }
}
