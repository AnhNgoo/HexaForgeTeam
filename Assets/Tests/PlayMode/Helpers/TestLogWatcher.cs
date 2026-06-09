using System.Collections.Generic;
using UnityEngine;

namespace DuskBlade.Tests
{
    public class TestLogWatcher
    {
        private const int MaxStoredErrors = 3;
        private const int MaxErrorLength = 220;
        private readonly List<string> errors = new List<string>();
        private bool isWatching;

        public bool HasErrorOrException
        {
            get { return errors.Count > 0; }
        }

        public void Start()
        {
            if (isWatching)
            {
                return;
            }

            errors.Clear();
            Application.logMessageReceived += OnLogMessageReceived;
            isWatching = true;
        }

        public void Stop()
        {
            if (!isWatching)
            {
                return;
            }

            Application.logMessageReceived -= OnLogMessageReceived;
            isWatching = false;
        }

        public IReadOnlyList<string> GetErrors()
        {
            return errors.AsReadOnly();
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception)
            {
                return;
            }

            if (errors.Count >= MaxStoredErrors)
            {
                return;
            }

            string message = string.IsNullOrEmpty(condition) ? type.ToString() : condition;
            message = message.Replace("\r", " ").Replace("\n", " ").Trim();
            if (message.Length > MaxErrorLength)
            {
                message = message.Substring(0, MaxErrorLength) + "...";
            }

            errors.Add(type + ": " + message);
        }
    }
}
