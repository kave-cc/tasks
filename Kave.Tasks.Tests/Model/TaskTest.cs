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
using KaVE.Commons.Model.Events.Enums;
using KaVE.Tasks.Model;
using NUnit.Framework;

namespace KaVE.Tasks.Test.Model
{
    [TestFixture]
    public class TaskTest
    {
        [Test]
        public void WhenAnnoyanceIsDifferent_EqualsIsFalse()
        {
            var task = new Task {Annoyance = Likert5Point.Neutral};
            var copy = (Task) task.Clone();

            copy.Annoyance = Likert5Point.Negative2;

            Assert.IsFalse(task.Equals(copy));
        }

        [Test]
        public void WhenAnnoyanceIsDifferent_HashCodeIsDifferent()
        {
            var task = new Task {Annoyance = Likert5Point.Neutral};
            var copy = (Task) task.Clone();

            copy.Annoyance = Likert5Point.Negative2;

            Assert.AreNotEqual(task.GetHashCode(), copy.GetHashCode());
        }

        [Test]
        public void WhenDescriptionIsDifferent_EqualsIsFalse()
        {
            var task = new Task {Description = "Descr"};
            var copy = (Task) task.Clone();

            copy.Description = "Descr2";

            Assert.IsFalse(task.Equals(copy));
        }

        [Test]
        public void WhenDescriptionIsDifferent_HashCodeIsDifferent()
        {
            var task = new Task {Description = "Descr"};
            var copy = (Task) task.Clone();

            copy.Description = "Descr2";

            Assert.AreNotEqual(task.GetHashCode(), copy.GetHashCode());
        }

        [Test]
        public void WhenIDIsDifferent_EqualsIsFalse()
        {
            var task = new Task();
            var copy = (Task) task.Clone();

            copy.Id = "Id";

            Assert.IsFalse(task.Equals(copy));
        }

        [Test]
        public void WhenIDIsDifferent_HashCodeIsDifferent()
        {
            var task = new Task();
            var copy = (Task) task.Clone();

            copy.Id = "Id";

            Assert.AreNotEqual(task.GetHashCode(), copy.GetHashCode());
        }

        [Test]
        public void WhenImportanceIsDifferent_EqualsIsFalse()
        {
            var task = new Task {Importance = Likert5Point.Neutral};
            var copy = (Task) task.Clone();

            copy.Importance = Likert5Point.Negative2;

            Assert.IsFalse(task.Equals(copy));
        }

        [Test]
        public void WhenImportanceIsDifferent_HashCodeIsDifferent()
        {
            var task = new Task {Importance = Likert5Point.Neutral};
            var copy = (Task) task.Clone();

            copy.Importance = Likert5Point.Negative2;

            Assert.AreNotEqual(task.GetHashCode(), copy.GetHashCode());
        }

        [Test]
        public void WhenIsActiveIsDifferent_EqualsIsFalse()
        {
            var task = new Task {IsActive = false};
            var copy = (Task) task.Clone();

            copy.IsActive = true;

            Assert.IsFalse(task.Equals(copy));
        }

        [Test]
        public void WhenIsActiveIsDifferent_HashCodeIsDifferent()
        {
            var task = new Task {IsActive = false};
            var copy = (Task) task.Clone();

            copy.IsActive = true;

            Assert.AreNotEqual(task.GetHashCode(), copy.GetHashCode());
        }

        [Test]
        public void WhenIsOpenIsDifferent_EqualsIsFalse()
        {
            var task = new Task {IsOpen = false};
            var copy = (Task) task.Clone();

            copy.IsOpen = true;

            Assert.IsFalse(task.Equals(copy));
        }

        [Test]
        public void WhenIsOpenIsDifferent_HashCodeIsDifferent()
        {
            var task = new Task {IsOpen = false};
            var copy = (Task) task.Clone();

            copy.IsOpen = true;

            Assert.AreNotEqual(task.GetHashCode(), copy.GetHashCode());
        }

        [Test]
        public void WhenTaskHasSubTasks_AddsDurationOfSubTasksToDuration()
        {
            var task = new Task();
            var subTask = new Task();

            var interval = new Interval {StartTime = DateTimeOffset.Now};
            interval.EndTime = interval.StartTime.AddMinutes(10);

            subTask.Intervals.Add(interval);
            task.Intervals.Add(interval);

            task.SubTasks.Add(subTask);

            var actual = task.TimeSpan;
            var expected = new TimeSpan(0, 20, 0);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WhenTasksAreEqual_EqualsIsTrue()
        {
            var task = new Task {Title = "Title"};
            var copy = (Task) task.Clone();

            Assert.AreEqual(task, copy);
        }

        [Test]
        public void WhenTasksAreEqual_HashCodeIsSame()
        {
            var task = new Task {Title = "Title"};
            var copy = (Task) task.Clone();

            Assert.AreEqual(task, copy);
        }

        [Test]
        public void WhenTitleIsDifferent_EqualsIsFalse()
        {
            var task = new Task {Title = "Title"};
            var copy = (Task) task.Clone();

            task.Title = "Title2";

            Assert.IsFalse(task.Equals(copy));
        }

        [Test]
        public void WhenTitleIsDifferent_HashCodeIsDifferent()
        {
            var task = new Task {Title = "Title"};
            var copy = (Task) task.Clone();

            task.Title = "Title2";

            Assert.AreNotEqual(task.GetHashCode(), copy.GetHashCode());
        }

        [Test]
        public void WhenUrgencyIsDifferent_EqualsIsFalse()
        {
            var task = new Task {Urgency = Likert5Point.Neutral};
            var copy = (Task) task.Clone();

            copy.Urgency = Likert5Point.Negative2;

            Assert.IsFalse(task.Equals(copy));
        }

        [Test]
        public void WhenUrgencyIsDifferent_HashCodeIsDifferent()
        {
            var task = new Task {Urgency = Likert5Point.Neutral};
            var copy = (Task) task.Clone();

            copy.Urgency = Likert5Point.Negative2;

            Assert.AreNotEqual(task.GetHashCode(), copy.GetHashCode());
        }
    }
}