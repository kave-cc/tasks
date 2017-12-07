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

using JetBrains.Application;
using KaVE.Commons.Model;
using KaVE.Commons.Utils;

namespace KaVE.Tasks
{
    [ShellComponent]
    public class TasksVersionUtil
    {
        private readonly IKaVEVersionUtil _versionUtil = new KaVEVersionUtil();

        public Variant GetVariant()
        {
            return _versionUtil.GetAssemblyVariantByContainedType<TasksVersionUtil>();
        }

        public string GetInformalVersion()
        {
            return _versionUtil.GetAssemblyInformalVersionByContainedType<TasksVersionUtil>();
        }
    }
}