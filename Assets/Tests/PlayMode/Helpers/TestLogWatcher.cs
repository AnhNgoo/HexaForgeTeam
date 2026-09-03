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

            if (IsKnownSceneBootstrapMessage(condition))
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

        private bool IsKnownSceneBootstrapMessage(string condition)
        {
            if (string.IsNullOrEmpty(condition)) return false;
            return condition.IndexOf("Player Camera is missing from the setup", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                   condition.IndexOf("[SafeZone] Không spawn được pool", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                   condition.IndexOf("Không tìm thấy Player sau", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                   condition.IndexOf("There are no audio listeners in the scene", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
