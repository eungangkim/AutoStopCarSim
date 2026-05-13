using UnityEngine;

public class LaneFollower : MonoBehaviour
{
    public CameraCaptureSender laneSensor;
    public CarController carController;

    public float kp = 0.005f;
    public float maxSteer = 1f;
    public float throttle = 0.5f;

    void Update()
    {
        float steer = 0f;

        if (laneSensor != null && laneSensor.laneValid)
        {
            steer = Mathf.Clamp(laneSensor.latestError * kp, -maxSteer, maxSteer);
        }
        Debug.Log($"valid: {laneSensor.laneValid}, error: {laneSensor.latestError}");


        carController.SetInput(throttle, steer);
    }
}