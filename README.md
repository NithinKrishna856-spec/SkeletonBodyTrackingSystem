# Skeleton Body Tracking System

## Title Page
**Project Title:** Skeleton Body Tracking System  
**Submitted by:** Nithin Krishna Krishnappa  
**Institution:** New Jersey Institute of Technology  
**Course Title:** Master's Project (CS 700B 612)  
**Instructors:** Dr. Margarita Vinnikov, Dr. Roni Barak Ventura  
**Date:** December 18, 2025  

---

## Abstract

This project focuses on the development of a "Skeleton Body Tracking System" utilizing the Orbbec Gemini 336L depth sensor. The goal was to collect detailed joint data from human movement in real time to support applications such as virtual reality (VR) gaming, rehabilitation, and motion analysis. The system integrates a Python backend for pose estimation and UDP streaming, along with a Unity frontend for 3D visualization.

Key features include low-latency networking, joint smoothing to reduce sensor jitter, and visualization capabilities using virtual bones (spine, clavicles, hips). The modular architecture ensures future extensibility for advanced analytics and immersive VR compatibility.

---

## Introduction

Skeleton tracking technology plays a pivotal role in applications like gaming, rehabilitation, and biomechanics research. By analyzing human motion data, such systems empower clinicians, developers, and researchers to explore movement patterns, design real-time VR experiences, and improve clinical assessments of range-of-motion.

For this master's project, the Orbbec Gemini 336L sensor was selected for its high-quality 3D depth data capture capabilities. The primary objective was to build a system capable of accurately collecting and visualizing skeleton motion data in real time. Using Python for backend data processing (mediating sensor input, pose estimation, and data transmission) and Unity for visualization, the project achieves a complete pipeline for capturing and displaying skeleton motion data.

---

## System Architecture

The Skeleton Body Tracking System is designed as a modular architecture composed of two main components: the Python backend and the Unity frontend.

### Python Backend
The backend handles motion data acquisition, processing, and transmission:
1. **Sensor Setup**:  
   The Orbbec Gemini 336L depth sensor is initialized using the `pyorbbecsdk` library. Color and depth data are configured for real-time analysis of human movement. Camera calibration is applied to output accurate 3D joint positions.  

2. **Pose Estimation**:  
   MediaPipe’s pose estimation library detects 33 human skeletal landmarks using RGB inputs. Each landmark is assigned a 3D position (x, y, z) based on depth data, derived with pinhole projection methods.

3. **Joint Data Smoothing**:  
   To reduce jitter caused by sensor noise, a low-pass filter is applied per joint:
   ```
   P_smooth = (P_raw × α) + (P_prev × (1 - α))
   ```
   where α = 0.5.

4. **Angle Calculation**:  
   Elbow and knee joint angles are calculated using vector mathematics for clinical feedback in rehabilitation settings.

5. **UDP Networking**:  
   Skeleton data is formatted as JSON packets and transmitted to Unity via UDP at port **5065**. Each frame contains joint IDs, positions, and visibility flags.

6. **CSV Logging**:  
   Operators can begin session logging with the `S` key, saving joint data and angles to a CSV file for further analysis.

---

### Unity Frontend
The frontend receives skeleton data and visualizes it in the `SkeletonTest` scene:
1. **UDP Setup**:  
   The Unity script `SkeletonDataReceiver.cs` listens on UDP port **5065** for JSON packets. Data is deserialized into a custom `SkeletonMessage` structure, containing joint positions and visibility flags.

2. **Joint Visualization**:  
   The `InitializeSkeleton()` function creates 33 sphere GameObjects representing joints. Joint size dynamically adjusts based on user-defined parameters (e.g., `globalScale`).

3. **Bone and Limb Visualization**:  
   Connections between joints are drawn using cylinder GameObjects to depict skeletal limbs. Virtual bones (spine, clavicles, hips) are interpolated for added clarity.

4. **UI Overlay**:  
   A dashboard displays joint angles (e.g., elbows) for debugging and user feedback.

5. **Customization**:  
   Users can modify visualization properties such as joint size, bone thickness, and global scale via the Inspector.

---

## Implementation Details

### Python Backend
- **Languages and Libraries**:  
  Python (`mediapipe`, `opencv-python`, `numpy`, `pyorbbecsdk`).
- **Key Script**:  
  `main.py`: Handles sensor input, pose estimation, smoothing, angle calculation, and UDP transmission.
- **UDP JSON Format**:  
  Each packet includes:
  ```json
  {
    "frame": 123,
    "joints": [
      {"id": 0, "position": {"x": 0.1, "y": 0.5, "z": 1.2}, "visibility": 0.9},
      ...
    ]
  }
  ```

---

### Unity Frontend
- **Languages and Libraries**:  
  Unity (C#). Key scripts: `SkeletonDataReceiver.cs`.  
- **Key Functionalities**:  
  - Receives and processes skeleton data.
  - Visualizes joints as spheres and limbs as cylinders.
  - Provides virtual bone interpolation for additional skeletal clarity.
- **Interactivity**:  
  Allows users to adjust visualization parameters and receive real-time angle feedback via the UI overlay.

---

## Achievements

The Skeleton Body Tracking System represents a fully functional pipeline for real-time motion tracking and visualization. Key accomplishments include:
1. **Seamless Integration**:  
   - Successfully combined a Python backend for pose estimation, joint smoothing, and UDP networking with a Unity frontend for skeleton visualization and interactivity.  
   - Achieved smooth communication between the components with minimal latency.

2. **Skeleton Representation**:  
   - Developed a comprehensive visualization system in Unity, including spheres (joints), cylinders (limbs), and virtual bones (spine, clavicles, hips).

3. **Modular Design**:  
   - Built a flexible and extensible pipeline that can serve as a foundation for integration with VR setups and further motion analysis.

---

## Challenges

Several challenges emerged during the development process:
1. **Sensor Compatibility**:  
   - Attempted integration with Azure Body Tracking and Nuitrack, both of which failed to support the Orbbec Gemini 336L sensor.  
   - Identified limited library support and eventually selected the functional Orbbec SDK and MediaPipe.

2. **Network Communication**:  
   - Debugging the UDP pipeline required careful handling to ensure frame-level consistency between Python and Unity.  

3. **Project Organization**:  
   - Reorganization of project files led to broken references in Unity, requiring additional effort to restore proper functionality.

---

## Future Work

While you do not plan to extend the Skeleton Body Tracking System yourself, the modular architecture provides a solid foundation for future developers to:
1. **Integrate VR Games and Features**:  
   - Create immersive rehabilitation games or virtual environments that use skeleton tracking data for interactivity.  

2. **Enhance Machine Learning Analytics**:  
   - Analyze motion data recorded in CSV logs to detect abnormalities, predict movements, or improve rehabilitation programs.

3. **Expand Sensor Compatibility**:  
   - Adapt the system for other sensors to broaden its applicability across different hardware setups.

---

## Conclusion

The Skeleton Body Tracking System provides a robust and modular solution for real-time motion tracking and visualization using the Orbbec Gemini 336L sensor. With Python powering pose estimation and UDP communication, and Unity facilitating 3D visualization, the project successfully achieves its goals of capturing, processing, and displaying skeleton data to support applications like gaming, rehabilitation, and motion analysis.

This work lays the groundwork for researchers and developers to create VR-capable games, analyze human movement data, and improve clinical assessments.

---

## References

1. **Libraries and Frameworks Used**:
   - MediaPipe: Real-time pose estimation library.  
     URL: [https://mediapipe.dev](https://mediapipe.dev)
   - OpenCV: Python library for image processing.  
     URL: [https://opencv.org](https://opencv.org)
   - NumPy: Library for matrix and vector calculations.  
     URL: [https://numpy.org](https://numpy.org)
   - pyorbbecsdk: SDK for interfacing with Orbbec sensors.  
     URL: [https://orbbec3d.com](https://orbbec3d.com)

2. **Hardware Used**:
   - Orbbec Gemini 336L Depth Sensor.  
     URL: [https://orbbec3d.com/product/gemini](https://orbbec3d.com/product/gemini)

3. **Development Environment**:
   - Unity Engine: 3D visualization and interactive environment platform.  
     URL: [https://unity.com](https://unity.com)