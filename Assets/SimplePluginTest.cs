using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class SimplePluginTest : MonoBehaviour
{
    // ========================================================================
    // STRUCT MATCHING C++ (must have identical memory layout)
    // ========================================================================
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3D
    {
        public float x;
        public float y;
        public float z;
    }

    // ========================================================================
    // P/INVOKE DECLARATIONS (tells Unity how to call C++ functions)
    // ========================================================================
    private const string DLL_NAME = "SimplePlugin";

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern void Initialize();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetVersion();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern float AddNumbers(float a, float b);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern float MultiplyNumbers(float a, float b);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetCallCount();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ResetCounter();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern int FillFloatArray([Out] float[] outArray, int maxSize);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern void GetTestVector(out Vector3D outVector);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetLastMessage();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern void SetMessage(string message);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    private static extern void Shutdown();

    // ========================================================================
    // HELPER: Convert IntPtr (char*) to C# string
    // ========================================================================
    private string PtrToString(IntPtr ptr)
    {
        return Marshal.PtrToStringAnsi(ptr);
    }

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================
    void Start()
    {
        Debug.Log("=== SimplePlugin Test Started ===");

        // Test 1: Initialize
        try
        {
            Initialize();
            Debug.Log("✓ Initialize() called");
        }
        catch (Exception e)
        {
            Debug.LogError($"✗ Initialize() failed: {e.Message}");
            return;
        }

        // Test 2: Get version string
        try
        {
            string version = PtrToString(GetVersion());
            Debug.Log($"✓ GetVersion(): {version}");
        }
        catch (Exception e)
        {
            Debug.LogError($"✗ GetVersion() failed: {e.Message}");
        }

        // Test 3: Math operations
        try
        {
            float sum = AddNumbers(10.5f, 3.2f);
            Debug.Log($"✓ AddNumbers(10.5, 3.2) = {sum}");

            float product = MultiplyNumbers(4.0f, 7.0f);
            Debug.Log($"✓ MultiplyNumbers(4.0, 7.0) = {product}");
        }
        catch (Exception e)
        {
            Debug.LogError($"✗ Math operations failed: {e.Message}");
        }

        // Test 4: Call counter (stateful)
        try
        {
            int count = GetCallCount();
            Debug.Log($"✓ GetCallCount() = {count} (should be 1 from AddNumbers)");
        }
        catch (Exception e)
        {
            Debug.LogError($"✗ GetCallCount() failed: {e.Message}");
        }

        // Test 5: Array marshalling
        try
        {
            float[] testArray = new float[10];
            int filled = FillFloatArray(testArray, testArray.Length);
            Debug.Log($"✓ FillFloatArray() returned {filled} elements:");
            for (int i = 0; i < filled; i++)
            {
                Debug.Log($"  [{i}] = {testArray[i]}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"✗ FillFloatArray() failed: {e.Message}");
        }

        // Test 6: Struct marshalling
        try
        {
            Vector3D vec;
            GetTestVector(out vec);
            Debug.Log($"✓ GetTestVector(): ({vec.x:F3}, {vec.y:F3}, {vec.z:F3})");
        }
        catch (Exception e)
        {
            Debug.LogError($"✗ GetTestVector() failed: {e.Message}");
        }

        // Test 7: String marshalling
        try
        {
            SetMessage("Hello from Unity!");
            string msg = PtrToString(GetLastMessage());
            Debug.Log($"✓ GetLastMessage(): \"{msg}\"");
        }
        catch (Exception e)
        {
            Debug.LogError($"✗ String marshalling failed: {e.Message}");
        }

        Debug.Log("=== All Tests Complete ===");
    }

    void OnDestroy()
    {
        try
        {
            Shutdown();
            Debug.Log("✓ Shutdown() called");
        }
        catch (Exception e)
        {
            Debug.LogError($"✗ Shutdown() failed: {e.Message}");
        }
    }

    // ========================================================================
    // OPTIONAL: UI BUTTONS FOR MANUAL TESTING
    // ========================================================================
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Label("SimplePlugin Test Controls", new GUIStyle { fontSize = 16, fontStyle = FontStyle.Bold });

        if (GUILayout.Button("Reset Counter"))
        {
            ResetCounter();
            Debug.Log($"Counter reset. New count: {GetCallCount()}");
        }

        if (GUILayout.Button("Add 5 + 3"))
        {
            float result = AddNumbers(5, 3);
            Debug.Log($"5 + 3 = {result}");
        }

        if (GUILayout.Button("Get Vector"))
        {
            Vector3D vec;
            GetTestVector(out vec);
            Debug.Log($"Vector: ({vec.x:F3}, {vec.y:F3}, {vec.z:F3})");
        }

        if (GUILayout.Button("Show Call Count"))
        {
            Debug.Log($"Total calls: {GetCallCount()}");
        }

        GUILayout.EndArea();
    }
}