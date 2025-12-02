using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class Log
{
    public static bool isopen = true;
    public static void log(object str)
    {
#if UNITY_EDITOR
        if (isopen)
        {
                Debug.Log(str);
        }
#endif
    }
    public static void logerror(object str)
    {
#if UNITY_EDITOR
        if (isopen)
        {
            Debug.LogError(str);
        }
#endif
    }
}

