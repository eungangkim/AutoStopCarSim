using TMPro;
using UnityEngine;

public class DrivingDebugUI : MonoBehaviour
{
    [Header("References")]
    public PrometeoCarController carController;
    public ObstacleDrivingController obstacleDrivingController;
    public ObstacleDistanceEstimator distanceEstimator;

    [Header("UI Texts")]
    public TMP_Text inputStatusText;
    public TMP_Text obstacleStatusText;

    private void Update()
    {
        UpdateInputUI();
        UpdateObstacleUI();
    }

    private void UpdateInputUI()
    {
        if (inputStatusText == null)
        {
            return;
        }

        bool w = Input.GetKey(KeyCode.W);
        bool a = Input.GetKey(KeyCode.A);
        bool s = Input.GetKey(KeyCode.S);
        bool d = Input.GetKey(KeyCode.D);
        bool space = Input.GetKey(KeyCode.Space);

        string controlMode = "Keyboard";

        if (carController != null && carController.useAIControl)
        {
            controlMode = "AI";
        }

        inputStatusText.text =
            $"[Input]\n" +
            $"Mode: {controlMode}\n" +
            $"W: {(w ? "ON" : "OFF")}\n" +
            $"A: {(a ? "ON" : "OFF")}\n" +
            $"S: {(s ? "ON" : "OFF")}\n" +
            $"D: {(d ? "ON" : "OFF")}\n" +
            $"Space: {(space ? "ON" : "OFF")}";
    }

    private void UpdateObstacleUI()
    {
        if (obstacleStatusText == null)
        {
            return;
        }

        bool detected = false;
        float distance = -1f;

        if (distanceEstimator != null)
        {
            detected = distanceEstimator.hasTarget;
            distance = distanceEstimator.nearestDistance;
        }

        string drivingState = "Unknown";

        if (obstacleDrivingController != null)
        {
            drivingState = obstacleDrivingController.currentState.ToString();
        }

        string detectedLine = detected
    ? "<color=green>Detected: YES</color>"
    : "<color=red>Detected: NO</color>";

        obstacleStatusText.text =
            $"[Obstacle]\n" +
            $"{detectedLine}\n" +
            $"Distance: {(detected ? distance.ToString("F2") + " m" : "-")}\n" +
            $"State: {drivingState}";
    }
}