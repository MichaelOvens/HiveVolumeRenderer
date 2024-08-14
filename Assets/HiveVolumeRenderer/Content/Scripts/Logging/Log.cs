using UnityEngine;

namespace HiveVolumeRenderer
{
    public static class Log
    {
        public static void Message(string message)
        {
            Debug.Log(message);
        }

        public static void Error(string message)
        {
            Debug.LogError(message);
        }
    }
}