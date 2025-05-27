using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static SimulationParameters;

public class InputManager
{
    public float2 WorldMousePosition { get; private set; }
    public bool LeftMouseButton { get; private set; }
    public bool RightMouseButton { get; private set; }
    public bool KeyDownR { get; private set; }

    public void Update()
    {
        LeftMouseButton = Input.GetMouseButton(0);
        RightMouseButton = Input.GetMouseButton(1);

        KeyDownR = Input.GetKeyDown(KeyCode.R);

        WorldMousePosition = (Vector2)MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, MainCamera.nearClipPlane));
    }
    
}