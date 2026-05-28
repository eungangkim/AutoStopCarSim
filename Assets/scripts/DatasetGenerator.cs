using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DatasetGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform carTransform;
    public Camera captureCamera;
    public GameObject pedestrianPrefab;
    public TMP_InputField pedestrianCountInput;
    public TMP_InputField bulkCountInput;

    [Header("Car Random Spawn Settings")]
    public Transform carSpawnPointParent;
    public Transform[] carSpawnPoints;
    public float carPositionRandomOffset = 1.5f;
    public float carYawRandomOffset = 10f;

    [Header("Pedestrian Count Random Settings")]
    public int minPedestrianCount = 1;
    public int maxPedestrianCount = 5;

    [Header("Pedestrian Spawn Settings")]
    public float minForwardDistance = 10f;
    public float maxForwardDistance = 40f;
    public float horizontalRange = 5f;
    public float spawnHeight = 0f;

    [Header("Camera Random Settings")]
    public bool useCameraRandomRotation = false;
    public float cameraYawRandomOffset = 0f;
    public float cameraPitchRandomOffset = 1f;

    [Header("Light Random Settings")]
    public Light directionalLight;
    public bool useLightRandom = true;
    public float minLightIntensity = 0.7f;
    public float maxLightIntensity = 1.4f;

    [Header("Capture Settings")]
    public int imageWidth = 1920;
    public int imageHeight = 1080;

    [Header("Dataset Settings")]
    public string datasetFolderName = "Dataset";

    private readonly List<GameObject> spawnedPedestrians = new List<GameObject>();

    private int captureIndex = 1;

    private string imageFolder;
    private string boxedFolder;
    private string labelFolder;

    private Quaternion originalCameraRotation;

    private void Start()
    {
        string root = Path.Combine(Application.dataPath, "..", datasetFolderName);

        imageFolder = Path.Combine(root, "images");
        boxedFolder = Path.Combine(root, "boxed");
        labelFolder = Path.Combine(root, "labels");

        Directory.CreateDirectory(imageFolder);
        Directory.CreateDirectory(boxedFolder);
        Directory.CreateDirectory(labelFolder);

        originalCameraRotation = captureCamera.transform.localRotation;

        LoadCarSpawnPoints();
        Debug.Log("Dataset 저장 경로: " + root);
    }

    public void SpawnPedestrians()
    {
        ClearPedestrians();

        if (!int.TryParse(pedestrianCountInput.text, out int count))
        {
            Debug.LogWarning("보행자 개수를 숫자로 입력하세요.");
            return;
        }

        SpawnRandomPedestrians(count);
    }

    private void SpawnRandomPedestrians(int count)
    {
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

            RandomizePedestrianRotation(pedestrian);

            spawnedPedestrians.Add(pedestrian);
        }
    }

    private void RandomizePedestrianRotation(GameObject pedestrian)
    {
        int mode = Random.Range(0, 4);

        if (mode == 0)
        {
            // 자동차 쪽 바라보기
            Vector3 lookDir = carTransform.position - pedestrian.transform.position;
            lookDir.y = 0f;

            if (lookDir != Vector3.zero)
            {
                pedestrian.transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
        else
        {
            // 아무 방향 바라보기
            float randomY = Random.Range(0f, 360f);
            pedestrian.transform.rotation = Quaternion.Euler(0f, randomY, 0f);
        }
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
        SaveYoloLabels(labelPath, boxes);

        Debug.Log($"데이터 저장 완료: {fileNumber}, 객체 수: {boxes.Count}");

        Destroy(inputImage);
        Destroy(boxedImage);

        captureIndex++;
    }

    public void GenerateBulkDataset()
    {
        if (!int.TryParse(bulkCountInput.text, out int bulkCount))
        {
            Debug.LogWarning("생성할 데이터 개수를 숫자로 입력하세요.");
            return;
        }

        StartCoroutine(BulkGenerateCoroutine(bulkCount));
    }

    private IEnumerator BulkGenerateCoroutine(int bulkCount)
    {
        for (int i = 0; i < bulkCount; i++)
        {
            ClearPedestrians();

            RandomizeCarPosition();
            RandomizeCameraRotation();
            RandomizeLight();

            int pedestrianCount = Random.Range(minPedestrianCount, maxPedestrianCount + 1);
            SpawnRandomPedestrians(pedestrianCount);

            // 오브젝트 생성 및 위치 변경이 반영되도록 한 프레임 대기
            yield return null;

            CaptureDataset();

            if ((i + 1) % 100 == 0)
            {
                Debug.Log($"{i + 1}/{bulkCount}개 데이터 생성 완료");
            }

            yield return null;
        }

        ResetCameraRotation();

        Debug.Log($"대량 데이터 생성 완료: {bulkCount}개");
    }

    private void RandomizeCarPosition()
    {
        if (carSpawnPoints == null || carSpawnPoints.Length == 0)
        {
            Debug.LogWarning("Car Spawn Points가 설정되지 않았습니다.");
            return;
        }

        Transform selectedPoint = carSpawnPoints[Random.Range(0, carSpawnPoints.Length)];

        Vector3 randomOffset = new Vector3(
            Random.Range(-carPositionRandomOffset, carPositionRandomOffset),
            0f,
            Random.Range(-carPositionRandomOffset, carPositionRandomOffset)
        );

        carTransform.position = selectedPoint.position + randomOffset;

        float randomYaw = Random.Range(-carYawRandomOffset, carYawRandomOffset);
        carTransform.rotation = selectedPoint.rotation * Quaternion.Euler(0f, randomYaw, 0f);
    }

    private void RandomizeCameraRotation()
    {
        if (!useCameraRandomRotation) return;

        float randomYaw = Random.Range(-cameraYawRandomOffset, cameraYawRandomOffset);
        float randomPitch = Random.Range(-cameraPitchRandomOffset, cameraPitchRandomOffset);

        captureCamera.transform.localRotation =
            originalCameraRotation * Quaternion.Euler(randomPitch, randomYaw, 0f);
    }

    private void ResetCameraRotation()
    {
        captureCamera.transform.localRotation = originalCameraRotation;
    }

    private void RandomizeLight()
    {
        if (!useLightRandom) return;
        if (directionalLight == null) return;

        directionalLight.intensity = Random.Range(minLightIntensity, maxLightIntensity);

        float randomX = Random.Range(30f, 70f);
        float randomY = Random.Range(0f, 360f);

        directionalLight.transform.rotation = Quaternion.Euler(randomX, randomY, 0f);
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

    private void SaveYoloLabels(string path, List<BoundingBoxData> boxes)
    {
        List<string> lines = new List<string>();

        foreach (BoundingBoxData box in boxes)
        {
            float centerX = (box.xMin + box.xMax) / 2f;
            float centerY = (box.yMin + box.yMax) / 2f;
            float width = box.xMax - box.xMin;
            float height = box.yMax - box.yMin;

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

    private void LoadCarSpawnPoints()
    {
        if (carSpawnPointParent == null)
        {
            Debug.LogWarning("Car Spawn Point Parent가 설정되지 않았습니다.");
            return;
        }

        List<Transform> spawnPointList = new List<Transform>();

        Transform[] allChildren = carSpawnPointParent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child == carSpawnPointParent)
                continue;

            if (child.name.StartsWith("SpawnPoint"))
            {
                spawnPointList.Add(child);
            }
        }

        carSpawnPoints = spawnPointList.ToArray();

        Debug.Log($"Car Spawn Point {carSpawnPoints.Length}개 자동 등록 완료");
    }
}

