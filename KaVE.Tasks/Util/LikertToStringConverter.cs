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
using KaVE.Commons.Model.Events.Enums;

namespace KaVE.Tasks.Util
{
    public class LikertToStringConverter : IValueConverter
    {
        private static readonly string[] Strings = new[] { "Unknown", "Very Low", "Low", "Normal", "High", "Very High" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var likert = value as Likert5Point? ?? Likert5Point.Unknown;

            var index = (int) likert;

            return Strings[index];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
