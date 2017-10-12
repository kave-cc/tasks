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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskManagerPlugin.Model;
using TaskManagerPlugin.Repository;

namespace TaskManagerPlugin.Test.Repository
{
    [TestClass]
    public class TaskRepositoryTests
    {
        private const string FileUri = "test.json";

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(FileUri);
        }

        [TestMethod]
        public void ShouldCreateFile()
        {
            var repository = new TaskRepository(FileUri);
            repository.AddTask(new Task());
            Assert.IsTrue(File.Exists(FileUri));
        }

        [TestMethod]
        public void ShouldStoreTask()
        {
            var repository = new TaskRepository(FileUri);
            var task = new Task()
            {
                Title = "Title"
            };
            task.Title = "Title";
            task.Description = "Description";
            string id = repository.AddTask(task);

            var retrievedTask = repository.GetTaskById(id);

            Assert.AreEqual(task, retrievedTask);
        }

        [TestMethod]
        public void ShouldRemoveTask()
        {
            var repository = new TaskRepository(FileUri);
            var task = new Task()
            {
                Title = "Title"
            };
            string id = repository.AddTask(task);

            repository.RemoveTask(id);
            var retrievedTask = repository.GetTaskById(id);

            Assert.IsNull(retrievedTask);
        }

        [TestMethod]
        public void ShouldStoreSubTask()
        {
            var repository = new TaskRepository(FileUri);
            var task = new Task()
            {
                Title = "Title"
            };
            var subTask = new Task()
            {
                Title = "Title"
            };

            string parentId = repository.AddTask(task);
            string childId = repository.AddSubTask(subTask, parentId);
            Task retrievedTask = repository.GetTaskById(parentId);
            Task retrievedSubTask = repository.GetTaskById(childId);

            Assert.IsTrue(retrievedTask.SubTasks.Contains(subTask));
            Assert.AreEqual(task, retrievedTask);
            Assert.AreEqual(subTask, retrievedSubTask);
        }

        [TestMethod]
        public void ShouldMoveIntervals()
        {
            var repository = new TaskRepository(FileUri);
            var task = new Task()
            {
                Title = "Title"
            };
            var subTask = new Task()
            {
                Title = "Title"
            };
            var interval = new Interval()
            {
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now
            };
            subTask.Intervals.Add(interval);

            string parentId = repository.AddTask(task);
            string childId = repository.AddSubTask(subTask, parentId);
            repository.RemoveTask(childId);

            Task retrievedTask = repository.GetTaskById(parentId);

            Assert.IsTrue(retrievedTask.Intervals.Contains(interval));
        }

        [TestMethod]
        public void ShouldMoveSubTask()
        {
            var repository = new TaskRepository(FileUri);
            var parent1 = new Task()
            {
                Title = "Title"
            };
            var parent2 = new Task()
            {
                Title = "Title"
            };
            var child = new Task()
            {
                Title = "Title"
            };

            string parent1Id = repository.AddTask(parent1);
            string parent2Id = repository.AddTask(parent2);
            string childId = repository.AddSubTask(child, parent1Id);

            repository.MoveTask(childId, parent2Id);
            var childTask = repository.GetTaskById(childId);
            var newParentTask = repository.GetTaskById(parent2Id);

            Assert.IsTrue(newParentTask.SubTasks.Contains(childTask));
        }

        [TestMethod]
        public void ShouldUpdateTask()
        {
            var repository = new TaskRepository(FileUri);
            var task = new Task()
            {
                Title = "Title"
            };

            string id = repository.AddTask(task);
            task.Title = "Title";
            repository.UpdateTask(id, task);

            var retrievedTask = repository.GetTaskById(id);

            Assert.AreEqual("Title", retrievedTask.Title);
        }
         
        [TestMethod]
        public void ShouldOnlyReturnOpenTasks()
        {
            var repository = new TaskRepository(FileUri);
            var openTask = new Task()
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask = new Task()
            {
                Title = "Title"
            };
            closedTask.Close();

            repository.AddTask(openTask);
            repository.AddTask(openTask);
            repository.AddTask(closedTask);

            var results = repository.GetOpenTasks();

            Assert.AreEqual(2, results.Count);
            foreach (Task task in results)
            {
                Assert.IsTrue(task.IsOpen);
            }
        }

        [TestMethod]
        public void ShouldOnlyReturnClosedTasks()
        {
            var repository = new TaskRepository(FileUri);
            var openTask = new Task()
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask = new Task()
            {
                Title = "Title"
            };
            closedTask.Close();

            repository.AddTask(openTask);
            repository.AddTask(closedTask);
            repository.AddTask(closedTask);

            var results = repository.GetClosedTasks();

            Assert.AreEqual(2, results.Count);
            foreach (Task task in results)
            {
                Assert.IsFalse(task.IsOpen);
            }
        }

        [TestMethod]
        public void WhenParentIsRoot_DecreaseShouldSkipOtherStatus()
        {
            var repository = new TaskRepository(FileUri);
            var openTask = new Task()
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask1 = new Task()
            {
                Title = "Title"
            };
            closedTask1.Close();

            var closedTask2 = new Task()
            {
                Title = "Title"
            };
            closedTask2.Close();

            repository.AddTask(closedTask1);
            repository.AddTask(openTask);
            repository.AddTask(closedTask2);

            repository.DecreasePriority(closedTask1);
            var rootTask = repository.GetTaskById(TaskRepository.RootTaskId);

            Assert.AreEqual(3, rootTask.SubTasks.Count);
            Assert.AreEqual(openTask, rootTask.SubTasks[0]);
            Assert.AreEqual(closedTask2, rootTask.SubTasks[1]);
            Assert.AreEqual(closedTask1, rootTask.SubTasks[2]);
        }

        [TestMethod]
        public void WhenParentIsNotRoot_DecreaseShouldNotSkipOtherStatus()
        {
            var repository = new TaskRepository(FileUri);
            var openTask = new Task()
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask1 = new Task()
            {
                Title = "Title"
            };
            closedTask1.Close();

            var closedTask2 = new Task()
            {
                Title = "Title"
            };
            closedTask2.Close();

            var parent = new Task()
            {
                Title = "Title"
            };

            string parentId = repository.AddTask(parent);
            repository.AddSubTask(closedTask1, parentId);
            repository.AddSubTask(openTask, parentId);
            repository.AddSubTask(closedTask2, parentId);

            repository.DecreasePriority(closedTask1);
            var fetchedParentTask = repository.GetTaskById(parentId);

            Assert.AreEqual(3, fetchedParentTask.SubTasks.Count);
            Assert.AreEqual(openTask, fetchedParentTask.SubTasks[0]);
            Assert.AreEqual(closedTask1, fetchedParentTask.SubTasks[1]);
            Assert.AreEqual(closedTask2, fetchedParentTask.SubTasks[2]);
        }

        [TestMethod]
        public void WhenParentIsRoot_IncreaseShouldSkipOtherStatus()
        {
            var repository = new TaskRepository(FileUri);
            var openTask = new Task()
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask1 = new Task()
            {
                Title = "Title"
            };
            closedTask1.Close();

            var closedTask2 = new Task()
            {
                Title = "Title"
            };
            closedTask2.Close();

            repository.AddTask(closedTask1);
            repository.AddTask(openTask);
            repository.AddTask(closedTask2);

            repository.IncreasePriority(closedTask2);
            var rootTask = repository.GetTaskById(TaskRepository.RootTaskId);

            Assert.AreEqual(3, rootTask.SubTasks.Count);
            Assert.AreEqual(closedTask2, rootTask.SubTasks[0]);
            Assert.AreEqual(closedTask1, rootTask.SubTasks[1]);
            Assert.AreEqual(openTask, rootTask.SubTasks[2]);
        }

        [TestMethod]
        public void WhenParentIsNotRoot_IncreaseShouldNotSkipOtherStatus()
        {
            var repository = new TaskRepository(FileUri);
            var openTask = new Task()
            {
                Title = "Title"
            };
            openTask.Open();

            var closedTask1 = new Task()
            {
                Title = "Title"
            };
            closedTask1.Close();

            var closedTask2 = new Task()
            {
                Title = "Title"
            };
            closedTask2.Close();

            var parentTask = new Task()
            {
                Title = "Title"
            };

            string parentId = repository.AddTask(parentTask);
            repository.AddSubTask(closedTask1, parentId);
            repository.AddSubTask(openTask, parentId);
            repository.AddSubTask(closedTask2, parentId);

            repository.IncreasePriority(closedTask2);
            var requestedParentTask = repository.GetTaskById(parentId);

            Assert.AreEqual(3, requestedParentTask.SubTasks.Count);
            Assert.AreEqual(closedTask1, requestedParentTask.SubTasks[0]);
            Assert.AreEqual(closedTask2, requestedParentTask.SubTasks[1]);
            Assert.AreEqual(openTask, requestedParentTask.SubTasks[2]);
        }

        [TestMethod]
        public void WhenMoveTaskTo_InsertsAtCorrectPosition()
        {
            var repository = new TaskRepository(FileUri);
            var task1 = new Task()
            {
                Title = "Title"
            };
            var task2 = new Task()
            {
                Title = "Title"
            };

            repository.AddTask(task1);
            var id2 = repository.AddTask(task2);

            repository.MoveTaskTo(id2, TaskRepository.RootTaskId, 0);

            var rootTask = repository.GetTaskById(TaskRepository.RootTaskId);

            Assert.AreEqual(2, rootTask.SubTasks.Count);
            Assert.AreEqual(task2, rootTask.SubTasks[0]);
            Assert.AreEqual(task1, rootTask.SubTasks[1]);
        }

        [TestMethod]
        public void WhenIntervalIsClosedAndTaskNotActive_DoesNothing()
        {
            var repository = new TaskRepository(FileUri);
            var task = new Task()
            {
                Title = "Title"
            };

            var interval = new Interval()
            {
                StartTime = DateTimeOffset.MinValue.AddMinutes(1),
                EndTime = DateTimeOffset.MaxValue
            };

            task.Intervals.Add(interval);

            repository.CloseLatestInterval(task, DateTimeOffset.Now);

            Assert.AreEqual(1, task.Intervals.Count);
            Assert.AreEqual(interval, task.Intervals[0]);
        }

        [TestMethod]
        public void WhenIntervalIsOpenAndTaskActive_ClosesIntervalAndAddsNewInterval()
        {
            var repository = new TaskRepository(FileUri);
            var task = new Task()
            {
                Title = "Title",
                IsActive = true
            };

            var interval = new Interval()
            {
                StartTime = DateTimeOffset.MinValue.AddMinutes(1)
            };

            task.Intervals.Add(interval);
            var endDateTime = DateTimeOffset.Now;

            repository.CloseLatestInterval(task, endDateTime);

            Assert.AreEqual(2, task.Intervals.Count);
            Assert.AreEqual(endDateTime, task.Intervals[0].EndTime);
            Assert.IsTrue(task.Intervals[1].IsActive);
        }
    }
}
 