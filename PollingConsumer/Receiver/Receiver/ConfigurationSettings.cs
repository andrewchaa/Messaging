﻿using System;
using System.Configuration;

namespace Receiver
{
    public static class ConfigurationSettings
    {
        static ConfigurationSettings()
        {
            PollingInterval = Convert.ToInt32(ConfigurationManager.AppSettings["PollingInterval"]);
            PollingTimeout = Convert.ToInt64(ConfigurationManager.AppSettings["PollingTimeout"]);
        }

        public static int PollingInterval { get; set; }

        public static long PollingTimeout { get; set; }
    }
}
