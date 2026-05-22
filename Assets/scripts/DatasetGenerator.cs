using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DatasetGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform carTransform;
    public Camera captureCamera;
    public GameObject pedestrianPrefab;
    public TMP_InputField pedestrianCountInput;

    [Header("Spawn Settings")]
    public float minForwardDistance = 10f;
    public float maxForwardDistance = 40f;
    public float horizontalRange = 5f;
    public float spawnHeight = 0f;

    [Header("Capture Settings")]
    public int imageWidth = 1280;
    public int imageHeight = 720;

    [Header("Dataset Settings")]
    public string datasetFolderName = "Dataset";

    private readonly List<GameObject> spawnedPedestrians = new List<GameObject>();
    private int captureIndex = 1;

    private string imageFolder;
    private string boxedFolder;
    private string labelFolder;

    private void Start()
    {
        string root = Path.Combine(Application.dataPath, "..", datasetFolderName);

        imageFolder = Path.Combine(root, "images");
        boxedFolder = Path.Combine(root, "boxed");
        labelFolder = Path.Combine(root, "labels");

        Directory.CreateDirectory(imageFolder);
        Directory.CreateDirectory(boxedFolder);
        Directory.CreateDirectory(labelFolder);
    }

    public void SpawnPedestrians()
    {
        ClearPedestrians();

        if (!int.TryParse(pedestrianCountInput.text, out int count))
        {
            Debug.LogWarning("보행자 개수를 숫자로 입력하세요.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 forward = carTransform.forward;
            Vector3 right = carTransform.right;

            float forwardDistance = Random.Range(minForwardDistance, maxForwardDistance);
            float horizontalOffset = Random.Range(-horizontalRange, horizontalRange);

            Vector3 spawnPos =
                carTransform.position +
                forward * forwardDistance +
                right * horizontalOffset;

            spawnPos.y = spawnHeight;

            GameObject pedestrian = Instantiate(
                pedestrianPrefab,
                spawnPos,
                Quaternion.identity
            );

            // 보행자가 자동차 쪽을 바라보도록 회전
            Vector3 lookDir = carTransform.position - pedestrian.transform.position;
            lookDir.y = 0f;

            if (lookDir != Vector3.zero)
            {
                pedestrian.transform.rotation = Quaternion.LookRotation(lookDir);
            }

            spawnedPedestrians.Add(pedestrian);
        }

        Debug.Log($"{count}명의 보행자를 생성했습니다.");
    }

    public void ClearPedestrians()
    {
        foreach (GameObject obj in spawnedPedestrians)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        spawnedPedestrians.Clear();
    }

    public void CaptureDataset()
    {
        Texture2D inputImage = CaptureCameraImage();

        List<BoundingBoxData> boxes = CalculateBoundingBoxes();

        Texture2D boxedImage = new Texture2D(inputImage.width, inputImage.height, TextureFormat.RGB24, false);
        boxedImage.SetPixels(inputImage.GetPixels());
        boxedImage.Apply();

        DrawBoundingBoxes(boxedImage, boxes);

        string fileNumber = captureIndex.ToString("D6");

        string inputPath = Path.Combine(imageFolder, $"input_{fileNumber}.png");
        string boxedPath = Path.Combine(boxedFolder, $"result_{fileNumber}.png");
        string labelPath = Path.Combine(labelFolder, $"input_{fileNumber}.txt");

        File.WriteAllBytes(inputPath, inputImage.EncodeToPNG());
        File.WriteAllBytes(boxedPath, boxedImage.EncodeToPNG());
        SaveYoloLabels(labelPath, boxes, inputImage.width, inputImage.height);

        Debug.Log($"데이터 저장 완료: {fileNumber}");

        Destroy(inputImage);
        Destroy(boxedImage);

        captureIndex++;
    }

    private Texture2D CaptureCameraImage()
    {
        RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        captureCamera.targetTexture = renderTexture;

        Texture2D image = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);

        captureCamera.Render();

        RenderTexture.active = renderTexture;
        image.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        image.Apply();

        captureCamera.targetTexture = null;
        RenderTexture.active = null;

        Destroy(renderTexture);

        return image;
    }

    private List<BoundingBoxData> CalculateBoundingBoxes()
    {
        List<BoundingBoxData> boxes = new List<BoundingBoxData>();

        foreach (GameObject obj in spawnedPedestrians)
        {
            if (obj == null) continue;

            DatasetObject datasetObject = obj.GetComponent<DatasetObject>();
            Renderer renderer = obj.GetComponentInChildren<Renderer>();

            if (datasetObject == null || renderer == null) continue;

            Bounds bounds = renderer.bounds;

            Vector3[] corners = GetBoundsCorners(bounds);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            bool visible = false;

            foreach (Vector3 corner in corners)
            {
                Vector3 viewportPoint = captureCamera.WorldToViewportPoint(corner);

                if (viewportPoint.z > 0)
                {
                    visible = true;

                    minX = Mathf.Min(minX, viewportPoint.x);
                    minY = Mathf.Min(minY, viewportPoint.y);
                    maxX = Mathf.Max(maxX, viewportPoint.x);
                    maxY = Mathf.Max(maxY, viewportPoint.y);
                }
            }

            if (!visible) continue;

            minX = Mathf.Clamp01(minX);
            minY = Mathf.Clamp01(minY);
            maxX = Mathf.Clamp01(maxX);
            maxY = Mathf.Clamp01(maxY);

            float width = maxX - minX;
            float height = maxY - minY;

            if (width <= 0.01f || height <= 0.01f) continue;

            BoundingBoxData box = new BoundingBoxData
            {
                classId = datasetObject.classId,
                xMin = minX,
                yMin = minY,
                xMax = maxX,
                yMax = maxY
            };

            boxes.Add(box);
        }

        return boxes;
    }

    private Vector3[] GetBoundsCorners(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        return new Vector3[]
        {
            center + new Vector3(-extents.x, -extents.y, -extents.z),
            center + new Vector3(-extents.x, -extents.y,  extents.z),
            center + new Vector3(-extents.x,  extents.y, -extents.z),
            center + new Vector3(-extents.x,  extents.y,  extents.z),
            center + new Vector3( extents.x, -extents.y, -extents.z),
            center + new Vector3( extents.x, -extents.y,  extents.z),
            center + new Vector3( extents.x,  extents.y, -extents.z),
            center + new Vector3( extents.x,  extents.y,  extents.z)
        };
    }

    private void DrawBoundingBoxes(Texture2D image, List<BoundingBoxData> boxes)
    {
        foreach (BoundingBoxData box in boxes)
        {
            int xMin = Mathf.RoundToInt(box.xMin * image.width);
            int xMax = Mathf.RoundToInt(box.xMax * image.width);

            // Unity viewport y는 아래가 0, 이미지 픽셀도 아래가 0 기준이라 그대로 사용
            int yMin = Mathf.RoundToInt(box.yMin * image.height);
            int yMax = Mathf.RoundToInt(box.yMax * image.height);

            DrawRectangle(image, xMin, yMin, xMax, yMax, Color.red, 3);
        }

        image.Apply();
    }

    private void DrawRectangle(Texture2D image, int xMin, int yMin, int xMax, int yMax, Color color, int thickness)
    {
        for (int t = 0; t < thickness; t++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                SetPixelSafe(image, x, yMin + t, color);
                SetPixelSafe(image, x, yMax - t, color);
            }

            for (int y = yMin; y <= yMax; y++)
            {
                SetPixelSafe(image, xMin + t, y, color);
                SetPixelSafe(image, xMax - t, y, color);
            }
        }
    }

    private void SetPixelSafe(Texture2D image, int x, int y, Color color)
    {
        if (x < 0 || x >= image.width) return;
        if (y < 0 || y >= image.height) return;

        image.SetPixel(x, y, color);
    }

    private void SaveYoloLabels(string path, List<BoundingBoxData> boxes, int imageWidth, int imageHeight)
    {
        List<string> lines = new List<string>();

        foreach (BoundingBoxData box in boxes)
        {
            float centerX = (box.xMin + box.xMax) / 2f;
            float centerY = (box.yMin + box.yMax) / 2f;
            float width = box.xMax - box.xMin;
            float height = box.yMax - box.yMin;

            // YOLO는 보통 y축도 0~1 정규화 좌표를 사용한다.
            // 이 구조에서는 캡쳐 이미지와 viewport 기준이 동일하게 저장되므로 그대로 사용한다.
            string line = $"{box.classId} {centerX:F6} {centerY:F6} {width:F6} {height:F6}";
            lines.Add(line);
        }

        File.WriteAllLines(path, lines);
    }

    private class BoundingBoxData
    {
        public int classId;
        public float xMin;
        public float yMin;
        public float xMax;
        public float yMax;
    }
}