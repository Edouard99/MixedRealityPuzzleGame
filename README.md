## Table Of Contents
* [Introduction](#introduction)
* [Setup Unity and Azure](#setup-unity-and-azure)

## Introduction
This project aims to develop a puzzle game for Mixed Reality Environment as Hololens 2 (on which I have worked). This puzzle game challenges the user to solve 3d holographic puzzle game. There is 3 different level of difficulty. A tutorial is also implemented to teach the user how to interact with Holographic pieces.

<p align="center">
  <img alt="Game" title="Game" src="./Media/game_Moment.jpg" width="450">
</p>

As this game was used for data collection purpose, for each session (1/2/3) the game analytics are saved on the Hololens storage and on an Azure container using blobs (Thanks to <a href="https://github.com/Unity3dAzure/StorageServices"> this unity package</a>). The analytics collected are :
* Time
* Player Position and Rotation
* Puzzle ID
* Level of Difficulty
* Time to solve
* Puzzle state (every second)

## Setup Unity and Azure
If you want to use or modify the game :
1. Create a new 3d core Unity Project (version 2020.3.28f1 or above to avoid UnityWebRequest issue)
2. Setup your Project for Mixed Reality using <a href="https://github.com/microsoft/MixedRealityToolkit-Unity">MRTK</a>. This <a href="https://docs.microsoft.com/en-us/learn/modules/learn-mrtk-tutorials/1-3-exercise-configure-unity-for-windows-mixed-reality?tabs=openxr">tutorial</a> gives all the guideline to setup your project.
3. Then in Unity : Assets>Import Package>Custom Package... and select the Game.unitypackage
4. Open the scene "Game_MX" in the file scene.
- - - -
In our case we used Azure to load the puzzle file grid (that was stored on an azure container) feel free to load the file directly from the device by modifying code;
If you want to use this solution, you will need to configure Azure : 

5. Create/Sign in with an azure account
6. Go to Storage accounts and create a new storage account
    * Basics : Name : **mxdatacollection** - Region : your choice - Performance : your choice - Redundancy : your choice
    * Advanced : Require secure transfer for REST API operations : **DISABLED**
7. Once the storage account is created, click on it and create 2 new containers in it (choose **Blob** or **Container** for *Public Access Level*) named **data** and **puzzlejson**.

(Note that all proposed names can be changed but you will need to modify them in the c# script Read_Write_json.cs)
