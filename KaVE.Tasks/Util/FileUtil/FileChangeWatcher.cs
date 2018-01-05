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

#pragma warning disable 4014

namespace KaVE.Tasks.Util.FileUtil
{
    public class FileChangeWatcher : IDisposable
    {
        public delegate void FileChangedHandler(object sender, FileChangedEventArgs args);

        private const int InitialDelay = 10;
        private const int MaxDelay = 1000;
        private readonly FileInfo _fileInfo;

        private readonly FileSystemWatcher _watcher;
        private DateTime _lastUpdate;

        public FileChangeWatcher(string fileUri)
        {
            _fileInfo = new FileInfo(fileUri);
            var directory = _fileInfo.DirectoryName;

            _watcher = new FileSystemWatcher(directory)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.json",
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
            if (e.FullPath != _fileInfo.FullName) return;
            CheckFileLock(InitialDelay);
        }

        private void CheckFileLock(int delay)
        {
            while (true)
            {
                FileUtils.Log("CheckFileLock");
                if (FileUtils.IsFileLocked(_fileInfo))
                {
                    if (delay > MaxDelay)
                    {
                        var args = new FileChangedEventArgs(_fileInfo, false);
                        OnFileChanged(args);
                        break;
                    }

                    FileUtils.Log("CheckFileLock delay: {0}", delay);
                    Thread.Sleep(delay);
                    delay = delay * 2;
                    continue;
                }
                var arg = new FileChangedEventArgs(_fileInfo, true);
                OnFileChanged(arg);
                break;
            }
        }

        protected virtual void OnFileChanged(FileChangedEventArgs args)
        {
            args.FileInfo.Refresh();
            if (args.FileInfo.LastWriteTimeUtc > _lastUpdate)
            {
                _lastUpdate = args.FileInfo.LastWriteTimeUtc;
                FileChanged?.Invoke(this, args);
            }
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