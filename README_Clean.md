
Understand Combining AR Training System

Project Category: AR interaction simulation
Unity 2023, AR Foundation, DOTween, TextMeshPro
Developer: Aaniket Darve
Build Goal: Android (ARCore)
Repository: github.com/USERNAME/PROJECT-NAME

Overview

Using augmented reality, this app mimics the working of a breakaway coupling (male–female quick disconnect).

It offers two modes of learning:

Method | Description
Guided Mode | Interactive, visual training with hints, arrows, and highlights, step by step.
Evaluation Method | Actions are recorded and tallied for performance review during freestyle work with no clues.

The encounter shows how industrial AR training runs—placement, guided actions, feedback, and logging—work.

Features

Detection and positioning of AR planes
Realistic coupling animations—connect, flow, stop, detach.
Sound and tactile reaction
Session logging: locally stored JSON
Summary screen shows time and the number of operations.
Step-by-step guided instructions
Free communication assessment method
Constant player logname with password
Cross-platform ready design

Application Flow

1. Login Screen: Type in your name and password, then choose either Guided or Assessment.
2. Guided Approach
   - Every action is hinted at in steps.
   - Execute: Place → Connect → Flow → Stop → Detach
   - Highlights and visual arrows direct you
   - Session summary appears immediately upon finishing.
3. Assessment Approach
   - Not any clues
   - Execute all tasks by oneself.
   - You get a performance summary when you push Go Back.
4. Summary Panel
   - Displays player name, mode, length, and operation counts.
   - Hit OK to go back to login.

Building the APK

1. Open project in Unity 2023.1+
2. Go to Build Settings → Android → Switch Platform
3. Under XR Plug-in Management, turn on ARCore.
4. Select either Build or Build and Run.
5. Move APK to an Android device then install.

Required Permissions: Camera

Using (Android)

1. Open the application
2. Type your Name and Password
3. Select Assessment or Guided.
4. Change the position of your phone to find a surface; planes will become visible.
5. Tap to put the coupling
6. Connect, Flow, Stop, Detach using on-screen buttons.
7. Look for sound feedback, flow effect, and animations.
8. Choose Go Back or complete Guided steps; view session summary
9. Select OK to go back to login.

Session Logging

Every run produces a JSON log at:
Android/data/com.company.projectname/files/

Example JSON:

{
  "playerName": "Aaniket",
  "mode": "Guided",
  "startUtc": "2025-10-23T14:20:00Z",
  "endUtc": "2025-10-23T14:23:12Z",
  "durationSec": 192.3,
  "placements": 1,
  "connects": 1,
  "flowStarts": 1,
  "flowStops": 1,
  "detaches": 1
}
