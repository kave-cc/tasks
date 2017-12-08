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
using KaVE.Tasks.Util;
using NUnit.Framework;

namespace TaskManagerPlugin.Test.Util
{
    [TestFixture]
    public class TimespanToStringConverterTest
    {
        private readonly TimeSpanToStringConverter _converter = new TimeSpanToStringConverter();

        [Test]
        public void WhenTimeSpanIsLessThanAMinute_ShouldOnlyShowSeconds()
        {
            var timeSpan = new TimeSpan(0, 0, 0, 10);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("10 seconds", converted);
        }

        [Test]
        public void WhenTimeSpanIsLessThanTenMinutes_ShouldShowMinutesAndSeconds()
        {
            var timeSpan = new TimeSpan(0, 0, 9, 10);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("9 minutes, 10 seconds", converted);
        }

        [Test]
        public void WhenTimeSpanIsLessThanTenMinutesAndZeroSeconds_ShouldOnlyShowMinutes()
        {
            var timeSpan = new TimeSpan(0, 0, 10, 0);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("10 minutes", converted);
        }

        [Test]
        public void WhenTimeSpanIsMoreThanAnHour_ShouldShowHoursAndMinutes()
        {
            var timeSpan = new TimeSpan(0, 10, 10, 10);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("10 hours, 10 minutes", converted);
        }

        [Test]
        public void WhenTimeSpanIsMoreThanAnHourAndZeroMinutes_ShouldOnlyShowHours()
        {
            var timeSpan = new TimeSpan(0, 10, 0, 10);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("10 hours", converted);
        }

        [Test]
        public void WhenTimeSpanIsOneHour_ShouldOnlyShowOneHour()
        {
            var timeSpan = new TimeSpan(0, 1, 0, 0);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("1 hour", converted);
        }

        [Test]
        public void WhenTimeSpanIsOneMinute_ShouldOnlyShowOneMinute()
        {
            var timeSpan = new TimeSpan(0, 0, 1, 0);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("1 minute", converted);
        }

        [Test]
        public void WhenTimeSpanIsOneSecond_ShouldOnlyShowOneSecond()
        {
            var timeSpan = new TimeSpan(0, 0, 0, 1);
            var converted = _converter.Convert(timeSpan, null, null, null);

            Assert.AreEqual("1 second", converted);
        }
    }
}