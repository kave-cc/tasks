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

using KaVE.Commons.Model.Events.Enums;
using KaVE.Tasks.Util;
using NUnit.Framework;

namespace TaskManagerPlugin.Test.Util
{
    [TestFixture]
    public class LikertToStringConverterTest
    {
        private readonly LikertToStringConverter _converter = new LikertToStringConverter();

        [Test]
        public void IfNegative1_ReturnsLow()
        {
            var likert = Likert5Point.Negative1;

            var converted = _converter.Convert(likert, null, null, null);

            Assert.AreEqual("Low", converted);
        }

        [Test]
        public void IfNegative2_ReturnsVeryLow()
        {
            var likert = Likert5Point.Negative2;

            var converted = _converter.Convert(likert, null, null, null);

            Assert.AreEqual("Very Low", converted);
        }

        [Test]
        public void IfNeutral_ReturnsNormal()
        {
            var likert = Likert5Point.Neutral;

            var converted = _converter.Convert(likert, null, null, null);

            Assert.AreEqual("Normal", converted);
        }

        [Test]
        public void IfPositive1_ReturnsUnknown()
        {
            var likert = Likert5Point.Positive1;

            var converted = _converter.Convert(likert, null, null, null);

            Assert.AreEqual("High", converted);
        }

        [Test]
        public void IfPositive2_ReturnsUnknown()
        {
            var likert = Likert5Point.Positive2;

            var converted = _converter.Convert(likert, null, null, null);

            Assert.AreEqual("Very High", converted);
        }

        [Test]
        public void IfUnknown_ReturnsUnknown()
        {
            var likert = Likert5Point.Unknown;

            var converted = _converter.Convert(likert, null, null, null);

            Assert.AreEqual("Unknown", converted);
        }
    }
}