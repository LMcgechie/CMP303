using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ThreadManager : MonoBehaviour
{
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
    private static bool actionToExecuteOnMainThread = false;


    private void FixedUpdate()
    {
        UpdateMain();
    }


    public static void ExecuteOnMainThread(Action newAction)
    {
        if (newAction == null)
        {
            Debug.Log("No action to execute on main thread!");
            return;
        }

        lock (executeOnMainThread)
        {
            executeOnMainThread.Add(newAction);
            actionToExecuteOnMainThread = true;
        }
    }

    public static void UpdateMain()
    {
        if (actionToExecuteOnMainThread)
        {
            executeCopiedOnMainThread.Clear();
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
            {
                executeCopiedOnMainThread[i]();
            }
        }
    }
}
