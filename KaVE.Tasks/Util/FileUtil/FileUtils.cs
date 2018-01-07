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

namespace KaVE.Tasks.Util.FileUtil
{
    public class FileUtils
    {
        private readonly string _fileUri;
        private bool _locked;
        private SemaphoreSlim _signal;

        public FileUtils(string fileUri)
        {
            _fileUri = fileUri;
        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            
            try
            {
                stream = file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
            }

            return false;
        }

        public bool WaitForFileUnlock(int maxMillis)
        {
            var cancelTokenSource = new CancellationTokenSource();
            var sleepCancelTokenSource = new CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;

            var readTask = Task.Run(() => {
                TryReadFile();
            }, cancelToken);

            var sleepTask = Task.Delay(maxMillis, sleepCancelTokenSource.Token);

            var res = Task.WaitAny(readTask, sleepTask);

            cancelTokenSource.Cancel();
            sleepCancelTokenSource.Cancel();
            
            return res == 0;
        }

        private void TryReadFile()
        {
            var fileInfo = new FileInfo(_fileUri);
            var counter = 0;
            while (IsFileLocked(fileInfo))
            {
                Console.WriteLine(counter++);
                Thread.Sleep(50);
            }
        }
    }
}