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
    public bool KeyDownSpace { get; private set; }
    public bool KeyDownRightArrow { get; private set; }
    public bool KeyDownF3 { get; private set; }

    public float2 MousePosition { get; private set; }
    public float2 DeltaMousePosition { get; private set; }

    public float ScrollWheelDelta;

    public void Update()
    {
        LeftMouseButton = Input.GetMouseButton(0);
        RightMouseButton = Input.GetMouseButton(1);

        KeyDownR = Input.GetKeyDown(KeyCode.R);
        KeyDownSpace = Input.GetKeyDown(KeyCode.Space);
        KeyDownRightArrow = Input.GetKeyDown(KeyCode.RightArrow);
        KeyDownF3 = Input.GetKeyDown(KeyCode.F3);

        WorldMousePosition = (Vector2)MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, MainCamera.nearClipPlane));

        var prevMousePos = MousePosition;
        MousePosition = (Vector2)Input.mousePosition;
        DeltaMousePosition = MousePosition - prevMousePos;

        ScrollWheelDelta = Input.mouseScrollDelta.y;
    }
    
}