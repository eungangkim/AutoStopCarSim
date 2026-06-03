using UnityEngine;

public class BBoxCoordinateDebugger : MonoBehaviour
{
    public VisionCapture visionCapture;

    public float imageWidth = 1920f;
    public float imageHeight = 1080f;

    private void OnGUI()
    {
        if (visionCapture == null || visionCapture.LatestDetections == null)
            return;

        foreach (var d in visionCapture.LatestDetections)
        {
            float scaleX = Screen.width / imageWidth;
            float scaleY = Screen.height / imageHeight;

            float x = d.x * scaleX;
            float y = d.y * scaleY;
            float w = d.width * scaleX;
            float h = d.height * scaleY;

            // detection.x, detection.y 위치에 빨간 점 표시
            GUI.color = Color.red;
            GUI.Label(new Rect(x - 5, y - 5, 20, 20), "●");

            // 좌상단 기준 박스라고 가정하고 박스 정보 표시
            GUI.color = Color.yellow;
            GUI.Label(new Rect(x, y - 20, 400, 20), $"x,y point / w:{d.width:F1}, h:{d.height:F1}");

            // 중심 위치도 표시
            GUI.color = Color.green;
            GUI.Label(new Rect(x + w * 0.5f - 5, y + h * 0.5f - 5, 20, 20), "●");
        }
    }
}