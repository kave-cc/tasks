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

using KaVE.Tasks.Util;
using NUnit.Framework;

namespace KaVE.Tasks.Test.Util
{
    [TestFixture]
    public class MinutesToDurationConverterTest
    {
        [Test]
        public void ShouldConvertMinutesToHoursMinutes()
        {
            var converter = new MinutesToDurationConverter();
            var duration = 65;

            var result = (string) converter.Convert(duration, null, null, null);

            Assert.AreEqual("01:05", result);
        }
    }
}