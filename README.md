# SuperMarisaWorldPrac
Download the latest release from [here](https://github.com/shadax1/SuperMarisaWorldPrac/releases) and add the .exe file into your Super Marisa World game folder. Start the game followed by the practice tool and you will be good to go.

**Note:** your game must be at version 1.05

![demo pic](https://raw.githubusercontent.com/shadax1/SuperMarisaWorldPrac/master/demo%20pic.png)

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
SCOREBONUS_OFFSET = { 0xE4 }
BOMBTIMER_OFFSET = { 0xB00 }
BOMB_OFFSET = { 0xB04 }
STOPWATCH_OFFSET = { 0x94C }
LIVES_OFFSET = { 0xA88 }
STARS_OFFSET = { 0xA8C }
TIME_OFFSET = { 0xA98 }
SCORE_OFFSET = { 0xA9C }
HIGHSCORE_OFFSET = { 0xAA0 }
PIPE_OFFSET = { 0xADC }
```

Then, with the pointer address `"SuperMarisaWorld.exe"+00155424` found with Cheat Engine, the following array of offsets is used to read which stage Marisa is located in:
```csharp
STAGE_ID_OFFSET = { 0x80, 0x14, 0x68, 0x108, 0x47 }
```
