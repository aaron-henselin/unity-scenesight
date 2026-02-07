using System;
using System.Collections.Concurrent;
using UnityEditor;
using UnityEngine;

namespace YourCompany.UnityCopilot.Editor
{
    [InitializeOnLoad]
    internal static class CopilotSdkDispatcher
    {
        private static readonly ConcurrentQueue<Action> Queue = new();

        static CopilotSdkDispatcher()
        {
            EditorApplication.update += Drain;
        }

        public static void Post(Action action)
        {
            if (action == null)
            {
                return;
            }

            Queue.Enqueue(action);
        }

        private static void Drain()
        {
            while (Queue.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}
