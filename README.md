# PocketSphinxForUnity

PocketSphinxForUnity is a Unity plugin that provides a managed C# interface for using
**PocketSphinx** and selected **SphinxBase** APIs inside Unity projects.

This project is developed and maintained by **The Reading Racer Technology Foundation**.

It enables **offline speech recognition** (no internet connection required) by embedding
PocketSphinx directly into your Unity application.

---

## ⚠️ Project Status

- Supported platforms: **Windows, macOS, Android, iOS**
- Some platforms have **not been tested recently**
- The software is provided **as-is**, without warranty
- This project is **not affiliated with** Carnegie Mellon University or the CMU Sphinx project

---

## 🎙 What is PocketSphinx?

PocketSphinx is an open-source speech recognition engine originally developed at
Carnegie Mellon University.

Unlike cloud-based speech services, PocketSphinx runs **entirely on-device**, making it
well suited for:

- Offline applications
- Privacy-sensitive environments
- Educational software
- Low-latency or constrained devices

PocketSphinxForUnity exposes a subset of PocketSphinx and SphinxBase functionality in a
form that can be used directly from Unity C# scripts.

---

## 📦 What This Project Provides

- C# wrapper classes for PocketSphinx and SphinxBase
- Unity-friendly APIs for speech recognition
- Support for **Finite State Grammars (FSGs)**
- Example scenes demonstrating common usage patterns
- Two bundled speech recognition models (children & adult)

---

## 🚀 Getting Started

### 📁 Required Folders & Files

You **must** include the following folders in your Unity project:

- **`Sphinx/`**  
  Contains the PocketSphinx and SphinxBase C# wrapper code.

- **`StreamingAssets/`**  
  Contains speech recognition models and related data.

> ⚠️ **Important:**  
> The `StreamingAssets` folder **must be placed directly under**:
>
> ```
> Assets/StreamingAssets
> ```
>
> Do not rename or nest this folder. Model loading depends on this exact structure,
> especially on Android and iOS.

---

### 🖥 Windows-Specific Setup

If you wish to test or run on **Windows**, you must also include the following native
libraries in your Unity project (as this repository does):

- `EasyCallWrapper.dll`
- `pocketsphinx.dll`
- `sphinxbase.dll`

These files must be available to Unity at runtime.

---

### 🧹 Recommended `.gitignore` Entries (Windows)

When running on Windows, the following files are automatically generated to help debug
Finite State Grammars:

- `tempFSG`
- `tempFSGcloze`

These files should not be committed. Add them to your `.gitignore`:

```gitignore
tempFSG*
tempFSGcloze*
🎬 Example Scenes

The Examples folder contains two sample scenes demonstrating how to create and use
Finite State Grammars (FSGs) for structured speech recognition.

SeaShellsRecognizerExample

Demonstrates creating an FSG where:

Words must be spoken in a specific order

Useful for guided reading or scripted prompts

SeaShellsRecognizerRandomOrder

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

📘 Dictionary Requirement

All words you want to recognize must exist in the dictionary.

Default dictionary: DefaultStoryList.dic

Words can be added if necessary

If a word is missing from the dictionary, PocketSphinx cannot recognize it

This is a core limitation of dictionary-based speech recognition systems.

🛠 Troubleshooting
❗ Speech is not recognized

Most common cause: incorrect microphone

The system always uses the default OS microphone

Verify:

The correct microphone is set as default

Unity has microphone permission

No other application is exclusively using the microphone

❗ Some words are never recognized

Second most common cause: missing dictionary entries

Ensure all words in your grammar exist in DefaultStoryList.dic

Add missing words with appropriate phonetic spellings

🌐 About the Foundation

PocketSphinxForUnity was originally developed to support speech recognition features
for an educational reading game.

To learn more about The Reading Racer Technology Foundation or the game that inspired
this API, visit:

👉 https://www.readingracer.com

📄 Licensing & Attribution

This project is licensed under the GNU LGPL v3 (or later).
See the LICENSE file for full terms.

This project makes use of the following third-party components:

PocketSphinx — BSD-2-Clause License

SphinxBase — BSD-2-Clause License

Full attribution and license details are available in THIRD_PARTY_LICENSES.txt.

❗ Disclaimer

This software is provided “AS IS”, without warranty of any kind.
The Reading Racer Technology Foundation assumes no responsibility for misuse, errors,
or unintended behavior.

🤝 Contributing

Contributions are welcome, but this project is maintained on a best-effort basis.
No guarantees are made regarding response time or feature acceptance.