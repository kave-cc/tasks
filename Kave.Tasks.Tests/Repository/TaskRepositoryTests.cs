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

namespace KaVE.Tasks.Tests.Repository
{
    public class TaskRepositoryTests
    {
        private string _dirRepo;
        private string _fileRepo;

        private TaskRepository _sut;

        [SetUp]
        public void SetUp()
        {
            _dirRepo = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _fileRepo = Path.Combine(_dirRepo, "repo.json");
            Console.WriteLine("Working with repository {0}", _fileRepo);
            _sut = new TaskRepository(_fileRepo);
        }

        [TearDown]
        public void Cleanup()
        {
            _sut.Dispose();
            if (Directory.Exists(_dirRepo))
            {
                try
                {
                    Directory.Delete(_dirRepo, true);
                }
                catch
                {
                    Directory.Delete(_dirRepo, true);
                }
            }
        }

        [Test]
        public void ShouldCreateFolderAndFileOnFirstStart()
        {
            Assert.IsTrue(Directory.Exists(_dirRepo));
            Assert.IsTrue(File.Exists(_fileRepo));
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

            var parentId = _sut.AddTask(task);
            var childId = _sut.AddSubTask(subTask, parentId);
            _sut.RemoveTask(childId);

            var retrievedTask = _sut.GetTaskById(parentId);

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

            var parent1Id = _sut.AddTask(parent1);
            var parent2Id = _sut.AddTask(parent2);
            var childId = _sut.AddSubTask(child, parent1Id);

            _sut.MoveTask(childId, parent2Id);
            var childTask = _sut.GetTaskById(childId);
            var newParentTask = _sut.GetTaskById(parent2Id);

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

            _sut.AddTask(openTask);
            _sut.AddTask(closedTask);
            _sut.AddTask(closedTask);

            var results = _sut.GetClosedTasks();

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

            _sut.AddTask(openTask);
            _sut.AddTask(openTask);
            _sut.AddTask(closedTask);

            var results = _sut.GetOpenTasks();

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
            var id = _sut.AddTask(task);

            _sut.RemoveTask(id);
            var retrievedTask = _sut.GetTaskById(id);

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

            var parentId = _sut.AddTask(task);
            Thread.Sleep(1000);
            var childId = _sut.AddSubTask(subTask, parentId);
            Thread.Sleep(1000);
            var retrievedTask = _sut.GetTaskById(parentId);
            Thread.Sleep(1000);
            var retrievedSubTask = _sut.GetTaskById(childId);

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
            var id = _sut.AddTask(task);

            Thread.Sleep(1000);
            var retrievedTask = _sut.GetTaskById(id);

            Assert.AreEqual(task, retrievedTask);
        }

        [Test]
        public void ShouldUpdateTask()
        {
            var task = new Task
            {
                Title = "Title"
            };

            var id = _sut.AddTask(task);
            task.Title = "Title2";
            _sut.UpdateTask(id, task);

            var retrievedTask = _sut.GetTaskById(id);

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
            _sut.AddTask(task);

            _sut.CloseLatestInterval(task.Id, DateTimeOffset.Now);

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
            _sut.AddTask(task);
            var endDateTime = DateTimeOffset.Now;

            _sut.CloseLatestInterval(task.Id, endDateTime);

            var retrievedTask = _sut.GetTaskById(task.Id);

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

            _sut.AddTask(task1);
            var id2 = _sut.AddTask(task2);

            _sut.MoveTaskTo(id2, TaskRepository.RootTaskId, 0);

            var rootTask = _sut.GetTaskById(TaskRepository.RootTaskId);

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

            var parentId = _sut.AddTask(parent);
            var idClosed1 = _sut.AddSubTask(closedTask1, parentId);
            _sut.AddSubTask(openTask, parentId);
            _sut.AddSubTask(closedTask2, parentId);

            _sut.DecreasePriority(idClosed1);
            var fetchedParentTask = _sut.GetTaskById(parentId);

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

            var parentId = _sut.AddTask(parentTask);
            _sut.AddSubTask(closedTask1, parentId);
            _sut.AddSubTask(openTask, parentId);
            var closed2Id = _sut.AddSubTask(closedTask2, parentId);

            _sut.IncreasePriority(closed2Id);
            var requestedParentTask = _sut.GetTaskById(parentId);

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

            var closed1Id = _sut.AddTask(closedTask1);
            _sut.AddTask(openTask);
            _sut.AddTask(closedTask2);

            _sut.DecreasePriority(closed1Id);
            var rootTask = _sut.GetTaskById(TaskRepository.RootTaskId);

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

            _sut.AddTask(closedTask1);
            _sut.AddTask(openTask);
            var closed2Id = _sut.AddTask(closedTask2);

            _sut.IncreasePriority(closed2Id);
            var rootTask = _sut.GetTaskById(TaskRepository.RootTaskId);

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

            var id = _sut.AddTask(openTask);

            Thread.Sleep(200);
            var externalRepo = new TaskRepository(_fileRepo);
            var retrievedTask = externalRepo.GetTaskById(id);
            retrievedTask.Title = "Title2";
            externalRepo.UpdateTask(retrievedTask.Id, retrievedTask);
            Thread.Sleep(200);
            var originalTask = _sut.GetTaskById(retrievedTask.Id);

            Assert.AreEqual(retrievedTask, originalTask);
        }
    }
}