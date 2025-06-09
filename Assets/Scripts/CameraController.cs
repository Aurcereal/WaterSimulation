using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

using static SimulationParameters;

public class CameraController
{

    public float3 Position => Target + localPos;
    public float3 Target;

    public Quaternion Rotation => Quaternion.LookRotation(fo, up);

    float3 localPos;

    float3 ri;
    float3 up;
    float3 fo;

    bool enabled;

    public CameraController(float3 startPos, float3 startTarget)
    {
        Target = startTarget;
        localPos = startPos - Target;

        fo = -normalize(localPos);
        up = float3(0, 1, 0);
        ri = normalize(cross(fo, up));
        up = normalize(up - fo * dot(fo, up));

        enabled = true;
    }

    void RotateAboutVector(float angle, float3 axis)
    {
        localPos = (Vector3)(Matrix4x4.Rotate(Quaternion.AngleAxis(angle, axis)) * float4(localPos, 0.0f));
        ri = (Vector3)(Matrix4x4.Rotate(Quaternion.AngleAxis(angle, axis)) * float4(ri, 0.0f));
        up = (Vector3)(Matrix4x4.Rotate(Quaternion.AngleAxis(angle, axis)) * float4(up, 0.0f));
        fo = (Vector3)(Matrix4x4.Rotate(Quaternion.AngleAxis(angle, axis)) * float4(fo, 0.0f));
    }

    void RotateGlobalY(float amt)
    {
        RotateAboutVector(amt, float3(0.0f, 1.0f, 0.0f));
    }

    void RotateLocalX(float amt)
    {
        RotateAboutVector(amt, ri);
    }

    void PanAlongRight(float amt)
    {
        Target += ri * amt;
    }

    void PanAlongUp(float amt)
    {
        Target += up * amt;
    }

    void ZoomAlongForward(float amt)
    {
        localPos += fo * amt;
    }

    public void Update()
    {
        var im = GameManager.Ins.inputManager;

        if (im.KeyDownF3)
            enabled = !enabled;

        if (!enabled)
            return;

        // Zoom
        ZoomAlongForward(im.ScrollWheelDelta * CameraZoomSpeed);

        if (im.RightMouseButton)
        {
            // Pan
            float2 pan = CameraPanSpeed * im.DeltaMousePosition;
            PanAlongRight(pan.x);
            PanAlongUp(-pan.y);
        }
        else if (im.LeftMouseButton)
        {
            // Rotate
            float2 rot = CameraRotateSpeed * im.DeltaMousePosition;
            RotateGlobalY(rot.x);
            RotateLocalX(rot.y);
        }
    }
    
    public void SetGlobalUniformCameraData()
    {
        Shader.SetGlobalFloat("FovY", radians(MainCamera.fieldOfView));
        Shader.SetGlobalFloat("Aspect", MainCamera.aspect);

        Shader.SetGlobalVector("CamRi", MainCamera.transform.right);
        Shader.SetGlobalVector("CamUp", MainCamera.transform.up);
        Shader.SetGlobalVector("CamFo", MainCamera.transform.forward);

        Shader.SetGlobalVector("CamPos", MainCamera.transform.position);
    }
}