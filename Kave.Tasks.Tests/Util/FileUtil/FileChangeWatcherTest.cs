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

namespace KaVE.Tasks.Tests.Util.FileUtil
{
    [TestFixture]
    internal class FileChangeWatcherTest
    {
        [SetUp]
        public void SetUp()
        {
            _currentDirectory = Path.Combine(TestDirectory, _counter++.ToString());

            CreateTestFile(_currentDirectory);

            _args = null;
            _signal = new SemaphoreSlim(0, 1);

            _watcher = new FileChangeWatcher(_filePath);
            _watcher.FileChanged += (sender, args) =>
            {
                if (args.FileInfo.FullName == _currentFileInfo.FullName)
                {
                    _args = args;
                    _signal.Release();
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _watcher?.Dispose();
            Directory.Delete(_currentDirectory, true);
        }

        private FileChangeWatcher _watcher;
        private const string TestDirectory = "fcwtTest";
        private const string TestFile = "test.txt";
        private FileInfo _currentFileInfo;
        private int _counter;
        private string _filePath;
        private string _currentDirectory;
        private StreamWriter _writer;
        private FileChangedEventArgs _args;
        private SemaphoreSlim _signal;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Directory.CreateDirectory(TestDirectory);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _watcher?.Dispose();
            _writer?.Dispose();
            Directory.Delete(TestDirectory, true);
        }

        private void WriteDummyText(int delayInMS)
        {
            Task.Run(() =>
            {
                FileUtils.Log("WDT(start)");
                using (var s = File.OpenWrite(_filePath))
                {
                    Thread.Sleep(delayInMS);
                }
                FileUtils.Log("WDT(end)");
            });
        }

        private void CreateTestFile(string directory)
        {
            _filePath = Path.Combine(directory, TestFile);
            Directory.CreateDirectory(directory);
            File.Create(_filePath).Close();
        }

        [Test]
        public async void WhenFileHasChanged_FiresChangeEvent()
        {
            WriteDummyText(0);

            Thread.Sleep(100);

            Assert.NotNull(_args);
            Assert.IsTrue(_args.LockWasFreed);
            Assert.AreEqual(_currentFileInfo.FullName, _args.FileInfo.FullName);
        }

        [Test]
        public async void WhenFileHasChangedAndIsLocked_ReturnsFalseAfterSomeTime()
        {
            WriteDummyText(6000);
            Thread.Sleep(3000);
            FileUtils.Log("After sleep");
            Assert.NotNull(_args);
            Assert.IsFalse(_args.LockWasFreed);
            Assert.AreEqual(_currentFileInfo.FullName, _args.FileInfo.FullName);
            Thread.Sleep(3000);

        }
    }
}