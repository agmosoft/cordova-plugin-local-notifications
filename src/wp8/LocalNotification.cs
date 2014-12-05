/*
    Copyright 2013-2014 appPlant UG

    Licensed to the Apache Software Foundation (ASF) under one
    or more contributor license agreements.  See the NOTICE file
    distributed with this work for additional information
    regarding copyright ownership.  The ASF licenses this file
    to you under the Apache License, Version 2.0 (the
    "License"); you may not use this file except in compliance
    with the License.  You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing,
    software distributed under the License is distributed on an
    "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
    KIND, either express or implied.  See the License for the
    specific language governing permissions and limitations
    under the License.
*/

using System;
using System.Linq;

using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;
using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;
using WPCordovaClassLib.Cordova.JSON;

using com.agmosoft.Cordova.Plugin.LocalNotification;
using System.Collections.Generic;

namespace Cordova.Extension.Commands
{
    /// <summary>
    /// Implementes access to application live tiles
    /// http://msdn.microsoft.com/en-us/library/hh202948(v=VS.92).aspx
    /// </summary>
    public class LocalNotification : BaseCommand
    {
        /// <summary>
        /// Informs if the device is ready and the deviceready event has been fired
        /// </summary>
        private bool DeviceReady = false;

        /// <summary>
        /// Informs either the app is running in background or foreground
        /// </summary>
        private bool RunsInBackground = false;

        /// <summary>
        /// Sets application live tile
        /// </summary>
        public void addOriginal (string jsonArgs)
        {
            string[] args   = JsonHelper.Deserialize<string[]>(jsonArgs);
            Options options = JsonHelper.Deserialize<Options>(args[0]);
            // Application Tile is always the first Tile, even if it is not pinned to Start.
            ShellTile AppTile = ShellTile.ActiveTiles.First();

            if (AppTile != null)
            {
                // Set the properties to update for the Application Tile
                // Empty strings for the text values and URIs will result in the property being cleared.
                FlipTileData TileData = CreateTileData(options);

                AppTile.Update(TileData);

                FireEvent("trigger", options.ID, options.JSON);
                FireEvent("add", options.ID, options.JSON);
            }

            DispatchCommandResult();
        }

        /// <summary>
        /// Clears the application live tile
        /// </summary>
        public void cancelOriginal (string jsonArgs)
        {
            string[] args         = JsonHelper.Deserialize<string[]>(jsonArgs);
            string notificationID = args[0];

            cancelAll(jsonArgs);

            FireEvent("cancel", notificationID, "");
            DispatchCommandResult();
        }

        /// <summary>
        /// Clears the application live tile
        /// </summary>
        public void cancelAllOriginal (string jsonArgs)
        {
            // Application Tile is always the first Tile, even if it is not pinned to Start.
            ShellTile AppTile = ShellTile.ActiveTiles.First();

            if (AppTile != null)
            {
                // Set the properties to update for the Application Tile
                // Empty strings for the text values and URIs will result in the property being cleared.
                FlipTileData TileData = new FlipTileData
                {
                    Count                = 0,
                    BackTitle            = "",
                    BackContent          = "",
                    WideBackContent      = "",
                    SmallBackgroundImage = new Uri("appdata:Background.png"),
                    BackgroundImage      = new Uri("appdata:Background.png"),
                    WideBackgroundImage  = new Uri("/Assets/Tiles/FlipCycleTileLarge.png", UriKind.Relative),
                };

                // Update the Application Tile
                AppTile.Update(TileData);
            }

            DispatchCommandResult();
        }

        /// <summary>
        /// Checks wether a notification with an ID is scheduled
        /// </summary>
        public void isScheduled (string jsonArgs)
        {
            DispatchCommandResult();
        }

        /// <summary>
        /// Retrieves a list with all currently pending notifications
        /// </summary>
        public void getScheduledIds (string jsonArgs)
        {
            DispatchCommandResult();
        }

        /// <summary>
        /// Checks wether a notification with an ID was triggered
        /// </summary>
        public void isTriggered (string jsonArgs)
        {
            DispatchCommandResult();
        }

        /// <summary>
        /// Retrieves a list with all currently triggered notifications
        /// </summary>
        public void getTriggeredIds (string jsonArgs)
        {
            DispatchCommandResult();
        }

        /// <summary>
        /// Informs that the device is ready and the deviceready event has been fired
        /// </summary>
        public void deviceready (string jsonArgs)
        {
            DeviceReady = true;
        }

        /// <summary>
        /// Creates tile data
        /// </summary>
        private FlipTileData CreateTileData (Options options)
        {
            FlipTileData tile = new FlipTileData();

            // Badge sollte nur gelöscht werden, wenn expliziet eine `0` angegeben wurde
            if (options.Badge != 0)
            {
                tile.Count = options.Badge;
            }

            tile.BackTitle       = options.Title;
            tile.BackContent     = options.ShortMessage;
            tile.WideBackContent = options.Message;

            if (!String.IsNullOrEmpty(options.SmallImage))
            {
                tile.SmallBackgroundImage = new Uri(options.SmallImage, UriKind.RelativeOrAbsolute);
            }

            if (!String.IsNullOrEmpty(options.Image))
            {
                tile.BackgroundImage = new Uri(options.Image, UriKind.RelativeOrAbsolute);
            }

            if (!String.IsNullOrEmpty(options.WideImage))
            {
                tile.WideBackgroundImage = new Uri(options.WideImage, UriKind.RelativeOrAbsolute);
            }

            return tile;
        }

        /// <summary>
        /// Fires the given event.
        /// </summary>
        private void FireEvent (string Event, string Id, string JSON = "")
        {
            string state = ApplicationState();
            string args  = String.Format("\'{0}\',\'{1}\',\'{2}\'", Id, state, JSON);
            string js    = String.Format("window.plugin.notification.local.on{0}({1})", Event, args);

            PluginResult pluginResult = new PluginResult(PluginResult.Status.OK, js);

            pluginResult.KeepCallback = true;

            DispatchCommandResult(pluginResult);
        }

        /// <summary>
        /// Retrieves the application state
        /// Either "background" or "foreground"
        /// </summary>
        private String ApplicationState ()
        {
            return RunsInBackground ? "background" : "foreground";
        }

        /// <summary>
        /// Occurs when the application is being deactivated.
        /// </summary>
        public override void OnPause (object sender, DeactivatedEventArgs e)
        {
            RunsInBackground = true;
        }

        /// <summary>
        /// Occurs when the application is being made active after previously being put
        /// into a dormant state or tombstoned.
        /// </summary>
        public override void OnResume (object sender, Microsoft.Phone.Shell.ActivatedEventArgs e)
        {
            RunsInBackground = false;
        }

        public void add(string jsonArgs)
            {
                string[] args   = JsonHelper.Deserialize<string[]>(jsonArgs);
                Options options = JsonHelper.Deserialize<Options>(args[0]);


                
                // Create Alarm
                Alarm alarm = new Alarm(options.ID);
                alarm.Content = options.Message;
                alarm.Sound = new Uri(options.Sound, UriKind.Relative);
                // Sound is not implemented yet
                //alarm.Sound = new Uri("/Ringtones/Ring01.wma", UriKind.Relative);
                // Convert Timestamp to DateTime
                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                DateTime date = origin.AddSeconds(options.Date);
                date = GetLocalDateTime(date);
                alarm.BeginTime = date.AddMinutes(1);
                alarm.ExpirationTime = date.AddMinutes(5.0);
                
                if (options.Repeat == "daily")
                {
                    alarm.RecurrenceType = RecurrenceInterval.Daily;
                }
                else if (options.Repeat == "weekly")
                {
                    alarm.RecurrenceType = RecurrenceInterval.Weekly;
                }
                else if (options.Repeat == "monthly")
                {
                    alarm.RecurrenceType = RecurrenceInterval.Monthly;
                }
                else
                {
                    alarm.RecurrenceType = RecurrenceInterval.None;
                }
                // Add alarm, if not in the past
                if (alarm.BeginTime > DateTime.Now)
                {
                    // Remove old
                    ScheduledAction oldAlarm = ScheduledActionService.Find(options.ID);
                    if (oldAlarm != null)
                    {
                        ScheduledActionService.Remove(options.ID);
                    }

                    // ScheduledActionService is limit to 50 notifications
                    List<ScheduledNotification> notifications = ScheduledActionService.GetActions<ScheduledNotification>().ToList();
                    if (notifications.Count < 50)
                    {
                        ScheduledActionService.Add(alarm);
                        
                    }
                }

                FireEvent("trigger", options.ID, options.JSON);
                FireEvent("add", options.ID, options.JSON);

                DispatchCommandResult();
            }

        DateTime GetLocalDateTime(DateTime targetDateTime)
        {
            int fromTimezone = 0;

            int localTimezone;

            if (TimeZoneInfo.Local.BaseUtcOffset.Minutes != 0)
            {
                localTimezone = Convert.ToInt16(TimeZoneInfo.Local.BaseUtcOffset.Hours.ToString() + (TimeZoneInfo.Local.BaseUtcOffset.Minutes / 60).ToString());
            }
            else
            {
                localTimezone = TimeZoneInfo.Local.BaseUtcOffset.Hours;
            }
            DateTime Sdt = targetDateTime;
            DateTime UTCDateTime = targetDateTime.AddHours(-(fromTimezone));
            DateTime localDateTime = UTCDateTime.AddHours(+(localTimezone));
            return localDateTime;
        }
            /// <summary>
            /// Clears the application live tile
            /// </summary>
            public void cancel(string jsonArgs)
            {
                string[] args         = JsonHelper.Deserialize<string[]>(jsonArgs);
                string notificationID = args[0];

                ScheduledAction alert = ScheduledActionService.Find(notificationID);
                if (alert != null)
                {
                    ScheduledActionService.Remove(notificationID);
                }

                FireEvent("cancel", notificationID, "");
            }

            /// <summary>
            /// Clears the application live tile
            /// </summary>
            public void cancelAll(string jsonArgs)
            {
                List<ScheduledNotification> reminders = ScheduledActionService.GetActions<ScheduledNotification>().ToList();
                foreach (ScheduledNotification reminder in reminders)
                {
                    ScheduledActionService.Remove(reminder.Name);
                }

                DispatchCommandResult();
            }
    }
}
