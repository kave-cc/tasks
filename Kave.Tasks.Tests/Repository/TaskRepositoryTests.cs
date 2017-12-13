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
using System.IO;
using System.Threading;
using KaVE.Tasks.Model;
using KaVE.Tasks.Repository;
using NUnit.Framework;

namespace KaVE.Tasks.Test.Repository
{
    [TestFixture]
    public class TaskRepositoryTests
    {
        [SetUp]
        public void SetUp()
        {
            _counter++;
            Directory.CreateDirectory(TestDirectory);
            _repository = new TaskRepository(Path.Combine(TestDirectory, FileBase + _counter.ToString()), false);
        }

        [TearDown]
        public void Cleanup()
        {
            File.Delete(FileBase + _counter.ToString());
            _repository.Dispose();
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Directory.CreateDirectory(TestDirectory);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Directory.Delete(TestDirectory, true);
        }

        private const string FileBase = "test.json";
        private const string TestDirectory = "TaskRepositoryTest";
        private int _counter;
        private TaskRepository _repository;

        [Test]
        public void ShouldCreateFile()
        {
            _repository.AddTask(new Task());
            Assert.IsTrue(File.Exists(Path.Combine(TestDirectory, FileBase + _counter.ToString())));
        }

        [Test]
        public void ShouldMoveIntervals()
        {
            var task = new Task
            {
                Title = "Title"
            };
            var subTask = new Task
            {
                Title = "Title"
            };
            var interval = new Interval
            {
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now
            };
            subTask.Intervals.Add(interval);

            var parentId = _repository.AddTask(task);
            var childId = _repository.AddSubTask(subTask, parentId);
            _repository.RemoveTask(childId);

            var retrievedTask = _repository.GetTaskById(parentId);

            Assert.IsTrue(retrievedTask.Intervals.Contains(interval));
        }

        [Test]
        public void ShouldMoveSubTask()
        {
            var parent1 = new Task
            {
                Title = "Title"
            };
            var parent2 = new Task
            {
                Title = "Title"
            };
            var child = new Task
            {
                Title = "Title"
            };

            var parent1Id = _repository.AddTask(parent1);
            var parent2Id = _repository.AddTask(parent2);
            var childId = _repository.AddSubTask(child, parent1Id);

            _repository.MoveTask(childId, parent2Id);
            var childTask = _repository.GetTaskById(childId);
            var newParentTask = _repository.GetTaskById(parent2Id);

            Assert.IsTrue(newParentTask.SubTasks.Contains(childTask));
        }

        [Test]
        public void ShouldOnlyReturnClosedTasks()
        {
            var openTask = new Task
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask = new Task
            {
                Title = "Title"
            };
            closedTask.Close();

            _repository.AddTask(openTask);
            _repository.AddTask(closedTask);
            _repository.AddTask(closedTask);

            var results = _repository.GetClosedTasks();

            Assert.AreEqual(2, results.Count);
            foreach (var task in results)
                Assert.IsFalse(task.IsOpen);
        }

        [Test]
        public void ShouldOnlyReturnOpenTasks()
        {
            var openTask = new Task
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask = new Task
            {
                Title = "Title"
            };
            closedTask.Close();

            _repository.AddTask(openTask);
            _repository.AddTask(openTask);
            _repository.AddTask(closedTask);

            var results = _repository.GetOpenTasks();

            Assert.AreEqual(2, results.Count);
            foreach (var task in results)
                Assert.IsTrue(task.IsOpen);
        }

        [Test]
        public void ShouldRemoveTask()
        {
            var task = new Task
            {
                Title = "Title"
            };
            var id = _repository.AddTask(task);

            _repository.RemoveTask(id);
            var retrievedTask = _repository.GetTaskById(id);

            Assert.IsNull(retrievedTask);
        }

        [Test]
        public void ShouldStoreSubTask()
        {
            var task = new Task
            {
                Title = "Title"
            };
            var subTask = new Task
            {
                Title = "Title"
            };

            var parentId = _repository.AddTask(task);
            Thread.Sleep(1000);
            var childId = _repository.AddSubTask(subTask, parentId);
            Thread.Sleep(1000);
            var retrievedTask = _repository.GetTaskById(parentId);
            Thread.Sleep(1000);
            var retrievedSubTask = _repository.GetTaskById(childId);

            Assert.IsTrue(retrievedTask.SubTasks.Contains(subTask));
            Assert.AreEqual(task, retrievedTask);
            Assert.AreEqual(subTask, retrievedSubTask);
        }

        [Test]
        public void ShouldStoreTask()
        {
            var task = new Task
            {
                Title = "Title"
            };
            task.Title = "Title";
            task.Description = "Description";
            var id = _repository.AddTask(task);

            Thread.Sleep(1000);
            var retrievedTask = _repository.GetTaskById(id);

            Assert.AreEqual(task, retrievedTask);
        }

        [Test]
        public void ShouldUpdateTask()
        {
            var task = new Task
            {
                Title = "Title"
            };

            var id = _repository.AddTask(task);
            task.Title = "Title2";
            _repository.UpdateTask(id, task);

            var retrievedTask = _repository.GetTaskById(id);

            Assert.AreEqual("Title2", retrievedTask.Title);
        }

        [Test]
        public void WhenIntervalIsClosedAndTaskNotActive_DoesNothing()
        {
            var task = new Task
            {
                Title = "Title"
            };

            var interval = new Interval
            {
                StartTime = DateTimeOffset.MinValue.AddMinutes(1),
                EndTime = DateTimeOffset.MaxValue
            };

            task.Intervals.Add(interval);
            _repository.AddTask(task);

            _repository.CloseLatestInterval(task.Id, DateTimeOffset.Now);

            Assert.AreEqual(1, task.Intervals.Count);
            Assert.AreEqual(interval, task.Intervals[0]);
        }

        [Test]
        public void WhenIntervalIsOpenAndTaskActive_ClosesIntervalAndAddsNewInterval()
        {
            var task = new Task
            {
                Title = "Title",
                IsActive = true
            };

            var interval = new Interval
            {
                StartTime = DateTimeOffset.MinValue.AddMinutes(1)
            };

            task.Intervals.Add(interval);
            _repository.AddTask(task);
            var endDateTime = DateTimeOffset.Now;

            _repository.CloseLatestInterval(task.Id, endDateTime);

            var retrievedTask = _repository.GetTaskById(task.Id);

            Assert.AreEqual(2, retrievedTask.Intervals.Count);
            Assert.AreEqual(endDateTime, retrievedTask.Intervals[0].EndTime);
            Assert.IsTrue(retrievedTask.Intervals[1].IsActive);
        }

        [Test]
        public void WhenMoveTaskTo_InsertsAtCorrectPosition()
        {
            var task1 = new Task
            {
                Title = "Title"
            };
            var task2 = new Task
            {
                Title = "Title"
            };

            _repository.AddTask(task1);
            var id2 = _repository.AddTask(task2);

            _repository.MoveTaskTo(id2, TaskRepository.RootTaskId, 0);

            var rootTask = _repository.GetTaskById(TaskRepository.RootTaskId);

            Assert.AreEqual(2, rootTask.SubTasks.Count);
            Assert.AreEqual(task2, rootTask.SubTasks[0]);
            Assert.AreEqual(task1, rootTask.SubTasks[1]);
        }

        [Test]
        public void WhenParentIsNotRoot_DecreaseShouldNotSkipOtherStatus()
        {
            var openTask = new Task
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask1 = new Task
            {
                Title = "Title"
            };
            closedTask1.Close();

            var closedTask2 = new Task
            {
                Title = "Title"
            };
            closedTask2.Close();

            var parent = new Task
            {
                Title = "Title"
            };

            var parentId = _repository.AddTask(parent);
            var idClosed1 =_repository.AddSubTask(closedTask1, parentId);
            _repository.AddSubTask(openTask, parentId);
            _repository.AddSubTask(closedTask2, parentId);

            _repository.DecreasePriority(idClosed1);
            var fetchedParentTask = _repository.GetTaskById(parentId);

            Assert.AreEqual(3, fetchedParentTask.SubTasks.Count);
             Assert.AreEqual(openTask, fetchedParentTask.SubTasks[0]);
            Assert.AreEqual(closedTask1, fetchedParentTask.SubTasks[1]);
            Assert.AreEqual(closedTask2, fetchedParentTask.SubTasks[2]);
        }

        [Test]
        public void WhenParentIsNotRoot_IncreaseShouldNotSkipOtherStatus()
        {
            var openTask = new Task
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask1 = new Task
            {
                Title = "Title"
            };
            closedTask1.Close();

            var closedTask2 = new Task
            {
                Title = "Title"
            };
            closedTask2.Close();

            var parentTask = new Task
            {
                Title = "Title"
            };

            var parentId = _repository.AddTask(parentTask);
            _repository.AddSubTask(closedTask1, parentId);
            _repository.AddSubTask(openTask, parentId);
            var closed2Id = _repository.AddSubTask(closedTask2, parentId);

            _repository.IncreasePriority(closed2Id);
            var requestedParentTask = _repository.GetTaskById(parentId);

            Assert.AreEqual(3, requestedParentTask.SubTasks.Count);
            Assert.AreEqual(closedTask1, requestedParentTask.SubTasks[0]);
            Assert.AreEqual(closedTask2, requestedParentTask.SubTasks[1]);
            Assert.AreEqual(openTask, requestedParentTask.SubTasks[2]);
        }

        [Test]
        public void WhenParentIsRoot_DecreaseShouldSkipOtherStatus()
        {
            var openTask = new Task
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask1 = new Task
            {
                Title = "Title"
            };
            closedTask1.Close();

            var closedTask2 = new Task
            {
                Title = "Title"
            };
            closedTask2.Close();

            var closed1Id = _repository.AddTask(closedTask1);
            _repository.AddTask(openTask);
            _repository.AddTask(closedTask2);

            _repository.DecreasePriority(closed1Id);
            var rootTask = _repository.GetTaskById(TaskRepository.RootTaskId);

            Assert.AreEqual(3, rootTask.SubTasks.Count);
            Assert.AreEqual(openTask, rootTask.SubTasks[0]);
            Assert.AreEqual(closedTask2, rootTask.SubTasks[1]);
            Assert.AreEqual(closedTask1, rootTask.SubTasks[2]);
        }

        [Test]
        public void WhenParentIsRoot_IncreaseShouldSkipOtherStatus()
        {
            var openTask = new Task
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask1 = new Task
            {
                Title = "Title"
            };
            closedTask1.Close();

            var closedTask2 = new Task
            {
                Title = "Title"
            };
            closedTask2.Close();

            _repository.AddTask(closedTask1);
            _repository.AddTask(openTask);
            var closed2Id = _repository.AddTask(closedTask2);

            _repository.IncreasePriority(closed2Id);
            var rootTask = _repository.GetTaskById(TaskRepository.RootTaskId);

            Assert.AreEqual(3, rootTask.SubTasks.Count);
            Assert.AreEqual(closedTask2, rootTask.SubTasks[0]);
            Assert.AreEqual(closedTask1, rootTask.SubTasks[1]);
            Assert.AreEqual(openTask, rootTask.SubTasks[2]);
        }

        [Test]
        public void WhenFileIsChangedExternally_UpdatesRepository()
        {
            var openTask = new Task
            {
                Title = "Title"
            };

            var id = _repository.AddTask(openTask);

            var externalRepo = new TaskRepository(Path.Combine(TestDirectory, FileBase + _counter.ToString()), false);
            var retrievedTask = externalRepo.GetTaskById(id);
            retrievedTask.Title = "Title2";
            externalRepo.UpdateTask(retrievedTask.Id, retrievedTask);
            Thread.Sleep(100);
            var originalTask = _repository.GetTaskById(retrievedTask.Id);

            Assert.AreEqual(retrievedTask, originalTask);
        }
    }
}