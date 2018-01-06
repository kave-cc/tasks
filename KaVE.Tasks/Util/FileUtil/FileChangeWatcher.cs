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
using KaVE.Commons.Utils;

#pragma warning disable 4014

namespace KaVE.Tasks.Util.FileUtil
{
    public class FileChangeWatcher : IDisposable
    {
        public delegate void FileChangedHandler(object sender, FileChangedEventArgs args);

        private const int InitialDelay = 10;
        private readonly int _timeout;
        private readonly FileInfo _fileInfo;
        private readonly FileSystemWatcher _watcher;
        private DateTime _lastUpdate;

        public FileChangeWatcher(string fileUri, int timeout = 1000)
        {
            _timeout = timeout;

            _fileInfo = new FileInfo(fileUri);
            var directory = _fileInfo.DirectoryName;

            if (!File.Exists(fileUri))
                throw new ArgumentException("The file does not exist!");

            _watcher = new FileSystemWatcher(directory)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = _fileInfo.Name,
                IncludeSubdirectories = false
            };

            _watcher.Changed += WatchFileChanged;
            _watcher.Deleted += WatchFileDeleted;
            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }

        public event FileChangedHandler FileChanged;

        private void WatchFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == _fileInfo.FullName)
                Dispose();
        }

        private void WatchFileChanged(object sender, FileSystemEventArgs e)
        {
            CheckFileLock(InitialDelay);
        }

        private void CheckFileLock(int delay)
        {
            var fileWasUnlocked = false;
            int delaySum = 0;

            while (delaySum < _timeout)
            {
                FileUtils.Log("CheckFileLock");
                if (FileUtils.IsFileLocked(_fileInfo))
                {
                    FileUtils.Log("CheckFileLock delay: {0}", delay);
                    Thread.Sleep(delay);
                    delay = delay * 2;
                    delaySum += delay;
                }
                else
                {
                    fileWasUnlocked = true;
                    break;
                }
            }


            var args = new FileChangedEventArgs(_fileInfo, fileWasUnlocked);
            OnFileChanged(args);
        }

        protected virtual void OnFileChanged(FileChangedEventArgs args)
        {
            Console.WriteLine(args.ToStringReflection());

            args.FileInfo.Refresh();
            if (args.FileInfo.LastWriteTimeUtc <= _lastUpdate) return;

            _lastUpdate = args.FileInfo.LastWriteTimeUtc;
            FileChanged?.Invoke(this, args);
        }
    }

    public class FileChangedEventArgs
    {
        public FileInfo FileInfo;
        public bool LockWasFreed;

        public FileChangedEventArgs(FileInfo fileInfo, bool lockWasFreed)
        {
            FileInfo = fileInfo;
            LockWasFreed = lockWasFreed;
        }
    }
}