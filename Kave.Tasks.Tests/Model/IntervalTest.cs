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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaVE.Tasks.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskManagerPlugin.Test.Model
{
    [TestClass]
    public class IntervalTest
    {

        [TestMethod]
        public void WhenIsActive_ShouldCalculateTimeSpanToNow()
        {
            var interval = new Interval
            {
                StartTime = DateTimeOffset.Now.AddMinutes(-10)
            };

            var timeSpan = interval.GetTimeSpan();
            var expected = new TimeSpan(0, 0, 10, 0);
            Assert.AreEqual(expected, timeSpan);
        }

        [TestMethod]
        public void WhenIsClosed_ShouldCalculateIntervalAsTimeSpan()
        {
            var interval = new Interval()
            {
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddMinutes(10)
            };

            var timeSpan = interval.GetTimeSpan();
            var expected = new TimeSpan(0, 0, 10, 0);
            Assert.AreEqual(expected.Minutes, timeSpan.Minutes);
            Assert.AreEqual(expected.Seconds, timeSpan.Seconds);
        }

        [TestMethod]
        public void WhenIsNotActiveAndNotClosed_ShouldReturnZero()
        {
            var interval = new Interval();

            var actual = interval.GetTimeSpan();
            var expected = new TimeSpan(0);

            Assert.AreEqual(expected, actual);
        }
    }
}
