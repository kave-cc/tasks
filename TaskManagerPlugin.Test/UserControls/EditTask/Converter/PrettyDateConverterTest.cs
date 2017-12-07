﻿/*
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
using KaVE.Tasks.UserControls.TaskDetail.Converter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskManagerPlugin.Test.UserControls.EditTask.Converter
{
    [TestClass]
    public class PrettyDateConverterTest
    {
        private PrettyDateConverter _converter = new PrettyDateConverter();

        [TestMethod]
        public void WhenDateIsBeforeYesterday_ShouldPrintDateTime()
        {
            var date = DateTimeOffset.Now.AddDays(-2);
            var desiredOutput = date.ToString("M");

            string result = (string)_converter.Convert(date, null, null, null);

            Assert.AreEqual(desiredOutput, result);
        }

        [TestMethod]
        public void WhenDateIsYesterday_ShouldPrintYesterday()
        {
            var date = DateTimeOffset.Now.AddDays(-1);

            var result = (string)_converter.Convert(date, null, null, null);

            Assert.AreEqual("yesterday", result);
        }

        [TestMethod]
        public void WhenDateIsToday_ShouldPrintToday()
        {
            var date = DateTimeOffset.Now;

            var result = (string)_converter.Convert(date, null, null, null);

            Assert.AreEqual("today", result);
        }

        [TestMethod]
        public void WhenDateIsTomorrow_ShouldPrintTomorrow()
        {
            var date = DateTimeOffset.Now.AddDays(1);

            var result = (string)_converter.Convert(date, null, null, null);

            Assert.AreEqual("tomorrow", result);
        }

        [TestMethod]
        public void WhenDateIsAfterTomorrow_ShouldPrintDateTime()
        {
            var date = DateTimeOffset.Now.AddDays(1);
            var desiredResult = date.ToString("M");

            var result = (string)_converter.Convert(date, null, null, null);

            Assert.AreEqual("tomorrow", result);
        }

        [TestMethod]
        public void WhenDateIsMin_ShouldReturnNull()
        {
            var date = DateTimeOffset.MinValue;

            var result = _converter.Convert(date, null, null, null);

            Assert.IsNull(result);
        }
    }
}
