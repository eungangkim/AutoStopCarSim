using System;
using UnityEngine;

[Serializable]
public class VisionDetection
{
    public int classId;
    public float confidence;

    // Pixel 좌표 기준
    public float x;
    public float y;
    public float width;
    public float height;

    public VisionDetection(int classId, float confidence, float x, float y, float width, float height)
    {
        this.classId = classId;
        this.confidence = confidence;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public Vector2 Center
    {
        get
        {
            return new Vector2(x, y);
        }
    }

    public float Area
    {
        get
        {
            return width * height;
        }
    }
}