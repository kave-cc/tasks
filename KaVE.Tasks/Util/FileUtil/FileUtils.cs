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

            Log("IsFileLocked (start)");
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
                Log("IsFileLocked (close)");
            }

            return false;
        }

        public static void Log(string msg, params object[] args)
        {
            var formattedMsg = args.Length > 0 ? string.Format(msg, args) : msg;
            var now = DateTime.Now;
            Console.WriteLine("{0}.{1}.{2} " + formattedMsg, now.Minute, now.Second, now.Millisecond);
        }

        public bool WaitForFileUnlock(int maxMillis)
        {
            var cancelTokenSource = new CancellationTokenSource();
            var sleepCancelTokenSource = new CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;

            var readTask = Task.Run(() => {
                Log("WaitForFileUnlock (in)");
                TryReadFile();
                Log("WaitForFileUnlock (try read done)");
            }, cancelToken);

            var sleepTask = Task.Delay(maxMillis, sleepCancelTokenSource.Token);

            var res = Task.WaitAny(readTask, sleepTask);
            Log("Go on");

            cancelTokenSource.Cancel();
            sleepCancelTokenSource.Cancel();

            Log("WaitForFileUnlock (out)" + res);
            return res == 0;
        }

        private void TryReadFile()
        {
            Log("TRFF");
            var fileInfo = new FileInfo(_fileUri);
            var counter = 0;
            while (IsFileLocked(fileInfo))
            {
                Log("LOCKED");
                Console.WriteLine(counter++);
                Thread.Sleep(50);
            }
            Log("UNLOCKED");
        }
    }
}