﻿using BluConsole.Core;

namespace BluConsole.Test
{
    public static class Test4
    {
        public static void LogLevelFour(string message, BluLogType type = BluLogType.Normal)
        {
            TestManager.Log(message, type);
        }

        public static void LogLevelFive(string message, BluLogType type = BluLogType.Normal)
        {
            Test5.LogLevelFive(message, type);
        }

        public static void LogLevelSix(string message, BluLogType type = BluLogType.Normal)
        {
            Test5.LogLevelSix(message, type);
        }

        public static void Loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooog(string message, BluLogType type = BluLogType.Normal)
        {
            Test5.Loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooog(message, type);
        }
    }
}