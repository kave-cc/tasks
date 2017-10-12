﻿/*
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
using JetBrains.UI.ActionsRevised;
using JetBrains.UI.MenuGroups;
using TaskManagerPlugin.UserControls.ActiveTask;
using TaskManagerPlugin.UserControls.NavigationControl;

namespace TaskManagerPlugin.Menu
{
    [ActionGroup(Id, ActionGroupInsertStyles.Submenu, Id = 1934, Text="&TaskManager")]
    public class TaskManagerMenu : IAction, IInsertLast<VsMainMenuGroup>
    {
        public const string Id = "TaskManager.Menu";

        public TaskManagerMenu(MainNavigationWindowAction t,
            ActiveTaskWindowAction a){ }

    }
}