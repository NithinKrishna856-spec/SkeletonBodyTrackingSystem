# Skeleton Body Tracking System

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2021.3+-black.svg)](https://unity.com/)
[![Python](https://img.shields.io/badge/Python-3.8+-blue.svg)](https://www.python.org/)

Unity-based Skeleton Body Tracking system for real-time gesture visualization using MediaPipe and Orbbec Gemini 336L depth sensor. Designed for applications in VR gaming, rehabilitation, and motion analysis.

---

## 📋 Table of Contents

- [About the Project](#about-the-project)
- [Features](#features)
- [System Architecture](#system-architecture)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [How to Run](#how-to-run)
- [Project Structure](#project-structure)
- [Technologies Used](#technologies-used)
- [Achievements](#achievements)
- [Challenges](#challenges)
- [Future Work](#future-work)
- [License](#license)
- [References](#references)

---

## 🎯 About the Project

**Project Title:** Skeleton Body Tracking System  
**Submitted by:** Nithin Krishna Krishnappa  
**Institution:** New Jersey Institute of Technology  
**Course Title:** Master's Project (CS 700B 612)  
**Instructors:** Dr. Margarita Vinnikov, Dr. Roni Barak Ventura

### Abstract

This project focuses on the development of a "Skeleton Body Tracking System" utilizing the **Orbbec Gemini 336L depth sensor**. The system collects detailed joint data from human movement in real time to support applications such as: 

- 🎮 Virtual Reality (VR) gaming
- 🏥 Rehabilitation and physical therapy
- 📊 Motion analysis and biomechanics research

The system integrates a **Python backend** for pose estimation and UDP streaming, along with a **Unity frontend** for 3D visualization. Key features include low-latency networking, joint smoothing to reduce sensor jitter, and visualization capabilities using virtual bones (spine, clavicles, hips).

---

## ✨ Features

- ✅ Real-time skeleton tracking using **MediaPipe** and **Orbbec Gemini 336L**
- ✅ 33 skeletal landmarks with 3D position tracking
- ✅ Joint smoothing with low-pass filtering to reduce sensor jitter
- ✅ Real-time angle calculation for elbows and knees
- ✅ UDP networking for low-latency data transmission
- ✅ CSV logging for motion data analysis
- ✅ 3D visualization in Unity with customizable joint and bone rendering
- ✅ UI overlay for real-time joint angle feedback
- ✅ Modular architecture for future VR integration

---

## 🏗️ System Architecture

The Skeleton Body Tracking System consists of two main components:

### Python Backend

Handles motion data acquisition, processing, and transmission: 

1. **Sensor Setup**  
   - Initializes the Orbbec Gemini 336L depth sensor using `pyorbbecsdk`
   - Configures color and depth data streams for real-time analysis
   - Applies camera calibration for accurate 3D joint positions

2. **Pose Estimation**  
   - Uses MediaPipe's pose estimation library to detect 33 human skeletal landmarks
   - Assigns 3D positions (x, y, z) based on depth data using pinhole projection

3. **Joint Data Smoothing**  
   - Applies low-pass filter to reduce jitter: 
   ```
   P_smooth = (P_raw × α) + (P_prev × (1 - α))  where α = 0.5
   ```

4. **Angle Calculation**  
   - Calculates elbow and knee joint angles using vector mathematics

5. **UDP Networking**  
   - Transmits skeleton data as JSON packets to Unity via UDP (port 5065)

6. **CSV Logging**  
   - Press `S` key to start logging joint data and angles to CSV files

### Unity Frontend

Receives and visualizes skeleton data in real-time:

1. **UDP Receiver**  
   - `SkeletonDataReceiver. cs` listens on port 5065 for JSON packets
   - Deserializes data into `SkeletonMessage` structure

2. **Joint Visualization**  
   - Creates 33 sphere GameObjects representing joints
   - Dynamically adjusts joint size based on user-defined parameters

3. **Bone Visualization**  
   - Draws connections between joints using cylinder GameObjects
   - Interpolates virtual bones (spine, clavicles, hips) for clarity

4. **UI Overlay**  
   - Displays joint angles (e.g., elbows) for debugging and feedback

5. **Customization**  
   - Modify joint size, bone thickness, and global scale via Inspector

---

## 📦 Prerequisites

Before running this project, ensure you have: 

- **Unity Hub** (Latest version)
- **Unity Editor** (Version 2021.3 or later recommended)
- **Python 3.8+**
- **Git**
- **Orbbec Gemini 336L Depth Sensor** (for hardware testing)

---

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/NithinKrishna856-spec/SkeletonBodyTrackingSystem.git
cd SkeletonBodyTrackingSystem
```

### 2. Install Python Dependencies

```bash
pip install -r requirements.txt
```

**Required Python packages:**
- `mediapipe` - Real-time pose estimation
- `opencv-python` - Image processing
- `numpy` - Mathematical operations
- `pyorbbecsdk` - Orbbec sensor interface

### 3. Open in Unity

1. Open **Unity Hub**
2. Click **Add** → Select the `SkeletonBodyTrackingSystem` folder
3. Open the project with Unity Editor (2021.3+ recommended)

---

## ▶️ How to Run

**Important:** This project requires running **two components** in order: 
1. Python backend (for pose estimation and data streaming)
2. Unity frontend (for visualization)

### **Step 1: Start the Python Backend**

#### On Windows:
Double-click `start_backend.bat` or run in terminal:
```bash
start_backend.bat
```

#### On Mac/Linux:
```bash
./start_backend.sh
```

**Or manually:**
```bash
python main.py
```

**What to expect:**
- The Orbbec sensor will initialize
- MediaPipe will start pose estimation
- Skeleton data will be streamed via UDP on port 5065
- Console will show frame-by-frame processing

**Keyboard Controls (in Python backend):**
- Press `S` - Start CSV logging
- Press `Q` - Quit the application

**⚠️ Keep this window open while using Unity!**

---

### **Step 2: Start the Unity Frontend**

1. Open **Unity Hub**
2. Open the **SkeletonBodyTrackingSystem** project
3. Navigate to `Assets/Scenes/`
4. Open the `SkeletonTest` scene
5. Click the **Play** button

**What to expect:**
- Unity will connect to the Python backend via UDP
- Skeleton joints will appear as spheres
- Bones will connect the joints
- Joint angles will display in the UI overlay

---

### **Troubleshooting**

#### Python backend won't start: 
- **Check Python installation:** `python --version` (should be 3.8+)
- **Install dependencies:** `pip install -r requirements.txt`
- **Check sensor connection:** Ensure Orbbec Gemini 336L is plugged in

#### Unity shows no skeleton:
- **Verify Python is running:** Check that `main.py` is active
- **Check UDP port:** Default is 5065 (verify firewall settings)
- **Check console:** Look for connection errors in Unity Console

#### Skeleton is jittery:
- Adjust smoothing factor in `main.py` (default α = 0.5)
- Ensure good lighting conditions
- Check sensor positioning (1-3 meters from subject)

---

### **Quick Start Workflow**

```
1. Connect Orbbec sensor → 2. Run start_backend.bat → 3. Open Unity → 4. Press Play
```

**⚠️ Always start the Python backend BEFORE pressing Play in Unity!**

---

## 📂 Project Structure

```
SkeletonBodyTrackingSystem/
├── Assets/                     # Unity assets
│   ├── Scenes/                # Unity scenes (SkeletonTest)
│   ├── Scripts/               # C# scripts (SkeletonDataReceiver. cs)
│   ├── Materials/             # Materials for visualization
│   └── Prefabs/               # Reusable GameObjects
├── Packages/                   # Unity package dependencies
├── ProjectSettings/            # Unity project configuration
├── UserSettings/               # User-specific Unity settings
├── main.py                     # Python backend script
├── requirements.txt            # Python dependencies
├── README.md                   # Project documentation
├── LICENSE                     # MIT License
└── .gitignore                  # Git ignore rules
```

---

## 🛠️ Technologies Used

### Backend
- **Python 3.8+**
- **MediaPipe** - Pose estimation
- **OpenCV** - Image processing
- **NumPy** - Mathematical computations
- **pyorbbecsdk** - Orbbec sensor SDK

### Frontend
- **Unity Engine** (2021.3+)
- **C#** - Scripting language
- **Unity Input System**
- **TextMeshPro** - UI text rendering

### Hardware
- **Orbbec Gemini 336L Depth Sensor**

### Networking
- **UDP Protocol** - Low-latency data transmission (Port 5065)
- **JSON** - Data serialization format

---

## 🏆 Achievements

1. **Seamless Integration**  
   Successfully combined Python backend for pose estimation with Unity frontend for visualization with minimal latency

2. **Comprehensive Visualization**  
   Developed a complete skeleton representation including joints (spheres), limbs (cylinders), and virtual bones

3. **Modular Design**  
   Built a flexible pipeline that can be extended for VR applications and advanced motion analysis

4. **Real-time Performance**  
   Achieved smooth real-time tracking and visualization with joint smoothing and low-latency networking

---

## 🚧 Challenges

1. **Sensor Compatibility**  
   - Initial attempts with Azure Body Tracking and Nuitrack failed to support the Orbbec Gemini 336L
   - Solution: Implemented custom solution using Orbbec SDK and MediaPipe

2. **Network Communication**  
   - Debugging UDP pipeline required careful frame-level synchronization between Python and Unity

3. **Project Organization**  
   - File reorganization caused broken references in Unity, requiring additional setup time

---

## 🔮 Future Work

The modular architecture provides a foundation for future developers to: 

1. **VR Integration**  
   - Create immersive rehabilitation games using skeleton tracking data
   - Develop interactive virtual environments for physical therapy

2. **Machine Learning Analytics**  
   - Analyze CSV motion data to detect movement abnormalities
   - Predict movements or improve rehabilitation programs

3. **Multi-Sensor Support**  
   - Adapt the system for other depth sensors (e.g., Kinect, RealSense)
   - Support multiple simultaneous sensors for full-body capture

4. **Advanced Visualization**  
   - Add heat maps for joint stress analysis
   - Implement motion trails for movement pattern visualization

---

## 📄 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## 📚 References

### Libraries and Frameworks
- **MediaPipe** - Real-time pose estimation library  
  URL: [https://mediapipe.dev](https://mediapipe.dev)

- **OpenCV** - Python library for image processing  
  URL: [https://opencv.org](https://opencv.org)

- **NumPy** - Library for matrix and vector calculations  
  URL:  [https://numpy.org](https://numpy.org)

- **pyorbbecsdk** - SDK for interfacing with Orbbec sensors  
  URL: [https://orbbec3d.com](https://orbbec3d.com)

### Hardware
- **Orbbec Gemini 336L Depth Sensor**  
  URL: [https://orbbec3d.com/product/gemini](https://orbbec3d.com/product/gemini)

### Development Environment
- **Unity Engine** - 3D visualization and interactive environment platform  
  URL: [https://unity.com](https://unity.com)