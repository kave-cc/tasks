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
    internal class FileChangeWatcherTest
    {
        private FileChangeWatcher _sut;
        private const string FileName = "test.txt";
        private string _fileUri;
        private string _dir;
        private FileChangedEventArgs _args;
        private SemaphoreSlim _signal;
        private const int Timeout = 200;
        private ManualResetEventSlim _resetEvent; 

        [SetUp]
        public void SetUp()
        {
            _resetEvent = new ManualResetEventSlim();

            _dir = Path.GetRandomFileName();
            Directory.CreateDirectory(_dir);

            _fileUri = Path.Combine(_dir, FileName);
            File.Create(_fileUri).Close();

            _sut = new FileChangeWatcher(_fileUri, Timeout);
            _sut.FileChanged += FileChanged;
            _signal = new SemaphoreSlim(0, 1);

            _args = null;
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
            Directory.Delete(_dir, true);
        }

        private void FileChanged(object sender, FileChangedEventArgs args)
        {
            _args = args;
            _resetEvent.Set();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void whenDirectoryDoesNotExist_throws()
        {
            _sut = new FileChangeWatcher(Path.GetRandomFileName());
        }

        [Test]
        public void whenOtherFileInSameDirectoryIsChanged_doesNotNotify()
        {
            var directory = Path.GetDirectoryName(_fileUri);
            var randomFile = Path.Combine(directory, Path.GetRandomFileName());

            File.WriteAllText(randomFile, @"test");

            _signal.Wait(Timeout * 2);

            Assert.Null(_args);
        }

        [Test]
        public void whenFileIsChangedAndLocked_lockWasFreedIsTrue()
        {
            File.WriteAllText(_fileUri, @"test");

            Thread.Sleep(Timeout * 2);
            _resetEvent.Wait(Timeout * 2);

            Assert.NotNull(_args);
            Assert.IsTrue(_args.LockWasFreed);
        }

        [Test]
        public void whenFileIsChangedAndLocked_lockWasFreedIsFalse()
        {
            File.WriteAllText(_fileUri, @"test");
            var stream = File.Open(_fileUri, FileMode.Open, FileAccess.Write, FileShare.None);

            _signal.Wait(Timeout * 2);

            Assert.NotNull(_args);
            Assert.IsFalse(_args.LockWasFreed);

            stream.Close();
        }

    }
}