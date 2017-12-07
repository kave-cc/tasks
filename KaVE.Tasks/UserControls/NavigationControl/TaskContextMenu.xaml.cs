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

using System.Windows;
using System.Windows.Controls;

namespace KaVE.Tasks.UserControls.NavigationControl
{
    /// <summary>
    /// Interaction logic for TaskContextMenu.xaml
    /// </summary>
    public partial class TaskContextMenu : ContextMenu, IMenuItemClicked
    {
        public TaskContextMenu(MenuContext context)
        {
            DataContext = context;
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dependencyObject = sender as DependencyObject;
            var name = dependencyObject.GetValue(FrameworkElement.NameProperty) as string;
            OnMenuItemClicked(name);
        }

        public event MenuItemClickedHandler MenuItemClicked;

        protected virtual void OnMenuItemClicked(string itemname)
        {
            MenuItemClicked?.Invoke(this, itemname);
        }
    }
}
