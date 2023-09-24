﻿using BluConsole.Core;
using BluConsole.Core.UnityLoggerApi;
using BluConsole.Extensions;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluConsole.Editor
{
    public enum Direction
    {
        None = 0,
        Up = 1,
        Down = 2
    }

    [Serializable]
    public abstract class BluConsoleWindow
    {
        public Rect WindowRect { get; set; }
        public List<string> StackTraceIgnorePrefixs { get; set; }
        public int[] Rows { get; set; }
        public BluLog[] Logs { get; set; }
        public int QtLogs { get; set; }
        public int SelectedMessage { get; set; }
        public double LastTimeClicked { get; set; }
        public bool HasKeyboardArrowKeyInput { get; private set; }
        public Direction KeyboardArrowKeyDirection { get; private set; }
        public BluLogConfiguration LogConfiguration { get; set; }

        protected Vector2 ScrollPosition;

        public bool IsRepaintEvent
        {
            get
            {
                return Event.current.type == EventType.Repaint;
            }
        }

        public bool IsDoubleClick
        {
            get
            {
                return (EditorApplication.timeSinceStartup - LastTimeClicked) < 0.3f && !HasKeyboardArrowKeyInput;
            }
        }

        public bool IsScrollUp
        {
            get
            {
                return Event.current.type == EventType.ScrollWheel && Event.current.delta.y < 0f;
            }
        }

        public virtual void OnGUI(int id)
        {
            HandleKeyboardArrowKeys();
            HandleKeyboardEnterKey();
        }

        public BluLog GetCompleteLog(int row)
        {
            var log = UnityLoggerServer.GetCompleteLog(row);
            log.FilterStackTrace(StackTraceIgnorePrefixs);
            return log;
        }

        protected float DefaultButtonWidth
        {
            get
            {
                return WindowRect.width;
            }
        }

        protected float DefaultButtonHeight
        {
            get
            {
                return BluConsoleSkin.MessageStyle.CalcSize("Test".GUIContent()).y + LogConfiguration.DefaultButtonHeightOffset;
            }
        }

        protected void DrawPopup(BluLog log)
        {
            DrawPopup(log.Message);
        }

        protected void DrawPopup(string message)
        {
            Event clickEvent = Event.current;

            GenericMenu.MenuFunction copyCallback = () => { EditorGUIUtility.systemCopyBuffer = message; };

            GenericMenu menu = new GenericMenu();
            menu.AddItem(content: "Copy".GUIContent(), on: false, func: copyCallback);
            menu.ShowAsContext();

            clickEvent.Use();
        }

        protected bool IsClicked(Rect rect)
        {
            return Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
        }

        protected void DrawBackground(Rect rect, GUIStyle style, bool isSelected)
        {
            if (IsRepaintEvent)
                style.Draw(rect, false, false, isSelected, false);
        }

        private string GetTruncatedMessage(string m, int length)
        {
            string message = m.Replace(System.Environment.NewLine, " ");
            if (message.Length <= length)
                return message;

            return string.Format("{0}... <truncated>", message.Substring(startIndex: 0, length: length));
        }

        protected string GetTruncatedListMessage(string m)
        {
            return GetTruncatedMessage(m, LogConfiguration.MaxLengthListWindowText);
        }

        protected string GetTruncatedDetailMessage(string m)
        {
            return GetTruncatedMessage(m, LogConfiguration.MaxLengthDetailWindowText);
        }

        protected string GetTruncatedDetailMessage(BluLog log)
        {
            return GetTruncatedDetailMessage(log.Message);
        }

        protected string GetTruncatedListMessage(BluLog log)
        {
            return GetTruncatedListMessage(log.Message);
        }

        private void HandleKeyboardArrowKeys()
        {
            Event e = Event.current;
            HasKeyboardArrowKeyInput = false;
            KeyboardArrowKeyDirection = Direction.None;

            if (e == null || e.type != EventType.KeyDown || !e.isKey)
                return;

            var refresh = false;
            switch (e.keyCode)
            {
                case KeyCode.UpArrow:
                    refresh = true;
                    KeyboardArrowKeyDirection = Direction.Up;
                    MoveLogPosition(Direction.Up);
                    break;

                case KeyCode.DownArrow:
                    refresh = true;
                    KeyboardArrowKeyDirection = Direction.Down;
                    MoveLogPosition(Direction.Down);
                    break;
            }

            if (refresh)
                HasKeyboardArrowKeyInput = true;
        }

        private void HandleKeyboardEnterKey()
        {
            Event e = Event.current;

            if (e == null || !e.isKey || e.type != EventType.KeyUp || e.keyCode != KeyCode.Return)
                return;

            OnEnterKeyPressed();
        }

        protected virtual void OnEnterKeyPressed()
        {
        }

        private void MoveLogPosition(Direction direction)
        {
            SelectedMessage += direction == Direction.Up ? -1 : +1;
            if (SelectedMessage < 0)
                SelectedMessage = 0;
        }
    }
}