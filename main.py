import cv2
import sys
import numpy as np
import mediapipe as mp
import socket
import json
import time
import csv
import datetime
import math
from pyorbbecsdk import *

# --- CONFIGURATION ---
UDP_IP = "127.0.0.1"
UDP_PORT = 5065
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# --- SMOOTHING CONFIGURATION ---
# 0.0 = No movement (Frozen), 1.0 = Raw Data (Jittery)
# 0.5 is a good balance for Rehab.
SMOOTHING_FACTOR = 0.5 

# MediaPipe Setup
mp_pose = mp.solutions.pose
pose = mp_pose.Pose(
    static_image_mode=False,
    model_complexity=1,
    smooth_landmarks=True,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

# Global dictionary to store previous frame's joint positions
previous_joints = {}

# --- HELPER: 3D ANGLE CALCULATION ---
def calculate_angle_3d(a, b, c):
    """
    Calculates the angle at point B formed by A-B-C in 3D space.
    a, b, c are dictionaries {'x': val, 'y': val, 'z': val}
    """
    p1 = np.array([a['x'], a['y'], a['z']])
    p2 = np.array([b['x'], b['y'], b['z']]) 
    p3 = np.array([c['x'], c['y'], c['z']])
    
    ba = p1 - p2
    bc = p3 - p2
    
    cosine_angle = np.dot(ba, bc) / (np.linalg.norm(ba) * np.linalg.norm(bc))
    cosine_angle = np.clip(cosine_angle, -1.0, 1.0)
    angle = np.arccos(cosine_angle)
    return np.degrees(angle)

def main():
    global previous_joints
    print("--- TELEREHAB RESEARCH TRACKER (WITH SMOOTHING) ---")
    print(f"Smoothing Factor: {SMOOTHING_FACTOR}")
    print("Commands:")
    print("  [S] - Start/Stop CSV Recording")
    print("  [Q] - Quit")
    
    # 1. Initialize SDK
    try:
        ctx = Context()
        device_list = ctx.query_devices()
        if device_list.get_count() == 0:
            print("Error: No Orbbec device found.")
            return
        device = device_list.get_device_by_index(0)
        pipeline = Pipeline(device)
        config = Config()
        
        try:
            profile_list = pipeline.get_stream_profile_list(OBSensorType.COLOR_SENSOR)
            color_profile = profile_list.get_default_video_stream_profile()
            config.enable_stream(color_profile)
            color_w, color_h = color_profile.get_width(), color_profile.get_height()
        except: pass

        try:
            profile_list = pipeline.get_stream_profile_list(OBSensorType.DEPTH_SENSOR)
            depth_profile = profile_list.get_default_video_stream_profile()
            config.enable_stream(depth_profile)
            depth_w, depth_h = depth_profile.get_width(), depth_profile.get_height()
        except: pass

        pipeline.start(config)
        
    except Exception as e:
        print(f"Init Failed: {e}")
        return

    # Intrinsics
    try:
        cam_param = pipeline.get_camera_param()
        fx = cam_param.rgb_intrinsic.fx
        fy = cam_param.rgb_intrinsic.fy
        cx = cam_param.rgb_intrinsic.cx
        cy = cam_param.rgb_intrinsic.cy
    except:
        fx, fy, cx, cy = 600.0, 600.0, 640.0, 360.0

    # --- RECORDING STATE ---
    is_recording = False
    csv_file = None
    csv_writer = None
    frame_idx = 0

    try:
        while True:
            frames = pipeline.wait_for_frames(100)
            if frames is None: continue

            color_frame = frames.get_color_frame()
            depth_frame = frames.get_depth_frame()
            if color_frame is None: continue

            # Decode Images
            raw_data = np.frombuffer(color_frame.get_data(), dtype=np.uint8)
            if len(raw_data) != color_h * color_w * 3:
                bgr_image = cv2.imdecode(raw_data, cv2.IMREAD_COLOR)
                if bgr_image is None: continue
                color_image = cv2.cvtColor(bgr_image, cv2.COLOR_BGR2RGB)
            else:
                color_image = raw_data.reshape((color_h, color_w, 3))
                bgr_image = cv2.cvtColor(color_image, cv2.COLOR_RGB2BGR)

            depth_image = None
            if depth_frame is not None:
                d_data = np.frombuffer(depth_frame.get_data(), dtype=np.uint16)
                depth_image = d_data.reshape((depth_h, depth_w))

            # MediaPipe Processing
            results = pose.process(color_image)
            skeleton_data = {"frame": frame_idx, "joints": []}
            landmarks_3d = {} # Current Frame Data

            if results.pose_landmarks:
                # Draw landmarks on the python window for debug
                mp.solutions.drawing_utils.draw_landmarks(
                    bgr_image, results.pose_landmarks, mp_pose.POSE_CONNECTIONS)

                for idx, landmark in enumerate(results.pose_landmarks.landmark):
                    cx_px = int(landmark.x * color_w)
                    cy_px = int(landmark.y * color_h)

                    # Get Depth
                    depth_z = 1.5 
                    if depth_image is not None:
                        dx_px = int(cx_px * (depth_w / color_w))
                        dy_px = int(cy_px * (depth_h / color_h))
                        if 0 <= dx_px < depth_w and 0 <= dy_px < depth_h:
                            raw_mm = depth_image[dy_px, dx_px]
                            if raw_mm > 0: depth_z = raw_mm / 1000.0

                    # Pinhole Projection (Raw)
                    raw_x = (cx_px - cx) * depth_z / fx
                    raw_y = (cy_px - cy) * depth_z / fy
                    
                    # --- SMOOTHING ALGORITHM ---
                    if idx in previous_joints:
                        prev = previous_joints[idx]
                        # Apply Low-Pass Filter
                        smooth_x = (raw_x * SMOOTHING_FACTOR) + (prev['x'] * (1.0 - SMOOTHING_FACTOR))
                        smooth_y = (raw_y * SMOOTHING_FACTOR) + (prev['y'] * (1.0 - SMOOTHING_FACTOR))
                        smooth_z = (depth_z * SMOOTHING_FACTOR) + (prev['z'] * (1.0 - SMOOTHING_FACTOR))
                    else:
                        smooth_x, smooth_y, smooth_z = raw_x, raw_y, depth_z

                    # Store for next frame
                    current_pos = {"x": smooth_x, "y": smooth_y, "z": smooth_z}
                    previous_joints[idx] = current_pos
                    landmarks_3d[idx] = current_pos
                    
                    # Add to JSON
                    skeleton_data["joints"].append({
                        "id": idx,
                        "position": current_pos,
                        "visibility": landmark.visibility
                    })

                # --- CALCULATE ANGLES & RECORD (Using SMOOTHED Data) ---
                if is_recording and len(landmarks_3d) > 28:
                    try:
                        l_elbow_ang = calculate_angle_3d(landmarks_3d[11], landmarks_3d[13], landmarks_3d[15])
                        r_elbow_ang = calculate_angle_3d(landmarks_3d[12], landmarks_3d[14], landmarks_3d[16])
                        l_knee_ang = calculate_angle_3d(landmarks_3d[23], landmarks_3d[25], landmarks_3d[27])
                        r_knee_ang = calculate_angle_3d(landmarks_3d[24], landmarks_3d[26], landmarks_3d[28])

                        timestamp = datetime.datetime.now().strftime("%H:%M:%S.%f")[:-3]
                        csv_writer.writerow([timestamp, frame_idx, 
                                             f"{l_elbow_ang:.1f}", f"{r_elbow_ang:.1f}", 
                                             f"{l_knee_ang:.1f}", f"{r_knee_ang:.1f}"])
                    except: pass

            # UDP Send
            sock.sendto(json.dumps(skeleton_data).encode(), (UDP_IP, UDP_PORT))
            frame_idx += 1

            # --- UI FEEDBACK ---
            if is_recording:
                cv2.circle(bgr_image, (30, 30), 10, (0, 0, 255), -1)
                cv2.putText(bgr_image, "REC", (50, 40), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)
            else:
                cv2.putText(bgr_image, "Press 'S' to Record", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)

            cv2.imshow("Smoothed Research Tracker", bgr_image)
            
            key = cv2.waitKey(1)
            if key in [ord('q'), 27]: break
            elif key == ord('s'):
                if is_recording:
                    is_recording = False
                    if csv_file: csv_file.close()
                    print("--- Saved CSV ---")
                else:
                    timestamp_str = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
                    filename = f"Rehab_Data_{timestamp_str}.csv"
                    csv_file = open(filename, 'w', newline='')
                    csv_writer = csv.writer(csv_file)
                    csv_writer.writerow(["Timestamp", "Frame", "L_Elbow", "R_Elbow", "L_Knee", "R_Knee"])
                    is_recording = True
                    print(f"--- Recording to {filename} ---")

    except KeyboardInterrupt: pass
    finally:
        if csv_file: csv_file.close()
        pipeline.stop()

if __name__ == "__main__":
    main()