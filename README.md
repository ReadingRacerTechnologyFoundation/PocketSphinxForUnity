# PocketSphinxForUnity

PocketSphinxForUnity is a Unity plugin that provides a managed C# interface for using
**PocketSphinx** and selected **SphinxBase** APIs inside Unity projects.

This project is developed and maintained by **The Reading Racer Technology Foundation**.

It enables **offline speech recognition** (no internet connection required) by embedding
PocketSphinx directly into your Unity application.

## ⚠️ Project Status

- Supported platforms: **Windows, macOS, Android, iOS**
- The software is provided **as-is**, without warranty
- This project is **not affiliated with** Carnegie Mellon University or the CMU Sphinx project

## 🎙 What is PocketSphinx?

PocketSphinx is an open-source speech recognition engine originally developed at
Carnegie Mellon University.

Unlike cloud-based speech services, PocketSphinx runs **entirely on-device**, making it
well suited for:

- Offline applications
- Privacy-sensitive environments
- Educational software
- Low-latency or constrained devices
- The FSG approach used here makes it ideal for word by word recognition

PocketSphinxForUnity exposes a subset of PocketSphinx and SphinxBase functionality in a
form that can be used directly from Unity C# scripts.

## 📦 What This Project Provides

- C# wrapper classes for PocketSphinx and SphinxBase
- Unity-friendly APIs for speech recognition
- Support for **Finite State Grammars (FSGs)**
- Example scenes demonstrating common usage patterns
- Two bundled speech recognition models (children & adult)

## 🚀 Getting Started

### Via the Unity Package Manager
1. Open your Unity project and navigate to Window > Package Manager.
2. Click the + (plus) icon in the top-left corner and select Add package from git URL....
3. Paste the following URL, which uses the path query parameter to point directly to your subfolder:
>https://github.com/ReadingRacerTechnologyFoundation/PocketSphinxForUnity.git?path=/Packages/com.readingracer.ps-unity

### Or Pull the git repo
All Required files are found under Packages/com.readingracer.ps-unity
**Make sure to update submodules**
```bash
git pull
git submodule update --init --recursive
```

### 📁 Automatic Folder Setup

ps-unity will automatically copy the files into your Assets Directory

-**`Assets/Resources/RRTF/ModelPaths.asset`**
  Contains where the model data is in storage. See InitModelPaths.cs for more info.

-**`Assets/StreamingAssets/ps-unity-modeldata`**
  These are all the model files data we provide you in order to start recognizing speech.

-**`Assets/ps_unity_settings.asset`**
  If you want to provide your own model data use this asset to signify you don't want the streaming assets data copied over.
  Then modify ModelPaths.asset to detail where the new model data is

#### Troubleshooting

- If you believe you somehow corrupted your Assets described in the Automatic Folder setup portion, then delete those files and they will be copied back.

---

### What words can be recognized?
- We currently only provide an Adult and Child speech recognition models
- Our default dictionary is not comprehensive. You may get an error/crash if the word is not found in the dictionary.
- To add to the default dictionary, Modify `Assets/StreamingAssets/ps-unity-modeldata/ps_all_english/lm/default_dictionary.dic`

---

### 🧹 Recommended `.gitignore` Entries (Windows)

When running on Windows, the following files are automatically generated to help debug
Finite State Grammars:

- `tempFSG`
- `tempFSGcloze`

---

### 🎬 Example Scenes

The `Packages/com.readingracer.ps-unity/Samples` folder contains two sample scenes demonstrating how to create and use
Finite State Grammars (FSGs) for structured speech recognition. These scenes also contain scripts like MicDevicePicker.cs and VolumeOutput
that show you how to choose a microphone and calculate the volume via the microphone management class MicController.cs.

**RecognizeSentence.scene**

Demonstrates creating an FSG where:

Words must be spoken in a specific order

Useful for guided reading or scripted prompts

**RecognizeRandomWords.scene**

Demonstrates creating an FSG where:

Words can be spoken in any order

Useful for exploratory or free-form recognition tasks

🧠 Speech Recognition Models

Two speech recognition models are included:

Children’s model

Adult model

You can see how to switch between models by examining the example scripts:

FollowAlong

FollowAlongRandomly

These scripts demonstrate how to configure recognizers to use different acoustic models
depending on your application.

## 📄 Licensing & Attribution

This project is licensed under the GNU LGPL v3 (or later).
See the LICENSE file for full terms.

This project makes use of the following third-party components:

PocketSphinx — BSD-2-Clause License

SphinxBase — BSD-2-Clause License

Full attribution and license details are available in THIRD_PARTY_LICENSES.txt.

### ❗ Disclaimer

This software is provided “AS IS”, without warranty of any kind.
The Reading Racer Technology Foundation assumes no responsibility for misuse, errors,
or unintended behavior.

### 🤝 Contributing

Contributions are welcome, but this project is maintained on a best-effort basis.
No guarantees are made regarding response time or feature acceptance.

# 🌐 Reading Racer Technology Foundation
PocketSphinxForUnity was originally developed to support speech recognition features
for an educational reading game called Reading Racer, available on iOS.

Thank you for using PocketSphinx for Unity developed by 
The Reading Racer Technology Foundation (RRTF). A non profit with the mission to promote and share Reading Racer’s speech engine and provide technical support to those building educational products.

To learn more visit Reading Racer's [website](https://www.readingracer.com/).