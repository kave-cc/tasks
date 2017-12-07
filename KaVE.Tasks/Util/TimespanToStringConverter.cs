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
using System.Windows.Data;

namespace KaVE.Tasks.Util
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timespan = (TimeSpan) value;
            var hours = timespan.Hours;
            var minutes = timespan.Minutes;
            var seconds = timespan.Seconds;

            var hourString = hours <= 0 ? "" : hours == 1 ? "1 hour" : hours + " hours";
            var minuteString = minutes <= 0 ? "" : minutes == 1 ? "1 minute" : minutes + " minutes";
            var secondString = seconds <= 0 || hours*60 + minutes >= 10 ? "" : seconds == 1 ? "1 second" : seconds + " seconds";
            var firstSeparator = hourString.Length > 0 && minuteString.Length > 0 ? ", " : "";
            var secondSeparator = (hourString.Length > 0 || minuteString.Length > 0) && secondString.Length > 0 ? ", " : "";

            return string.Format(hourString + firstSeparator + minuteString + secondSeparator + secondString);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}