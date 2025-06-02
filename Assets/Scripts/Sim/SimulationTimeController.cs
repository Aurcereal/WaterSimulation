using UnityEngine;

public class SimulationTimeController
{

    bool paused;
    bool progressFrame;

    public void UpdateState()
    {
        if (GameManager.Ins.inputManager.KeyDownSpace)
            paused = !paused;
        progressFrame = GameManager.Ins.inputManager.KeyDownRightArrow;
    }

    public bool ShouldUpdate()
    {
        return !paused || progressFrame;
    }

    public float GetDeltaTime()
    {
        if (!paused) return Time.deltaTime;
        else if (progressFrame) return 1.0f / 60.0f;
        else return 0f;
    }

}