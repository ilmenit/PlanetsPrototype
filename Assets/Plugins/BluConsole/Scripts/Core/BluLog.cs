﻿using System;
using System.Collections.Generic;

namespace BluConsole.Core
{
    [Serializable]
    public class BluLog
    {
        public string Message { get; private set; }

        public string MessageLower { get; private set; }

        public string File { get; private set; }

        public int Line { get; private set; }

        public int Mode { get; private set; }

        public int InstanceID { get; private set; }

        public List<BluLogFrame> StackTrace { get; private set; }

        public BluLogType LogType { get; set; }

        public void SetMessage(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return;

            var index = 0;
            while (index < condition.Length && condition[index++] != '\n') ;
            Message = condition.Substring(0, index - 1);
            MessageLower = Message.ToLower();
        }

        public void SetFile(string file)
        {
            File = file;
        }

        public void SetLine(int line)
        {
            Line = line;
        }

        public void SetMode(int mode)
        {
            Mode = mode;
        }

        public void SetStackTrace(string condition)
        {
            StackTrace = new List<BluLogFrame>();

            if (string.IsNullOrEmpty(condition))
                return;

            var splits = condition.Split('\n');
            for (var i = 1; i < splits.Length; i++)
            {
                if (string.IsNullOrEmpty(splits[i]))
                    continue;
                StackTrace.Add(new BluLogFrame(splits[i]));
            }
        }

        public void SetInstanceID(int instanceID)
        {
            InstanceID = instanceID;
        }

        public void FilterStackTrace(List<string> prefixs)
        {
            var newStackTrace = new List<BluLogFrame>(StackTrace.Count);
            foreach (var frame in StackTrace)
            {
                var hasPrefix = false;
                foreach (var prefix in prefixs)
                {
                    if (frame.FrameInformation.StartsWith(prefix))
                    {
                        hasPrefix = true;
                        break;
                    }
                }
                if (!hasPrefix)
                    newStackTrace.Add(frame);
            }
            StackTrace = newStackTrace;
        }
    }
}