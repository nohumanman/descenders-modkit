using UnityEngine;

public class ErrorLogger : MonoBehaviour
{
    private void Start()
    {
        Application.logMessageReceived += HandleLogMessage;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLogMessage;
    }

    private void HandleLogMessage(string logMessage, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            Debug.LogError($"Error/Exception: {logMessage}\nStackTrace: {stackTrace}");
        }
    }
}
