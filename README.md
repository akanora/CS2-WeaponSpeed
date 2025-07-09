# Weapon Speed
A simple CounterStrikeSharp plugin that gives players a **customizable speed boost** when they fire specified weapons with optional **VIP support** and damage disabling features.

![Weapon Speed Demo](https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExNHF2djQ0cnJsc25peW1kcjdveWRhMXB0NWgzZm12cTJndWY0NjBvaSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/CkcM0UEwOXhtTE23FP/giphy.gif)
![Weapon Speed Effect](https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExanB3b2h1MWZwb3p5YXZqeHhqaG00c3gydmtpOHN5NGRwOGtwaWNraSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/CVp3lGr2MADllNKOGy/giphy.gif)

## Features
- Pushes players backward when firing specified weapons
- Custom per-weapon speed via config (`weapon_ak47:750`)
- VIP-exclusive speeds (`weapon_awp:1000` for VIPs only)
- Disable damage for selected weapons
- Configurable global default speed
- Live config reload via console command
- Customizable VIP flag (`@css/vip` by default)

## Installation
1. Download the plugin from the Releases section.
2. Unzip the archive.
3. Place the folder into:
`addons/counterstrikesharp/plugins/`

## Configuration
The config file is automatically created at:
`addons/counterstrikesharp/configs/plugins/WeaponSpeed/WeaponSpeed.json`

```json
{
  "WeaponSpeedForce": 700.0,
  "EnablePlugin": true,
  "WeaponList": [
    "weapon_deagle:800",
    "weapon_mp5sd:850"
  ],
  "VipWeaponList": [
    "weapon_taser:900",
    "weapon_awp:1000"
  ],
  "DisableDamageWeapons": [
    "weapon_taser"
  ],
  "EnableDamageControl": true,
  "VipFlag": "@css/vip"
}
```

### Config Options
- `WeaponSpeedForce`:	Default force if weapon-specific force not set
- `EnablePlugin`: Enables/disables the plugin entirely
- `WeaponList`: Weapons and their force (e.g. `"weapon_deagle:800"`)
- `VipWeaponList`: 	VIP-only weapons with speed boost
- `DisableDamageWeapons`: Weapons that deal no damage when fired
- `EnableDamageControl`: Enables the damage blocker for listed weapons
- `VipFlag`: 	Required flag for VIP speed boost (default: `@css/vip`)

## Commands
- `css_weaponspeed_reload_config` - Reloads the plugin configuration (requires @css/root permission)

## How it Works
Whenever a player fires a weapon listed in the config:
- They get **launched backward** based on the direction they're aiming.
- The launch force is either global (`WeaponSpeedForce`) or defined per-weapon (`weapon_name:speed`).
- If they are a VIP, they may get stronger launch effects depending on `VipWeaponList`.

## Requirements
- CounterStrikeSharp
- CS2 Server