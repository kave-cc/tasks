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

using System;
using System.Globalization;
using System.Windows.Data;

namespace KaVE.Tasks.UserControls.TaskDetail.Converter
{
    public class PrettyDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (DateTimeOffset.MinValue.Equals(value))
            {
                return null;
            }

            var dateTime = ((DateTimeOffset) value).Date;
            var today = DateTimeOffset.Now.Date;

            int difference = (dateTime - today).Days;
            switch (difference)
            {
                case 0:
                    return "today";
                case -1:
                    return "yesterday";
                case 1:
                    return "tomorrow";
                default:
                    return dateTime.ToString("M");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
