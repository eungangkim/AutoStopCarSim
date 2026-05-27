using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.InferenceEngine; // 💡 정확한 최신 네임스페이스 적용

public struct DetectedObject
{
    public int classID;
    public float confidence;
    public Rect boundingBox;
}

public class YoloParser
{
    public static List<DetectedObject> ParseAndNMS(Tensor<float> outputTensor, float confThreshold = 0.25f, float iouThreshold = 0.45f)
    {
        int numClasses = outputTensor.shape[1] - 4;
        int numBoxes = outputTensor.shape[2];

        // InferenceEngine 최신 문법
        outputTensor.CompleteAllPendingOperations();
        ReadOnlySpan<float> tensorData = outputTensor.AsReadOnlySpan();

        List<DetectedObject> candidates = new List<DetectedObject>();

        // [C-1] 노이즈 필터링
        for (int i = 0; i < numBoxes; i++)
        {
            float maxConf = 0f;
            int bestClassID = -1;

            for (int c = 0; c < numClasses; c++)
            {
                float conf = tensorData[(c + 4) * numBoxes + i];
                if (conf > maxConf)
                {
                    maxConf = conf;
                    bestClassID = c;
                }
            }

            if (maxConf >= confThreshold)
            {
                float cx = tensorData[0 * numBoxes + i];
                float cy = tensorData[1 * numBoxes + i];
                float w = tensorData[2 * numBoxes + i];
                float h = tensorData[3 * numBoxes + i];

                float xMin = cx - (w / 2f);
                float yMin = cy - (h / 2f);

                candidates.Add(new DetectedObject
                {
                    classID = bestClassID,
                    confidence = maxConf,
                    boundingBox = new Rect(xMin, yMin, w, h)
                });
            }
        }

        // [C-2] 확률 내림차순 정렬
        candidates.Sort((a, b) => b.confidence.CompareTo(a.confidence));

        List<DetectedObject> finalObjects = new List<DetectedObject>();

        // [C-2] NMS (겹침 제거)
        foreach (var candidate in candidates)
        {
            bool isOverlap = false;

            foreach (var finalObj in finalObjects)
            {
                if (candidate.classID == finalObj.classID)
                {
                    float iou = CalculateIoU(candidate.boundingBox, finalObj.boundingBox);
                    if (iou > iouThreshold)
                    {
                        isOverlap = true;
                        break;
                    }
                }
            }

            if (!isOverlap)
            {
                finalObjects.Add(candidate);
            }
        }

        return finalObjects;
    }

    private static float CalculateIoU(Rect boxA, Rect boxB)
    {
        float x1 = Mathf.Max(boxA.xMin, boxB.xMin);
        float y1 = Mathf.Max(boxA.yMin, boxB.yMin);
        float x2 = Mathf.Min(boxA.xMax, boxB.xMax);
        float y2 = Mathf.Min(boxA.yMax, boxB.yMax);

        float intersectionArea = Mathf.Max(0, x2 - x1) * Mathf.Max(0, y2 - y1);
        float unionArea = (boxA.width * boxA.height) + (boxB.width * boxB.height) - intersectionArea;

        return intersectionArea / (unionArea + 1e-6f);
    }
}