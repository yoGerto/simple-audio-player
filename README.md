# Simple Audio Player for VRChat
A Utility script for VRChat world creation that makes playing Audio files easy and simple.#
This script is intended to be used with either a Pickup or Interact object, and supports both options natively.

Currently supports multiple modes of audio playback, selectable from within the editor.

## Installation / How to add to project
1. Download the latest release from the [Releases](https://github.com/yoGerto/simple-audio-player/releases) page.
2. With your Unity project open, double click the .unityproject file. This will bring up a window showing what files will be added. Click Import.
You should now have a folder called SimpleAudioPlayer in your Assets folder.
4. Click the object in the Hierarchy that you wish to add the script to. Drag and drop the SimpleAudioPlayer script onto the inspector for that object.
5. Enter play mode, then exit play mode. The script's UI should now be visible.

## Implementation notes
The sounds are played through a child object that is created within the parent object. This approach was taken because I wanted to use the Manual Sync mode, which is much faster and responsive when handling small updates.
