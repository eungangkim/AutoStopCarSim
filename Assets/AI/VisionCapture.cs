using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;



public class VisionCapture : MonoBehaviour
{
    [Header("카메라 설정")]
    public Camera visionCamera;
    public float captureInterval = 0.5f;

    [Header("AI 모델 설정")]
    public Unity.InferenceEngine.ModelAsset yoloModelAsset;

    private float timer = 0f;
    private Texture2D resultTexture;

    // Sentis 객체
    private Unity.InferenceEngine.Model runtimeModel;
    private Unity.InferenceEngine.Worker worker;

    void Start()
    {
        resultTexture = new Texture2D(224, 224, TextureFormat.RGBA32, false);

        if (yoloModelAsset != null)
        {
            runtimeModel = Unity.InferenceEngine.ModelLoader.Load(yoloModelAsset);

            //노트북 부하 고려하여 CPU만 사용, GPU 쓸거면 아래 부분 주석으로 변경
            //worker = new Worker(runtimeModel, BackendType.GPUCompute);
            worker = new Unity.InferenceEngine.Worker(runtimeModel, Unity.InferenceEngine.BackendType.CPU);
            Debug.Log("AI 모델 로드 및 GPU 워커 생성 완료");
        }
        else
        {
            Debug.LogError("YOLO ONNX 모델 파일이 연결되지 않았습니다");
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

        Debug.Log($"비동기 방식으로 캡쳐, AI에게 넘길 {resultTexture.width}x{resultTexture.height}size의 텍스쳐 준비됨.");
        //캡처 이후 AI 추론 로직 실행
        RunInference(resultTexture);
    }

    // B단계 핵심: AI 추론(Execute) 로직
    private void RunInference(Texture2D inputTex)
    {
        if (worker == null) return;

        //텍스처를 텐서로 변환
        using Unity.InferenceEngine.Tensor<float> inputTensor = Unity.InferenceEngine.TextureConverter.ToTensor(inputTex, width: 224, height: 224, channels: 3);

        //워커에게 텐서 주고 계산
        worker.Schedule(inputTensor);

        //워커의 출력값(텐서)
        Unity.InferenceEngine.Tensor<float> outputTensor = worker.PeekOutput() as Unity.InferenceEngine.Tensor<float>;

        //텐서 형태 로그로 확인
        Debug.Log($"추론 완료, 출력 텐서 형태(Shape): {outputTensor.shape}");
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