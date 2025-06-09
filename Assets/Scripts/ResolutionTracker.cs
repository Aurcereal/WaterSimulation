using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class ResolutionTracker : MonoBehaviour
{
    public static ResolutionTracker Ins;
    void Awake()
    {
        Ins = this;
        size = int2(Screen.width, Screen.height);
    }

    int2 size;

    public delegate void ResolutionChangeDelegate(int2 newSize);
    public static ResolutionChangeDelegate ResolutionChangeEvent;

    public static int ScreenWidth => Ins.size.x;
    public static int ScreenHeight => Ins.size.y;

    void Update()
    {
        int2 newSize = int2(Screen.width, Screen.height);
        if (newSize.x != size.x || newSize.y != size.y)
        {
            size = newSize;
            ResolutionChangeEvent.Invoke(newSize);
        }
    }
}
