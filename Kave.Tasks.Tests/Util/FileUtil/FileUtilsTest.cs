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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KaVE.Tasks.Util.FileUtil;
using NUnit.Framework;

namespace TaskManagerPlugin.Test.Util.FileUtil
{
    [TestFixture]
    internal class FileUtilsTest
    {
        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(TestDirectory);
            var fileName = _counter + FileBase;

            _counter++;
            _currentFile = Path.Combine(TestDirectory, fileName);
            _stream = File.Create(_currentFile);
        }

        [TearDown]
        public void TearDown()
        {
            _stream.Dispose();
            Directory.Delete(TestDirectory, true);
        }

        private FileStream _stream;
        private const string TestDirectory = "FileUtilsTest";
        private const string FileBase = "test.json";
        private int _counter;
        private string _currentFile;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
        }

        [Test]
        public void WhenFileIsLocked_ReturnsTrue()
        {
            var fileInfo = new FileInfo(_currentFile);

            var locked = FileUtils.IsFileLocked(fileInfo);

            Assert.IsTrue(locked);
        }

        [Test]
        public void WhenFileIsUnlocked_ReturnsFalse()
        {
            var fileInfo = new FileInfo(_currentFile);

            _stream.Close();
            var locked = FileUtils.IsFileLocked(fileInfo);

            Assert.IsFalse(locked);
        }

        [Test]
        public void WhenFileIsUnlocked_ReturnsTrue()
        {
            var utils = new FileUtils(_currentFile);

            var result = false;

            Parallel.Invoke(
                () => result = utils.WaitForFileUnlock(1000),
                () =>
                {
                    Thread.Sleep(150);
                    _stream.Close();
                });

            Assert.IsTrue(result);
        }

        [Test]
        public void WhenMaxDelayIsHit_ReturnsFalse()
        {
            var utils = new FileUtils(_currentFile);
            var result = utils.WaitForFileUnlock(10);

            Assert.IsFalse(result);
        }
    }
}