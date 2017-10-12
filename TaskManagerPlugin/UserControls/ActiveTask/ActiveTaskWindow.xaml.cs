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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TaskManagerPlugin.Model;

namespace TaskManagerPlugin.UserControls.ActiveTask
{
    /// <summary>
    /// Interaction logic for ActiveTaskWindow.xaml
    /// </summary>
    public partial class ActiveTaskWindow
    {
        private readonly TaskViewModel _viewModel;

        public ActiveTaskWindow(TaskViewModel viewModel)
        {
            this._viewModel = viewModel;
            DataContext = _viewModel;

            InitializeComponent();
        }

        private void CloseTask_Click(object sender, EventArgs e)
        {
            _viewModel.CompleteTask(_viewModel.ActiveTask);
        }

        private void PauseTask_Click(object sender, EventArgs e)
        {
            _viewModel.OpenTask(_viewModel.ActiveTask);
        }

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            var selectedTask = OpenTasksCombo.SelectedItem as Task;

            if (selectedTask != null)
            {
                _viewModel.ActivateTask(selectedTask);
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenTask(_viewModel.ActiveTask);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CompleteTask(_viewModel.ActiveTask);
        }
    }

    public class NullToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = parameter != null && (bool)parameter;
            return (value != null && !inverse ||
                    value == null && inverse) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StandardDateToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTimeOffset date = (DateTimeOffset) value;
            return date == DateTimeOffset.MinValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
