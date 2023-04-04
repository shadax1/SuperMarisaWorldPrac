# SuperMarisaWorldPrac
Download the latest release from [here](https://github.com/shadax1/SuperMarisaWorldPrac/releases) and add the .exe file into your Super Marisa World game folder. Start the game followed bt the practice tool and you will be good to go.

**Note:** your game must be at version 1.05

## Potential issues with stage detection
When Marisa is in a stage, the tool will attempt to scan for the memery address that stores in which stage Marisa is as it can change from a PC to another.
In case of a successful scan, then the tool will display where Marisa is at the bottom of the tool and unlock the save state feature. If it fails, it's recommended to launch the game with Windows 7 compatibility.
If it still fails after that then there isn't much I can do aside from telling you to follow [this short guide](https://youtu.be/xBSTnnGhPpo) I made which requires Cheat Engine (I use version 6.3) in an effort to find the correct pointer. Take a screenshot of the final screen in Cheat Engine and send it over to me either on Twitter [@shadax1](https://twitter.com/Shadax1) or on Discord to Shadax#6921

## Pointers and memory addresses used
Using the pointer `"SuperMarisaWorld.exe"+00155438` found with Cheat Engine, the following offsets are used to do various read and writes:
```csharp
STATE_OFFSET = { -0x30 }
SCREEN_ID_OFFSET = { 0x14 }
PIPE_OFFSET = { 0xADC }
X_OFFSET = { 0x1C }
Y_OFFSET = { 0x28 }
LIVES_OFFSET = { 0xA88 }
STARS_OFFSET = { 0xA8C }
TIME_OFFSET = { 0xA98 }
SCORE_OFFSET = { 0xA9C }
HIGHSCORE_OFFSET = { 0xAA0 }
ANIMATION_OFFSET = { 0x50 }
POWERUP_OFFSET = { 0x54 }
GROUNDED_FLAG_OFFSET = { 0x74 }
PSPEED_OFFSET = { 0x88 }
FLIGHT_OFFSET = { 0x90 }
IFRAMES_OFFSET = { 0x94 }
```

Then, with the pointer `"SuperMarisaWorld.exe"+0015542C` found with Cheat Engine, the following array of offsets is used to read which stage Marisa is located in:
```csharp
STAGE_ID_OFFSET = { 0x2C8, 0x108, 0x47 }
```
Note that the first value in that array can change a bit from a PC to another. The tool tries multiple values upon launch to try to find potential stage ID values.
