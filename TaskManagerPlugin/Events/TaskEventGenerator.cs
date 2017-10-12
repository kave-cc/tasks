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
using JetBrains.Application;
using JetBrains.Threading;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.Tasks;
using KaVE.Commons.Utils;
using KaVE.VS.Commons;
using KaVE.VS.Commons.Generators;
using TaskManagerPlugin.Model;
using TaskManagerPlugin.UserControls;

namespace TaskManagerPlugin.Events
{
    [ShellComponent]
    public class TaskEventGenerator : EventGeneratorBase
    {
        private readonly TasksVersionUtil _versionUtil;

        private TaskEvent _currentEvent;
        

        public TaskEventGenerator(IRSEnv env, 
            IMessageBus messageBus, 
            IDateUtils dateUtils, 
            IThreading threading,
            TasksVersionUtil versionUtil) : base(env, messageBus, dateUtils, threading)
        {
            _versionUtil = versionUtil;
        }

        public void FireTaskEvent(object sender, TaskEventArgs e)
        { 
            _currentEvent = Create<TaskEvent>();
            _currentEvent.TriggeredBy = EventTrigger.Unknown;
            _currentEvent.TaskId = e.Task.Id;
            _currentEvent.Action = e.Action;
            _currentEvent.Version = _versionUtil.GetInformalVersion();

            if (e.Action == TaskAction.Create || e.Action == TaskAction.Edit)
            {
                _currentEvent.Annoyance = e.Task.Annoyance;
                _currentEvent.Importance = e.Task.Importance;
                _currentEvent.Urgency = e.Task.Urgency;
            }

            if (e.Action == TaskAction.Create || e.Action == TaskAction.Move)
            {
                _currentEvent.NewParentId = e.Task.Parent.Id;
            }

            Fire(_currentEvent);
            _currentEvent = null;
        }
    }
}
