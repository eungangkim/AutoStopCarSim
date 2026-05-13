using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections;

public class CameraCaptureSender : MonoBehaviour
{
    public Camera targetCamera;
    public RenderTexture renderTexture;
    public int width = 512;
    public int height = 256;
    public string serverIp = "127.0.0.1";
    public int serverPort = 5001;
    public float sendInterval = 0.1f;

    private Texture2D tex;
    public float latestError = 0f;
    public bool laneValid = false;

    void Start()
    {
        tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        StartCoroutine(SendLoop());
    }

    IEnumerator SendLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(sendInterval);
            CaptureAndSend();
        }
    }

    void CaptureAndSend()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        targetCamera.targetTexture = renderTexture;
        targetCamera.Render();

        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        byte[] jpgBytes = tex.EncodeToJPG(75);

        RenderTexture.active = currentRT;
        targetCamera.targetTexture = null;

        try
        {
            using (TcpClient client = new TcpClient(serverIp, serverPort))
            using (NetworkStream stream = client.GetStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                writer.Write(jpgBytes.Length);
                writer.Write(jpgBytes);
                writer.Flush();

                int responseLength = reader.ReadInt32();
                byte[] responseBytes = reader.ReadBytes(responseLength);
                string json = Encoding.UTF8.GetString(responseBytes);

                LaneResult result = JsonUtility.FromJson<LaneResult>(json);
                latestError = result.error;
                laneValid = result.valid;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Lane server connection failed: " + e.Message);
            laneValid = false;
        }
    }

    [System.Serializable]
    public class LaneResult
    {
        public bool valid;
        public float error;
        public float lane_center_x;
        public float image_center_x;
    }
}