# Weapon Speed
A simple CounterStrikeSharp plugin that gives players a speed boost when they fire specified weapons.

![Weapon Speed Demo](https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExNHF2djQ0cnJsc25peW1kcjdveWRhMXB0NWgzZm12cTJndWY0NjBvaSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/CkcM0UEwOXhtTE23FP/giphy.gif)
![Weapon Speed Effect](https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExanB3b2h1MWZwb3p5YXZqeHhqaG00c3gydmtpOHN5NGRwOGtwaWNraSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/CVp3lGr2MADllNKOGy/giphy.gif)

## Features
- Pushes players backward when firing configured weapons
- Configurable force strength
- Support for multiple weapons via config array
- Easy to enable/disable via config

## Installation
1. Download the plugin from release
2. Unzip it and place the folder in `addons/counterstrikesharp/plugins/`

## Configuration
The plugin creates a config file at:
`addons/counterstrikesharp/configs/plugins/WeaponSpeed/WeaponSpeed.json`

```json
{
  "WeaponSpeedForce": 700.0,
  "EnablePlugin": true,
  "WeaponList": [
    "weapon_taser",
    "weapon_deagle"
  ]
}
```

### Config Options
- `WeaponSpeedForce`: How strong the push force is (default: 700)
- `EnablePlugin`: Enable or disable the plugin (true/false)
- `WeaponList`: Array of weapons that trigger the speed boost (default: ["weapon_taser"])

## Commands
- `css_weaponspeed_reload_config` - Reloads the plugin configuration (requires admin)

## How it Works
When a player fires any weapon listed in the `WeaponList` config, they get pushed in the opposite direction of where they're aiming. The force can be adjusted in the config file, and you can add or remove weapons from the list as needed.

## Requirements
- CounterStrikeSharp
- CS2 Server