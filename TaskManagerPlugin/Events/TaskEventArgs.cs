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
using System.Diagnostics;
using KaVE.Commons.Model.Events.Tasks;
using KaVE.JetBrains.Annotations;
using KaVE.Tasks.Model;

namespace KaVE.Tasks.Events
{
    public class TaskEventArgs : EventArgs
    {
        public Task Task { get; }
        public string Version { get; }
        public TaskAction Action { get; }

        public TaskEventArgs([NotNull]Task task, [NotNull]TaskAction action)
        {
            Task = task;
            Action = action;

            Version = GetAssemblyVersion();
        }

        private static string GetAssemblyVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }
    }
}
