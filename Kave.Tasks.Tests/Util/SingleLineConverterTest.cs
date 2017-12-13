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

namespace KaVE.Tasks.Test.Util
{
    [TestFixture]
    public class SingleLineConverterTest
    {
        private readonly SingleLineConverter _converter = new SingleLineConverter();

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void WhenConvertBackIsCalled_Throws()
        {
            const string test = "asdf";

            _converter.ConvertBack(test, null, null, null);
        }

        [Test]
        public void WhenNewLinesAreContained_ReplacesNewLinesWithSpaces()
        {
            const string testString = "This is a test\nwith multiple new\nline\ncharacters.";
            const string expected = "This is a test with multiple new line characters.";

            var actual = _converter.Convert(testString, null, null, null);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WhenNoNewLinesAreContained_DoesNotModifyString()
        {
            const string testString = "This is a test without a new line character!";

            var actual = _converter.Convert(testString, null, null, null);

            Assert.AreEqual(testString, actual);
        }
    }
}