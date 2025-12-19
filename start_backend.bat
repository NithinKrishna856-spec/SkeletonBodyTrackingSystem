@echo off
title Skeleton Body Tracking - Python Backend
color 0A
echo ========================================
echo  Skeleton Body Tracking System
echo  Python Backend
echo ========================================
echo. 
echo Starting MediaPipe pose estimation...
echo.
echo Press 'S' to start logging
echo Press 'Q' to quit
echo. 
python main.py
if errorlevel 1 (
    echo.
    echo ERROR:  Failed to start Python backend!
    echo Please check:
    echo   1. Python is installed
    echo   2. Dependencies are installed: pip install -r requirements.txt
    echo   3. Orbbec sensor is connected
    pause
)