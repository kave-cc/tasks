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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManagerPlugin.Model;

namespace TaskManagerPlugin.Test.Model
{
    [TestClass]
    public class TaskTest
    {
        [TestMethod]
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
    }
}
