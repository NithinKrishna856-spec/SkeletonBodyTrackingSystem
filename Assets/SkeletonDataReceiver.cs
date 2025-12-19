using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

[System.Serializable]
public class JointData { public int id; public Vector3Data position; public float visibility; }
[System.Serializable]
public class Vector3Data { public float x, y, z; }
[System.Serializable]
public class SkeletonMessage { public int frame; public List<JointData> joints; }

public class SkeletonDataReceiver : MonoBehaviour
{
    [Header("Network")]
    public int port = 5065;

    [Header("Visuals - Spine Mode")]
    [Range(1f, 30f)] public float globalScale = 15.0f;
    [Range(0.001f, 0.1f)] public float boneThickness = 0.01f;
    [Range(0.01f, 0.2f)] public float jointSize = 0.05f;

    [Header("UI Dashboard")]
    public Text dashboardText;

    // Internals
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning = false;
    private string lastPacket = "";
    private bool newDataAvailable = false;
    private GameObject[] joints = new GameObject[33];
    private GameObject[] limbs = new GameObject[30]; // Increased for virtual bones

    // We removed the "Box" connections (11-23, 12-24, 23-24)
    private int[,] bonePairs = new int[,] {
        {0, 1}, {1, 2}, {2, 3}, {3, 7}, {0, 4}, {4, 5}, {5, 6}, {6, 8}, // Face
        {9, 10}, // Mouth
        {11, 13}, {13, 15}, // Left Arm
        {12, 14}, {14, 16}, // Right Arm
        {23, 25}, {25, 27}, // Left Leg
        {24, 26}, {26, 28}, // Right Leg
        {27, 29}, {29, 31}, {28, 30}, {30, 32} // Feet
    };

    // Virtual Bones (Spine, Neck, Clavicles)
    private GameObject spineBone, lClavicle, rClavicle, lHipBone, rHipBone;

    void Start()
    {
        InitializeSkeleton();
        StartUDP();
    }

    void InitializeSkeleton()
    {
        // 1. Joints
        for (int i = 0; i < 33; i++)
        {
            joints[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            joints[i].transform.parent = transform;
            Destroy(joints[i].GetComponent<Collider>());
            joints[i].name = "Joint_" + i;
            joints[i].GetComponent<Renderer>().material = new Material(Shader.Find("Sprites/Default"));
            joints[i].GetComponent<Renderer>().material.color = new Color(0.6f, 0.2f, 0.9f); // Purple
        }

        // 2. Standard Limbs
        for (int i = 0; i < bonePairs.GetLength(0); i++)
        {
            limbs[i] = CreateBone("Limb_" + i);
        }

        // 3. Virtual Spine Bones
        spineBone = CreateBone("Virtual_Spine");
        lClavicle = CreateBone("Virtual_L_Clavicle");
        rClavicle = CreateBone("Virtual_R_Clavicle");
        lHipBone = CreateBone("Virtual_L_Hip");
        rHipBone = CreateBone("Virtual_R_Hip");
    }

    GameObject CreateBone(string name)
    {
        GameObject bone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bone.transform.parent = transform;
        bone.name = name;
        Destroy(bone.GetComponent<Collider>());
        bone.GetComponent<Renderer>().material = new Material(Shader.Find("Sprites/Default"));
        bone.GetComponent<Renderer>().material.color = Color.white;
        return bone;
    }

    void StartUDP()
    {
        try
        {
            udpClient = new UdpClient(port);
            isRunning = true;
            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e) { Debug.LogError("UDP Error: " + e.Message); }
    }

    void ReceiveData()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        while (isRunning)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEP);
                lastPacket = Encoding.UTF8.GetString(data);
                newDataAvailable = true;
            }
            catch { }
        }
    }

    void Update()
    {
        if (newDataAvailable)
        {
            newDataAvailable = false;
            ProcessJSON(lastPacket);
        }
    }

    void ProcessJSON(string json)
    {
        SkeletonMessage msg = JsonUtility.FromJson<SkeletonMessage>(json);
        if (msg == null || msg.joints == null) return;

        // 1. Update Joints
        float jSize = globalScale * jointSize;
        foreach (JointData j in msg.joints)
        {
            if (j.id >= 0 && j.id < 33)
            {
                joints[j.id].transform.localPosition = new Vector3(j.position.x, -j.position.y, j.position.z) * globalScale;
                joints[j.id].transform.localScale = Vector3.one * jSize;
                joints[j.id].SetActive(j.visibility > 0.5f);
            }
        }

        // 2. Update Standard Limbs
        float bThick = globalScale * boneThickness;
        for (int i = 0; i < bonePairs.GetLength(0); i++)
        {
            UpdateBone(limbs[i], joints[bonePairs[i, 0]], joints[bonePairs[i, 1]], bThick);
        }

        // 3. Update VIRTUAL SPINE (The "T-Shape")
        if (joints[11].activeSelf && joints[12].activeSelf && joints[23].activeSelf && joints[24].activeSelf)
        {
            Vector3 lShoulder = joints[11].transform.position;
            Vector3 rShoulder = joints[12].transform.position;
            Vector3 lHip = joints[23].transform.position;
            Vector3 rHip = joints[24].transform.position;

            Vector3 neck = (lShoulder + rShoulder) / 2f;
            Vector3 pelvis = (lHip + rHip) / 2f;

            // Draw Spine (Neck to Pelvis)
            UpdateVirtualBone(spineBone, neck, pelvis, bThick);

            // Draw Clavicles (Neck to Shoulders)
            UpdateVirtualBone(lClavicle, neck, lShoulder, bThick);
            UpdateVirtualBone(rClavicle, neck, rShoulder, bThick);

            // Draw Hips (Pelvis to Hips)
            UpdateVirtualBone(lHipBone, pelvis, lHip, bThick);
            UpdateVirtualBone(rHipBone, pelvis, rHip, bThick);
        }

        // 4. Update Dashboard
        if (dashboardText != null)
        {
            float lElbow = GetAngle(11, 13, 15);
            float rElbow = GetAngle(12, 14, 16);
            dashboardText.text = $"<b>SPINE TRACKER</b>\nL-Elbow: {lElbow:F0}°\nR-Elbow: {rElbow:F0}°";
        }
    }

    void UpdateBone(GameObject bone, GameObject A, GameObject B, float thickness)
    {
        if (A.activeSelf && B.activeSelf)
        {
            bone.SetActive(true);
            float len = Vector3.Distance(A.transform.position, B.transform.position);
            bone.transform.position = (A.transform.position + B.transform.position) / 2f;
            bone.transform.up = B.transform.position - A.transform.position;
            bone.transform.localScale = new Vector3(thickness, len * 0.5f, thickness);

            // Auto Color (Green/Red)
            if (A.name.Contains("_1") || A.name.Contains("_2")) bone.GetComponent<Renderer>().material.color = Color.green; // Right side logic simplified
            else bone.GetComponent<Renderer>().material.color = Color.white;
        }
        else bone.SetActive(false);
    }

    void UpdateVirtualBone(GameObject bone, Vector3 start, Vector3 end, float thickness)
    {
        bone.SetActive(true);
        float len = Vector3.Distance(start, end);
        bone.transform.position = (start + end) / 2f;
        bone.transform.up = end - start;
        bone.transform.localScale = new Vector3(thickness, len * 0.5f, thickness);
        bone.GetComponent<Renderer>().material.color = Color.white; // Spine is White
    }

    float GetAngle(int a, int b, int c)
    {
        if (!joints[a].activeSelf || !joints[b].activeSelf || !joints[c].activeSelf) return 0f;
        return Vector3.Angle(joints[a].transform.position - joints[b].transform.position,
                             joints[c].transform.position - joints[b].transform.position);
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (udpClient != null) udpClient.Close();
        if (receiveThread != null) receiveThread.Abort();
    }
}