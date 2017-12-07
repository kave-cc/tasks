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
using JetBrains.Application.BuildScript.Application;
using JetBrains.UI.ToolWindowManagement;

namespace KaVE.Tasks.UserControls.ActiveTask
{
    [ToolWindowDescriptor(
            ProductNeutralId = "ActiveTaskWindow",
            Text = "Active Task",
            Type = ToolWindowType.SingleInstance,
            Icon = typeof(global::JetBrains.Ide.Resources.IdeThemedIcons.ExportHtml),
            VisibilityPersistenceScope = ToolWindowVisibilityPersistenceScope.Global,
            InitialDocking = ToolWindowInitialDocking.Left
        )
    ]
    public class ActiveTaskWindowDescriptor : ToolWindowDescriptor
    {
        public ActiveTaskWindowDescriptor(IApplicationHost host, IWindowBranding branding) : base(host, branding)
        {
        }

        public ActiveTaskWindowDescriptor(IApplicationHost host) : base(host)
        {
        }
    }
}
