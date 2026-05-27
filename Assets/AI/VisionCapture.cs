using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Collections.Generic;
using Unity.InferenceEngine; // 💡 정확한 최신 네임스페이스 적용

public class VisionCapture : MonoBehaviour
{
    [Header("카메라 설정")]
    public Camera visionCamera;
    public float captureInterval = 0.5f;

    [Header("AI 모델 설정")]
    public ModelAsset yoloModelAsset;

    private float timer = 0f;
    private Texture2D resultTexture;

    private Model runtimeModel;
    private Worker worker;

    void Start()
    {
        resultTexture = new Texture2D(224, 224, TextureFormat.RGBA32, false);

        if (yoloModelAsset != null)
        {
            runtimeModel = ModelLoader.Load(yoloModelAsset);

            // 노트북 부하 고려하여 CPU만 사용
            worker = new Worker(runtimeModel, BackendType.CPU);
            Debug.Log("🟢 AI 모델 로드 및 CPU 워커 생성 완료");
        }
        else
        {
            Debug.LogError("🔴 YOLO ONNX 모델 파일이 연결되지 않았습니다");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= captureInterval)
        {
            timer = 0f;
            CaptureScreenAsync();
        }
    }

    private void CaptureScreenAsync()
    {
        RenderTexture rt = visionCamera.targetTexture;
        if (rt == null) return;

        AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32, OnCompleteReadback);
    }

    private void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError || !Application.isPlaying) return;

        NativeArray<byte> pixelData = request.GetData<byte>();
        resultTexture.LoadRawTextureData(pixelData);
        resultTexture.Apply();

        RunInference(resultTexture);
    }

    private void RunInference(Texture2D inputTex)
    {
        if (worker == null) return;

        // 텍스처를 텐서로 변환
        using Tensor<float> inputTensor = TextureConverter.ToTensor(inputTex, width: 224, height: 224, channels: 3);

        // 워커에게 텐서 주고 계산
        worker.Schedule(inputTensor);

        // 워커의 출력값(텐서)
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

        // ---------------------------------------------------------
        // [C-1 & C-2] 데이터 파싱 및 NMS 처리
        // ---------------------------------------------------------
        List<DetectedObject> detectedObjects = YoloParser.ParseAndNMS(outputTensor, confThreshold: 0.25f, iouThreshold: 0.45f);

        // ---------------------------------------------------------
        // [C-3] 화면 비율에 맞게 좌표 뻥튀기 및 로그 출력
        // ---------------------------------------------------------
        float scaleX = (float)Screen.width / 224f;
        float scaleY = (float)Screen.height / 224f;

        if (detectedObjects.Count > 0)
        {
            Debug.Log($"============== 🟢 감지된 객체 수: {detectedObjects.Count}개 ==============");

            foreach (var obj in detectedObjects)
            {
                float screenX = obj.boundingBox.xMin * scaleX;
                float screenY = obj.boundingBox.yMin * scaleY;
                float screenW = obj.boundingBox.width * scaleX;
                float screenH = obj.boundingBox.height * scaleY;

                Debug.Log($"🎯 [클래스 {obj.classID}] 확률: {obj.confidence * 100:F1}% | " +
                          $"위치(X:{screenX:F0}, Y:{screenY:F0}) 크기(W:{screenW:F0}, H:{screenH:F0})");
            }
        }
    }

    void OnDestroy()
    {
        if (resultTexture != null)
        {
            Destroy(resultTexture);
        }

        worker?.Dispose();
    }
}