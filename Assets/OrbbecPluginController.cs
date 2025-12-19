using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class OrrbecPluginController : MonoBehaviour
{
    public RawImage cameraDisplay;
    public Text statusText;

    // Visualization settings (tweak in Inspector)
    public float visualizationRange = 2000f;    // fallback range in mm
    public bool useBinaryThreshold = false;     // if true, use threshold mode
    public float threshold = 1200f;             // threshold in mm for binary mode
    public bool invertDisplay = true;           // closer = brighter when true

    [DllImport("OrbbecUnityPlugin")]
    private static extern bool InitCamera();

    [DllImport("OrbbecUnityPlugin")]
    private static extern bool GetDepthFrame(ushort[] buffer, ref int width, ref int height);

    [DllImport("OrbbecUnityPlugin")]
    private static extern void CloseCamera();

    [DllImport("OrbbecUnityPlugin")]
    private static extern IntPtr GetLastErrorMessage();

    [DllImport("OrbbecUnityPlugin")]
    private static extern int GetFrameCount();

    private ushort[] depthBuffer;
    private Color[] colorBuffer;
    private Texture2D depthTexture;
    private bool isInitialized = false;
    private bool isShuttingDown = false;
    private float lastFrameTime = 0f;
    private float lastGCTime = 0f;
    private const float FRAME_INTERVAL = 0.05f;
    private const float GC_INTERVAL = 3f;
    private const int WIDTH = 640;
    private const int HEIGHT = 480;

    void Start()
    {
        Debug.Log("=== Orbbec Improved DLL Test ===");
        Debug.Log("Date: " + DateTime.UtcNow.ToString("u"));
        Debug.Log("User: NithinKrishna856-spec");

        UpdateStatus("Initializing...");

        try
        {
            Debug.Log("[INIT] Calling InitCamera...");

            if (InitCamera())
            {
                Debug.Log("[INIT] SUCCESS - Camera initialized!");
                isInitialized = true;

                depthBuffer = new ushort[WIDTH * HEIGHT];
                colorBuffer = new Color[WIDTH * HEIGHT];

                depthTexture = new Texture2D(WIDTH, HEIGHT, TextureFormat.RGB24, false);
                depthTexture.filterMode = FilterMode.Point;
                depthTexture.wrapMode = TextureWrapMode.Clamp;

                if (cameraDisplay != null)
                {
                    cameraDisplay.texture = depthTexture;
                    Debug.Log("[INIT] Texture assigned");
                }

                UpdateStatus("Ready!");
                lastGCTime = Time.time;
            }
            else
            {
                string error = Marshal.PtrToStringAnsi(GetLastErrorMessage());
                Debug.LogError("[INIT] FAILED: " + error);
                UpdateStatus("ERROR: " + error);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[INIT] Exception: " + ex.Message);
            UpdateStatus("ERROR");
        }
    }

    void Update()
    {
        if (!isInitialized || isShuttingDown) return;

        if (Time.time - lastFrameTime < FRAME_INTERVAL) return;
        lastFrameTime = Time.time;

        if (Time.time - lastGCTime > GC_INTERVAL)
        {
            GC.Collect();
            lastGCTime = Time.time;

            int nativeFrames = GetFrameCount();
            Debug.Log("[GC] Collected | DLL Frames: " + nativeFrames);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[INPUT] ESC - shutting down...");
            Shutdown();
            return;
        }

        try
        {
            int w = WIDTH;
            int h = HEIGHT;

            // Debug-enabled GetDepthFrame block
            if (GetDepthFrame(depthBuffer, ref w, ref h))
            {
                UpdateTexture();

                int frames = GetFrameCount();
                if (frames % 20 == 0)
                {
                    UpdateStatus("Frames: " + frames);

                    // DEBUG: Print min/max and a center pixel value
                    ushort minDepth = ushort.MaxValue;
                    ushort maxDepth = ushort.MinValue;
                    int nonZeroCount = 0;
                    for (int i = 0; i < depthBuffer.Length; i++)
                    {
                        ushort v = depthBuffer[i];
                        if (v == 0) continue;
                        nonZeroCount++;
                        if (v < minDepth) minDepth = v;
                        if (v > maxDepth) maxDepth = v;
                    }
                    int centerIndex = (HEIGHT / 2) * WIDTH + (WIDTH / 2);
                    ushort centerVal = depthBuffer.Length > centerIndex ? depthBuffer[centerIndex] : (ushort)0;
                    Debug.Log($"[DEPTH] Min:{minDepth} Max:{maxDepth} NonZero:{nonZeroCount} Center:{centerVal} Frames:{frames}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[UPDATE] Error: " + ex.Message);
            Shutdown();
        }
    }

    void UpdateTexture()
    {
        try
        {
            const ushort INVALID_SENTINEL = 65000; // treat >= this as invalid (65535 is common)

            // find min/max excluding invalid/zero
            ushort minV = ushort.MaxValue;
            ushort maxV = 0;
            for (int i = 0; i < depthBuffer.Length; i++)
            {
                ushort v = depthBuffer[i];
                if (v == 0 || v >= INVALID_SENTINEL) continue;
                if (v < minV) minV = v;
                if (v > maxV) maxV = v;
            }

            bool useMinMax = (minV < maxV);
            float mapRange = visualizationRange;
            float mapOffset = 0f;

            if (useMinMax)
            {
                // Normalize using measured min..max (avoid sentinel values)
                mapOffset = minV;
                mapRange = Mathf.Max(1.0f, (float)(maxV - minV));
            }
            else
            {
                // fallback: visualize within visualizationRange starting at 0
                mapOffset = 0f;
                mapRange = Mathf.Max(1.0f, visualizationRange);
            }

            if (useBinaryThreshold)
            {
                // Binary threshold mode (close = white)
                for (int i = 0; i < depthBuffer.Length; i++)
                {
                    ushort v = depthBuffer[i];
                    bool isClose = (v > 0 && v < threshold && v < INVALID_SENTINEL);
                    float t = isClose ? 1f : 0f;
                    colorBuffer[i].r = t;
                    colorBuffer[i].g = t;
                    colorBuffer[i].b = t;
                    colorBuffer[i].a = 1f;
                }
            }
            else
            {
                // Continuous grayscale mapping (closer -> brighter by default)
                for (int i = 0; i < depthBuffer.Length; i++)
                {
                    ushort v = depthBuffer[i];
                    if (v == 0 || v >= INVALID_SENTINEL)
                    {
                        // invalid: render as black
                        colorBuffer[i].r = 0f;
                        colorBuffer[i].g = 0f;
                        colorBuffer[i].b = 0f;
                        colorBuffer[i].a = 1f;
                    }
                    else
                    {
                        float normalized = ((float)v - mapOffset) / mapRange; // 0..1 (farther -> larger)
                        float display = Mathf.Clamp01(normalized);
                        if (invertDisplay)
                        {
                            display = 1.0f - display; // closer becomes brighter
                        }
                        colorBuffer[i].r = display;
                        colorBuffer[i].g = display;
                        colorBuffer[i].b = display;
                        colorBuffer[i].a = 1f;
                    }
                }
            }

            depthTexture.SetPixels(colorBuffer);
            depthTexture.Apply(false);
        }
        catch (Exception ex)
        {
            Debug.LogError("[TEXTURE] Error: " + ex.Message);
        }
    }

    void UpdateStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    void Shutdown()
    {
        if (isShuttingDown) return;

        isShuttingDown = true;
        isInitialized = false;

        Debug.Log("[SHUTDOWN] Starting...");

        try
        {
            if (depthTexture != null)
            {
                Destroy(depthTexture);
                depthTexture = null;
            }

            if (cameraDisplay != null)
            {
                cameraDisplay.texture = null;
            }

            System.Threading.Thread.Sleep(200);

            int finalCount = GetFrameCount();
            Debug.Log("[SHUTDOWN] Final frame count: " + finalCount);

            CloseCamera();

            string msg = Marshal.PtrToStringAnsi(GetLastErrorMessage());
            Debug.Log("[SHUTDOWN] " + msg);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Debug.Log("[SHUTDOWN] Complete!");
        }
        catch (Exception ex)
        {
            Debug.LogError("[SHUTDOWN] Error: " + ex.Message);
        }
    }

    void OnDestroy() { Shutdown(); }
    void OnApplicationQuit() { Shutdown(); }
    void OnDisable() { Shutdown(); }

    // --------- Public depth accessors for depth-lifting integration ---------
    // Returns depth in millimetres for pixel (x,y). (0 or >=65000 = invalid)
    public ushort GetDepthAtPixel(int x, int y)
    {
        if (depthBuffer == null) return 0;
        if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT) return 0;
        return depthBuffer[y * WIDTH + x];
    }

    public int GetDepthWidth() { return WIDTH; }
    public int GetDepthHeight() { return HEIGHT; }
}