using System.Collections.Generic;
using UnityEngine;

public class ObstacleDistanceEstimator : MonoBehaviour
{
    [Header("References")]
    public VisionCapture visionCapture;
    public Camera aiCamera;

    [Header("Target Class")]
    [Tooltip("보행자 또는 장애물 클래스 ID")]
    public int targetClassId = 0;

    [Range(0f, 1f)]
    public float minConfidence = 0.5f;

    [Header("Image Size")]
    [Tooltip("AI 입력 이미지 가로 크기. 예: 640x640이면 640")]
    public float imageWidth = 1920f;

    [Tooltip("AI 입력 이미지 세로 크기. 예: 640x640이면 640")]
    public float imageHeight = 1080f;

    [Header("Bounding Box Coordinate Mode")]
    [Tooltip("true면 detection.x, detection.y를 박스 중심 좌표로 봄. false면 좌상단 좌표로 봄.")]
    public bool bboxXYIsCenter = false;
    [Header("Box Height Distance Estimation")]
    public bool useBoxHeightDistance = true;

    [Tooltip("bbox 높이 기반 거리 스케일")]
    public float boxHeightDistanceScale = 3000f;

    [Tooltip("거리 보정값")]
    public float boxHeightDistanceOffset = 0f;
    [Header("Bottom Edge Distance Estimation")]
    [Tooltip("이미지 하단과 박스 하단 사이의 픽셀 거리 1px당 몇 m로 볼지")]
    public float bottomGapScale = 0.01f;
    public float bottomGapExponent = 1.4f;
    [Tooltip("거리 보정값. 전체 거리가 너무 작거나 크면 조절")]
    public float bottomGapDistanceOffset = 0f;

    [Tooltip("최소 거리 제한")]
    public float minEstimatedDistance = 0.5f;

    [Tooltip("최대 거리 제한")]
    public float maxEstimatedDistance = 100f;

    [Header("Direction Filter")]
    [Tooltip("차량 정면 기준 몇 도 안에 있는 대상만 위험 대상으로 볼지")]
    public float forwardAngleLimit = 25f;

    [Tooltip("주행 경로 반폭. 좌우 몇 m 안쪽 대상만 위험 대상으로 볼지")]
    public float drivingPathHalfWidth = 1.0f;

    public bool useDirectionFilter = true;
    public bool usePathWidthFilter = true;

    [Header("Debug")]
    public bool hasTarget;
    public float nearestDistance;
    public VisionDetection nearestDetection;

    public float nearestBottomY;
    public float nearestBottomGapPixel;
    public float nearestHorizontalAngle;
    public float nearestLateralOffset;
    public bool nearestIsInFront;
    public bool nearestIsInDrivingPath;

    private void Update()
    {
        EstimateNearestTarget();
    }

    private void EstimateNearestTarget()
    {
        hasTarget = false;
        nearestDistance = float.MaxValue;
        nearestDetection = null;

        nearestBottomY = 0f;
        nearestBottomGapPixel = 0f;
        nearestHorizontalAngle = 0f;
        nearestLateralOffset = 0f;
        nearestIsInFront = false;
        nearestIsInDrivingPath = false;

        if (visionCapture == null)
        {
            return;
        }

        IReadOnlyList<VisionDetection> detections = visionCapture.LatestDetections;

        if (detections == null || detections.Count == 0)
        {
            return;
        }

        foreach (VisionDetection detection in detections)
        {
            if (detection.classId != targetClassId)
            {
                continue;
            }

            if (detection.confidence < minConfidence)
            {
                continue;
            }

            if (detection.width <= 1f || detection.height <= 1f)
            {
                continue;
            }

            float boxBottomY = GetBoxBottomY(detection);
            float bottomGapPixel = imageHeight - boxBottomY;

            if (bottomGapPixel < 0f)
            {
                bottomGapPixel = 0f;
            }

            float distance;

            if (useBoxHeightDistance)
            {
                distance = EstimateDistanceFromBoxHeight(detection);
            }
            else
            {
                distance = EstimateDistanceFromBottomGap(bottomGapPixel);
            }
            Debug.Log(distance);
            float boxCenterX = GetBoxCenterX(detection);
            float horizontalAngle = EstimateHorizontalAngle(boxCenterX);
            float lateralOffset = Mathf.Tan(horizontalAngle * Mathf.Deg2Rad) * distance;

            bool isInFront = Mathf.Abs(horizontalAngle) <= forwardAngleLimit;
            bool isInDrivingPath = Mathf.Abs(lateralOffset) <= drivingPathHalfWidth;

            if (useDirectionFilter && !isInFront)
            {
                continue;
            }

            if (usePathWidthFilter && !isInDrivingPath)
            {
                continue;
            }

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestDetection = detection;
                hasTarget = true;

                nearestBottomY = boxBottomY;
                nearestBottomGapPixel = bottomGapPixel;
                nearestHorizontalAngle = horizontalAngle;
                nearestLateralOffset = lateralOffset;
                nearestIsInFront = isInFront;
                nearestIsInDrivingPath = isInDrivingPath;
            }
        }
    }

    private float GetBoxBottomY(VisionDetection detection)
    {
        if (bboxXYIsCenter)
        {
            return detection.y + detection.height * 0.5f;
        }

        return detection.y + detection.height;
    }

    private float GetBoxCenterX(VisionDetection detection)
    {
        if (bboxXYIsCenter)
        {
            return detection.x;
        }

        return detection.x + detection.width * 0.5f;
    }
    private float EstimateDistanceFromBoxHeight(VisionDetection detection)
    {
        float h = Mathf.Max(detection.height, 1f);

        float distance = boxHeightDistanceScale / h + boxHeightDistanceOffset;

        return Mathf.Clamp(distance, minEstimatedDistance, maxEstimatedDistance);
    }
    private float EstimateDistanceFromBottomGap(float bottomGapPixel)
    {
        float distance =
            Mathf.Pow(bottomGapPixel, bottomGapExponent) * bottomGapScale
            + bottomGapDistanceOffset;

        return Mathf.Clamp(distance, minEstimatedDistance, maxEstimatedDistance);
    }

    private float EstimateHorizontalAngle(float centerX)
    {
        float horizontalFov = 60f;

        if (aiCamera != null)
        {
            horizontalFov = Camera.VerticalToHorizontalFieldOfView(
                aiCamera.fieldOfView,
                aiCamera.aspect
            );
        }

        float normalizedX = centerX / imageWidth;
        float centeredX = normalizedX - 0.5f;

        return centeredX * horizontalFov;
    }
}