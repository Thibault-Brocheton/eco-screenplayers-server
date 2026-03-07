# 🎬 Eco Screen Players

This mod has been developed for Eco version **v12.0.0**

Play your favorite videos and music directly in Eco! This mod adds televisions, projectors, radios and music players to the game, allowing players to share audio and video content with their friends.

## ✨ Features

**New craftable objects:**

| Object | Type | Skill | Crafting Table | Power |
|---|---|---|---|---|
| Radio | Music | Basic Engineering 2 | Wainwright Table | 10w Mechanical |
| Record Player | Music | Basic Engineering 2 | Wainwright Table | 10w Mechanical |
| JukeBox | Music | Mechanics 2 | Assembly Line | 200w Mechanical |
| Cathode Ray Television | Video | Mechanics 2 | Assembly Line | 300w Mechanical |
| Flat Screen Television | Video | Electronics 2 | Electronics Assembly | 100w Electric |
| Video Projector | Video/Cinema | Electronics 2 | Electronics Assembly | 300w Electric |
| Cinema Projector | Video/Cinema | Electronics 3 | Electronics Assembly | 600w Electric |

**Playback controls:**
- Start / Stop and Pause / Resume via UI buttons or right-click interactions
- Adjustable volume (0–100) and max sound distance (0–32 blocks)
- Projectors have an adjustable projection distance setting
- URL field to link any `.mp4` video

**Web uploader:**
- Built-in web interface accessible from the Eco server web panel
- Players can upload `.mp4` and `.mp3` files directly
- `.mp3` files are automatically converted to `.mp4` using FFmpeg
- Admin validation system: optionally require admin approval before files are available

**Housing values:**
- All objects provide housing value (Living Room or Cultural category)
- Projectors use Light Bulbs as replaceable parts

**Vehicle support:**
- Optional connector for the [HotWheels](https://mod.io/g/eco/m/hotwheels) mod — adds a screen to the Tesla Model 3

## ⚙️ Configuration

The plugin configuration is editable from the server admin panel under **Mods > Screen Players**.

| Option | Default | Description |
|---|---|---|
| `EnableWebUploader` | `true` | Enable or disable the web upload interface |
| `RequireValidation` | `false` | When enabled, uploaded files must be validated by an admin before use |
| `AllowOnlyLocalUrl` | `false` | Restrict URL field to only accept URLs from the local server |
| `MaxUploadPerUser` | `5` | Maximum number of files a player can upload |
| `MaxFileSizeInMB` | `15` | Maximum file size for uploads (in MB) |

## 💻 Installation

1. Download the `CavRnMods` folder and copy it inside your `Mods/UserCode` folder in your Eco server.
2. If you want vehicle screen support with the [HotWheels](https://mod.io/g/eco/m/hotwheels) mod, also include `HotWheelsConnector.cs`. Otherwise, don't add it.
3. *(Optional)* Install [FFmpeg](https://ffmpeg.org/) on your server to enable `.mp3` upload support.
4. Restart your server.

## 📋 Requirements

- Eco v12.0.0+
- *(Optional)* FFmpeg — required for `.mp3` to `.mp4` conversion
- *(Optional)* [HotWheels](https://mod.io/g/eco/m/hotwheels) mod — for vehicle screen support