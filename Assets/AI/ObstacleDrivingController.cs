using UnityEngine;

public class ObstacleDrivingController : MonoBehaviour
{
    public enum DrivingState
    {
        Cruise,
        SlowDown,
        Stop,
        Crawl,
        WaitClear,
        Restart
    }

    [Header("References")]
    public PrometeoCarController carController;
    public ObstacleDistanceEstimator distanceEstimator;

    [Header("Driving Input")]
    [Range(0f, 1f)]
    public float normalThrottle = 0.5f;

    [Range(0f, 1f)]
    public float slowThrottle = 0.25f;

    [Range(0f, 1f)]
    public float crawlThrottle = 0.1f;

    [Range(-1f, 1f)]
    public float steerInput = 0f;

    [Header("Distance Thresholds")]
    [Tooltip("이 거리보다 멀면 정상 주행")]
    public float cruiseDistance = 35f;

    [Tooltip("이 거리 아래부터 감속")]
    public float slowDownDistance = 30f;

    [Tooltip("이 거리 아래부터 매우 천천히 접근")]
    public float crawlDistance = 15f;

    [Tooltip("이 거리 아래면 정지")]
    public float stopDistance = 9f;

    [Header("Restart")]
    [Tooltip("장애물이 사라진 뒤 이 시간만큼 기다렸다가 재출발")]
    public float clearWaitTime = 15f;

    [Tooltip("재출발 시 서서히 가속되는 정도")]
    public float throttleSmoothSpeed = 2.5f;

    [Header("Debug")]
    public DrivingState currentState = DrivingState.Cruise;
    public float currentThrottle;
    public float targetThrottle;
    public float currentDistance;
    public bool obstacleDetected;
    public bool brake;

    private float clearTimer;

    private void Start()
    {
        currentThrottle = 0f;
        targetThrottle = normalThrottle;

        if (carController != null)
        {
            //carController.isAuto = true;
        }
    }

    private void Update()
    {
        if (carController == null || distanceEstimator == null)
        {
            return;
        }

        obstacleDetected = distanceEstimator.hasTarget;
        currentDistance = obstacleDetected ? distanceEstimator.nearestDistance : -1f;

        UpdateState();
        UpdateThrottle();
        ApplyInput();
    }

    private void UpdateState()
    {
        brake = false;

        if (!obstacleDetected)
        {
            if (currentState == DrivingState.Stop || currentState == DrivingState.WaitClear)
            {
                currentState = DrivingState.WaitClear;
                clearTimer += Time.deltaTime;

                carController.isAuto = true;

                if (clearTimer >= clearWaitTime)
                {
                    currentState = DrivingState.Restart;
                    clearTimer = 0f;
                }
            }
            else if (currentState == DrivingState.Restart)
            {
                currentState = DrivingState.Cruise;
                carController.isAuto = false;
            }
            else
            {
                currentState = DrivingState.Cruise;
                carController.isAuto = false;
            }

            return;
        }

        clearTimer = 0f;

        if (currentDistance <= stopDistance)
        {
            currentState = DrivingState.Stop;
            brake = true;
            carController.isAuto = true;

        }
        else if (currentDistance <= crawlDistance)
        {
            currentState = DrivingState.SlowDown;
        }
        else if (currentDistance <= slowDownDistance)
        {
            currentState = DrivingState.SlowDown;
        }
        else
        {
            currentState = DrivingState.Cruise;
        }
    }

    private void UpdateThrottle()
    {
        switch (currentState)
        {
            case DrivingState.Cruise:
                targetThrottle = normalThrottle;
                break;

            case DrivingState.SlowDown:
                targetThrottle = slowThrottle;
                break;

            case DrivingState.Crawl:
                targetThrottle = crawlThrottle;
                break;

            case DrivingState.Stop:
                targetThrottle = 0f;
                break;

            case DrivingState.WaitClear:
                targetThrottle = 0f;
                brake = true;
                break;

            case DrivingState.Restart:
                targetThrottle = normalThrottle;
                break;
        }

        if (brake)
        {
            currentThrottle = 0f;
        }
        else
        {
            currentThrottle = Mathf.Lerp(
                currentThrottle,
                targetThrottle,
                Time.deltaTime * throttleSmoothSpeed
            );
        }
    }

    private void ApplyInput()
    {
        carController.SetInput(currentThrottle, steerInput, brake);
    }
}