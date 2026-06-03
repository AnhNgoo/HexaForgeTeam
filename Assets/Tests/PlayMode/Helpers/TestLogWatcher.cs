using System.Collections.Generic;
using UnityEngine;

namespace DuskBlade.Tests
{
    public class TestLogWatcher
    {
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

            if (string.IsNullOrEmpty(stackTrace))
            {
                errors.Add(condition);
            }
            else
            {
                errors.Add($"{condition}\n{stackTrace}");
            }
        }
    }
}
