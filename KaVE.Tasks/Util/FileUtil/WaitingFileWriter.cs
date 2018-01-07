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

namespace KaVE.Tasks.Util.FileUtil
{
    public class WaitingFileWriter
    {

        public static DateTime WriteAllText(string fileUri, string json, int maxDelay = 2000)
        {
            var utils = new FileUtils(fileUri);

            if (!utils.WaitForFileUnlock(maxDelay))
                throw new Exception("The file could not be written in time.");

            try
            {
                File.WriteAllText(fileUri, json);
                return new FileInfo(fileUri).LastWriteTimeUtc;
            }
            catch (IOException)
            {
            }
            return DateTime.MinValue;
        }
    }
}