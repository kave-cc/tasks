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
using KaVE.Tasks.Model;
using KaVE.Tasks.UserControls.TaskDetail;
using NUnit.Framework;

namespace KaVE.Tasks.Test.UserControls.TaskDetail
{
    [TestFixture, RequiresSTA]
    public class TaskDetailControlTest
    {
        [SetUp]
        public void SetUp()
        {
            _task = new Task {Title = "Title", Description = "Description"};
            var interval = new Interval
            {
                StartTime = DateTimeOffset.Now.AddMinutes(-10),
                EndTime = DateTimeOffset.Now
            };
            _task.Intervals.Add(interval);

            _control = new TaskDetailControl(_task);
        }

        private TaskDetailControl _control;
        private Task _task;

        [Test]
        public void ShouldDisplayDescription()
        {
            Assert.AreEqual(_task.Description, _control.Description.Text);
        }

        [Test]
        public void ShouldDisplayTimeSpan()
        {
            var actual = "10 minutes";
            Assert.AreEqual(actual, _control.ActiveTime.Text);
        }

        [Test]
        public void ShouldDisplayTitle()
        {
            Assert.AreEqual(_task.Title, _control.TaskTitle.Text);
        }
    }
}