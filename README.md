# SuperMarisaWorldPrac
Download the latest release from [here](https://github.com/shadax1/SuperMarisaWorldPrac/releases) and add the .exe file into your Super Marisa World game folder. Start the game followed by the practice tool and you will be good to go.

**Note:** your game must be at version 1.05

![demo pic](https://raw.githubusercontent.com/shadax1/SuperMarisaWorldPrac/master/demo%20pic.png)

## Potential issues with stage detection
When Marisa is in a stage, the tool will attempt to scan for the memory address that stores which stage Marisa is in as it can change from one PC to another.
In case of a successful scan, then the tool will display where Marisa is at the bottom of the window and unlock the save state feature. If it fails, it is recommended to launch the game with Windows 7 compatibility.
If it still fails after that then there isn't much I can do aside from recommending you to follow [this short guide](https://youtu.be/xBSTnnGhPpo) I made which requires Cheat Engine (I use version 6.3) in an effort to find the correct pointer. Take a screenshot of the final screen in Cheat Engine and send it over to me either on Twitter [@shadax1](https://twitter.com/Shadax1) or on Discord to Shadax#6921

## Pointer addresses and offsets used
Using the pointer address `"SuperMarisaWorld.exe"+00155438` found with Cheat Engine, the following offsets are used to do various reads and writes:
```csharp
STATE_OFFSET = { -0x30 }
SCREEN_ID_OFFSET = { 0x14 }
X_OFFSET = { 0x1C }
Y_OFFSET = { 0x28 }
ANIMATION_OFFSET = { 0x50 }
POWERUP_OFFSET = { 0x54 }
RUMIA_OFFSET = { 0x5C }
GROUNDED_FLAG_OFFSET = { 0x74 }
PSPEED_OFFSET = { 0x88 }
FLIGHT_OFFSET = { 0x90 }
IFRAMES_OFFSET = { 0x94 }
STOPWATCH_OFFSET = { 0x94C }
LIVES_OFFSET = { 0xA88 }
STARS_OFFSET = { 0xA8C }
TIME_OFFSET = { 0xA98 }
SCORE_OFFSET = { 0xA9C }
HIGHSCORE_OFFSET = { 0xAA0 }
PIPE_OFFSET = { 0xADC }
```

Then, with the pointer address `"SuperMarisaWorld.exe"+0015542C` found with Cheat Engine, the following array of offsets is used to read which stage Marisa is located in:
```csharp
STAGE_ID_OFFSET = { 0x2C8, 0x108, 0x47 }
```
As explained above, the first value in that array can change a bit from one PC to another. The tool tries multiple offsets upon launch to try to find the correct one.
