﻿using BluConsole.Core;

namespace BluConsole.Test
{
    public static class Test3
    {
        public static void LogLevelThree(string message, BluLogType type = BluLogType.Normal)
        {
            TestManager.Log(message, type);
        }

        public static void LogLevelFour(string message, BluLogType type = BluLogType.Normal)
        {
            Test4.LogLevelFour(message, type);
        }

        public static void LogLevelFive(string message, BluLogType type = BluLogType.Normal)
        {
            Test4.LogLevelFive(message, type);
        }

        public static void LogLevelSix(string message, BluLogType type = BluLogType.Normal)
        {
            Test4.LogLevelSix(message, type);
        }

        public static void Loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooog(string message, BluLogType type = BluLogType.Normal)
        {
            Test4.Loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooog(message, type);
        }
    }
}