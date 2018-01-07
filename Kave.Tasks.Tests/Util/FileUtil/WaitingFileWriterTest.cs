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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KaVE.Tasks.Util.FileUtil;
using NUnit.Framework;

namespace KaVE.Tasks.Tests.Util.FileUtil
{
    [TestFixture]
    internal class WaitingFileWriterTest
    {
        private const string FileName = "test.json";
        private int _counter;
        private string _currentFile;
        private FileStream _stream;
        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_dir);
            _currentFile = Path.Combine(_dir, FileName);
        }

        [TearDown]
        public void TearDown()
        {
            _stream.Close();
            Directory.Delete(_dir, true);
        }

        [Test]
        public void WhenFileIsLocked_WritesAfterUnlock()
        {
            _stream = File.Create(_currentFile);
            var text = "SampleText";

            Parallel.Invoke(
                () =>
                {
                    Thread.Sleep(50);
                    _stream.Close();
                },
                () => WaitingFileWriter.WriteAllText(_currentFile, text, 3000));

            Assert.AreEqual(text, File.ReadAllText(_currentFile));
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void WhenMaxDelayIsHit_Throws()
        {
            _stream = File.Create(_currentFile);
            var text = "SampleText";

            WaitingFileWriter.WriteAllText(_currentFile, text, 1);
        }
    }
}