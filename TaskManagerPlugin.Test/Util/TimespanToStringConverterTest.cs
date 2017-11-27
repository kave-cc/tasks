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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskManagerPlugin.Util;

namespace TaskManagerPlugin.Test.Util
{
    [TestClass]
    public class TimespanToStringConverterTest
    {
        private TimeSpanToStringConverter _converter = new TimeSpanToStringConverter();

        [TestMethod]
        public void WhenTimeSpanIsOneSecond_ShouldOnlyShowOneSecond()
        {
            var timeSpan = new TimeSpan(0, 0, 0, 1);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("1 second", converted);
        }

        [TestMethod]
        public void WhenTimeSpanIsLessThanAMinute_ShouldOnlyShowSeconds()
        {
            var timeSpan = new TimeSpan(0, 0, 0, 10);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("10 seconds", converted);
        }

        [TestMethod]
        public void WhenTimeSpanIsOneMinute_ShouldOnlyShowOneMinute()
        {
            var timeSpan = new TimeSpan(0, 0, 1, 0);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("1 minute", converted);
        }

        [TestMethod]
        public void WhenTimeSpanIsLessThanTenMinutesAndZeroSeconds_ShouldOnlyShowMinutes()
        {
            var timeSpan = new TimeSpan(0, 0, 10, 0);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("10 minutes", converted);
        }

        [TestMethod]
        public void WhenTimeSpanIsLessThanTenMinutes_ShouldShowMinutesAndSeconds()
        {
            var timeSpan = new TimeSpan(0, 0, 9, 10);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("9 minutes, 10 seconds", converted);
        }

        [TestMethod]
        public void WhenTimeSpanIsOneHour_ShouldOnlyShowOneHour()
        {
            var timeSpan = new TimeSpan(0, 1, 0, 0);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("1 hour", converted);
        }

        [TestMethod]
        public void WhenTimeSpanIsMoreThanAnHourAndZeroMinutes_ShouldOnlyShowHours()
        {
            var timeSpan = new TimeSpan(0, 10, 0, 10);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("10 hours", converted);
        }

        [TestMethod]
        public void WhenTimeSpanIsMoreThanAnHour_ShouldShowHoursAndMinutes()
        {
            var timeSpan = new TimeSpan(0, 10, 10, 10);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("10 hours, 10 minutes", converted);
        }
    }
}
