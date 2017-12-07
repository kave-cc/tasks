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
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification;
using JetBrains.Application;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Utils;
using KaVE.Tasks.UserControls;
using KaVE.VS.Commons;
using Timer = System.Timers.Timer;

namespace KaVE.Tasks.Events.Subscriber
{
    [ShellComponent]
    public class MessageBusSubscriber : IDisposable
    {
        private readonly Timer _timer;
        private readonly TaskViewModel _viewModel;
        private readonly TaskbarIcon _notifyIcon;
        private const double DefaultTimerInterval = 10*60*1000;
        private bool _timerElapsed = false;
        private DateTimeOffset _lastTimeStamp = DateTimeOffset.Now;

        public MessageBusSubscriber(IMessageBus messageBus, TaskViewModel viewModel, double milliSecs = DefaultTimerInterval)
        {
            messageBus.Subscribe<IDEEvent>(HandleMessage);
            _viewModel = viewModel;
            _viewModel.PropertyChanged += OnActiveTaskChanged;

            double interval = milliSecs > 0 ? milliSecs : DefaultTimerInterval;
            _timer = new Timer(interval);
            _timer.Elapsed += OnTimerElapsed;
            _notifyIcon = new TaskbarIcon { Visibility = Visibility.Hidden };
        }

        private void HandleMessage(IDEEvent obj)
        {
            var activity = obj as ActivityEvent;
            if (activity == null) return;

            if (_timerElapsed)
            {
                ShowAbsenceNotification();
            }
            _timerElapsed = false;
            RestartTimerIfTaskIsActive();
        }

        private void ShowAbsenceNotification()
        {
            Invoke.OnSTA(
                () => _notifyIcon.ShowCustomBalloon(new AbsenceNotification(CalculateMinutesSinceLastAction(), _viewModel.ActiveTask.Title,
                OnNotificationClosed), PopupAnimation.Slide, null));
        }

        private void OnNotificationClosed(bool trackInactiveTime)
        {
            var endTime = trackInactiveTime ? DateTimeOffset.Now : _lastTimeStamp;
            _viewModel.FinishActiveIntervalAt(endTime);

            _lastTimeStamp = DateTimeOffset.Now;
        }

        private void RestartTimerIfTaskIsActive()
        {
            if (_viewModel.ActiveTask != null)
            {
                _timer.Stop();
                _timer.Start();
            }
        }
        
        private void OnActiveTaskChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveTask")
            {
                _timer.Stop();
                RestartTimerIfTaskIsActive();
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _timerElapsed = true;
        }

        private int CalculateMinutesSinceLastAction()
        {
            if (_lastTimeStamp == DateTimeOffset.MinValue) return 0;

            return (DateTimeOffset.Now - _lastTimeStamp).Minutes;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _notifyIcon?.Dispose();
        }
    }
}
