using UnityEngine;
using System.Collections;

public class SimpleWebCam : MonoBehaviour
{
    private WebCamTexture webCamTexture;

    void Start()
    {
        StartCoroutine(StartCamera());
    }

    IEnumerator StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        Debug.Log("Found " + devices.Length + " cameras");

        foreach (WebCamDevice device in devices)
        {
            Debug.Log("Camera: " + device.name);
        }

        if (devices.Length == 0)
        {
            Debug.LogError("No cameras!");
            yield break;
        }

        string camName = devices[0].name;

        foreach (WebCamDevice device in devices)
        {
            if (device.name.Contains("RGB"))
            {
                camName = device.name;
                break;
            }
        }

        Debug.Log("Using: " + camName);

        webCamTexture = new WebCamTexture(camName, 1280, 720);
        GetComponent<Renderer>().material.mainTexture = webCamTexture;
        webCamTexture.Play();

        yield return new WaitForSeconds(2);

        Debug.Log("Result: " + webCamTexture.width + "x" + webCamTexture.height);
    }

    void OnDestroy()
    {
        if (webCamTexture != null) webCamTexture.Stop();
    }
}