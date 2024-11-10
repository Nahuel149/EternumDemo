using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;  // Add this import

public static class MainThread
{
    private static readonly Queue<Action> executionQueue = new Queue<Action>();
    private static readonly object queueLock = new object();

    public static async Task Execute(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var tcs = new TaskCompletionSource<bool>();

        lock (queueLock)
        {
            executionQueue.Enqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MainThread] Error executing action: {e.Message}\nStackTrace: {e.StackTrace}");
                    tcs.SetException(e);
                }
            });
        }

        await tcs.Task;
    }

    public static void Update()
    {
        lock (queueLock)
        {
            while (executionQueue.Count > 0)
            {
                try
                {
                    executionQueue.Dequeue()?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MainThread] Error in Update: {e.Message}\nStackTrace: {e.StackTrace}");
                }
            }
        }
    }

    public static bool IsMainThread()
    {
        return Thread.CurrentThread.ManagedThreadId == 1;  // Changed from UnityEngine.Threading to System.Threading
    }
}