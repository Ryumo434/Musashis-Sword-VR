This Repository is a VR Game written in C# in combination with Unity. It's part of a Study Project from our university (HWR Berlin).

## Introduction
This Game is a VR-Sword Game where the player needs to clear different levels. To be able to get to a new level, the player needs to kill every enemy in the previous one.

## Features

1. VR-Sword Combat with lightsaber (sword)
2. throw and recall the sword
3. different types of enemies
4. level based progress
5. small physical events (climbing, button press)

## Used Technology
- VR Meta Quest 3
- Unity
- XR Interaction Toolkit
- C#
- GitHub
- Local Storage

## Installation

It is planned to release it somehow in the Meta Horizon Store.
Nonetheless you can clone the Repository and set everything up yourself to play it. Follow these steps:

1. Get a Meta Quest 3
2. Activate the Developer Mode
3. Install Unity (Unity 2022.3.33f1 LTS)
4. Install Android SDK directly with Unity
5. Clone the Repository and open it with Unit
6. If not already satisfied with cloning the project import following packages:
   a. XR Interaction Toolkit
   b. XR Plugin Management
   c. XR Hands
7. Use following Project Settings:
   a. Color Space: Linear
   b. Minimum API Level: Android 10.0 (29)
   c. Active Input Handling: Both or New
8. In Project Settings Tab XR Plug-In Management select:
  a. Android and tick "OpenXR"
  b. in Tab OpenXR Add as Enabled Interaction Profile "Oculus Touch Controller Profile" and activate "Meta Quest Support" (only needed for building)
9. Use following Build Settings:
  a. Switch to Android
  b. Select all Scenes you want to build
  c. Select the VR as Default Device
10. Hit Build And Run

## How to Play
Use the VR Controller to Navigate. Swing your arm to use the sword and use the Grip-Button to hold things or recall the sword.
If you see a Push Button in the VR World use your hands to push it.

DO NOT WALK WHILE WEARING THE VR GLASSES. This can lead to serious injuries.

## Credtis

Mihoshi
Nasser
Lukas
