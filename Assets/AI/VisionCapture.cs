using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class VisionCapture : MonoBehaviour
{
    [Header("카메라 설정")]
    public Camera visionCamera;
    public float captureInterval = 0.5f;

    private float timer = 0f;
    private Texture2D resultTexture;

    void Start()
    {
        resultTexture = new Texture2D(224, 224, TextureFormat.RGBA32, false);
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
    }

    void OnDestroy()
    {
        if (resultTexture != null)
        {
            Destroy(resultTexture);
        }
    }
}