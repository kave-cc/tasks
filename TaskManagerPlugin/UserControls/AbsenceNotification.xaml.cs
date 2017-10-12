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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskManagerPlugin.UserControls
{
    /// <summary>
    /// Interaction logic for AbsenceNotification.xaml
    /// </summary>
    public partial class AbsenceNotification : UserControl
    {
        public int MinutesSinceLastAction { get; }
        public Action<Boolean> CloseAction;
        public string TaskTitle { get; }

        public AbsenceNotification(int minutesSinceLastAction, string taskTitle, Action<Boolean> closeAction = null)
        {
            DataContext = this;
            MinutesSinceLastAction = minutesSinceLastAction;
            CloseAction = closeAction;
            TaskTitle = taskTitle;
            InitializeComponent();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void Close(bool trackInactiveTime)
        {
            this.Visibility = Visibility.Collapsed;
            CloseAction?.Invoke(trackInactiveTime);
        }
    }
}
