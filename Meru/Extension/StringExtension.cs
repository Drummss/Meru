﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace IA.Extension
{
    public static class StringExtension
    {
        public static TimeSpan GetTimeFromString(this string value)
        {
            List<string> arguments = value.Split(' ').ToList();
            int splitIndex = 0;

            for (int i = 0; i < arguments.Count; i++)
            {
                if (arguments[i].ToLower() == "in")
                {
                    splitIndex = i;
                }
            }

            TimeSpan timeUntilReminder = new TimeSpan();

            List<string> timeList = new List<string>();
            timeList.AddRange(arguments);
            timeList.RemoveRange(0, splitIndex);

            for (int i = 1; i < timeList.Count; i++)
            {
                switch (timeList[i])
                {
                    case "seconds":
                    case "second":
                        int seconds = int.Parse(timeList[i - 1]);
                        timeUntilReminder = timeUntilReminder.Add(new TimeSpan(0, 0, seconds));
                        break;

                    case "minutes":
                    case "minute":
                        int minutes = int.Parse(timeList[i - 1]);
                        timeUntilReminder = timeUntilReminder.Add(new TimeSpan(0, minutes, 0));
                        break;

                    case "hours":
                    case "hour":
                        int hours = int.Parse(timeList[i - 1]);
                        timeUntilReminder = timeUntilReminder.Add(new TimeSpan(hours, 0, 0));
                        break;

                    case "days":
                    case "day":
                        int days = int.Parse(timeList[i - 1]);
                        timeUntilReminder = timeUntilReminder.Add(new TimeSpan(days, 0, 0, 0));
                        break;

                    case "week":
                    case "weeks":
                        int weeks = int.Parse(timeList[i - 1]);
                        timeUntilReminder = timeUntilReminder.Add(new TimeSpan(weeks * 7, 0, 0, 0, 0));
                        break;

                    case "year":
                    case "years":
                        int years = int.Parse(timeList[i - 1]);
                        timeUntilReminder = timeUntilReminder.Add(new TimeSpan(years * 356, 0, 0, 0));
                        break;
                }
            }

            if (timeUntilReminder >= TimeSpan.MaxValue)
            {
                return new TimeSpan();
            }

            return timeUntilReminder;
        }
    }
}