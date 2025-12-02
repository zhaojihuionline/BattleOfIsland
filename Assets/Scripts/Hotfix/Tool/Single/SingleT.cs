using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleT<T> where T : new()
{
    protected static T instance;
    public static T Ins
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }
            return instance;
        }
    }
}
