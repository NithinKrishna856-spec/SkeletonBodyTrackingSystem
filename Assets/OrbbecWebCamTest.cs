using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OrbbecWebCamTest : MonoBehaviour
{
    public RawImage displayImage;
    private WebCamTexture webCamTexture;

    void Start()
    {
        StartCoroutine(StartCamera());
    }

    IEnumerator StartCamera()
    {
        // List all available cameras
        WebCamDevice[] devices = WebCamTexture.devices;

        Debug.Log("========== AVAILABLE CAMERAS ==========");
        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log("[" + i + "] " + devices[i].name);
        }
        Debug.Log("========================================");

        // Find Orbbec RGB camera
        string orbbecCamera = null;
        foreach (var device in devices)
        {
            if (device.name.ToLower().Contains("orbbec") ||
                device.name.ToLower().Contains("gemini"))
            {
                if (device.name.ToLower().Contains("rgb"))
                {
                    orbbecCamera = device.name;
                    break;
                }
            }
        }

        if (orbbecCamera != null)
        {
            Debug.Log(">>> USING ORBBEC:  " + orbbecCamera);
            webCamTexture = new WebCamTexture(orbbecCamera, 1280, 720, 30);
        }
        else
        {
            Debug.LogWarning("Orbbec RGB not found!  Using default camera.");
            webCamTexture = new WebCamTexture(1280, 720, 30);
        }

        // Start the camera
        webCamTexture.Play();
        Debug.Log("Camera Play() called.. .");

        // Wait for camera to initialize
        int timeout = 100;
        while (webCamTexture.width < 100 && timeout > 0)
        {
            Debug.Log("Waiting for camera...  width = " + webCamTexture.width);
            yield return new WaitForSeconds(0.1f);
            timeout--;
        }

        if (webCamTexture.width < 100)
        {
            Debug.LogError("Camera failed to start!");
            yield break;
        }

        Debug.Log("Camera READY!  Size: " + webCamTexture.width + "x" + webCamTexture.height);

        // NOW assign the texture
        if (displayImage != null)
        {
            displayImage.texture = webCamTexture;
            displayImage.rectTransform.sizeDelta = new Vector2(webCamTexture.width, webCamTexture.height);
            Debug.Log("Texture assigned to RawImage!");
        }
        else
        {
            Debug.LogError("displayImage is null!");
        }
    }

    void OnDestroy()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }
}