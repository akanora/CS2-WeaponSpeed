using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;

namespace WeaponSpeed;

public class WeaponSpeedConfig : BasePluginConfig
{
    [JsonPropertyName("WeaponSpeedForce")] public float WeaponSpeedForce { get; set; } = 700f;
    [JsonPropertyName("EnablePlugin")] public bool EnablePlugin { get; set; } = true;
    [JsonPropertyName("WeaponList")] public string[] WeaponList { get; set; } = new string[] { "weapon_taser" };
}

[MinimumApiVersion(80)]
public class WeaponSpeed : BasePlugin, IPluginConfig<WeaponSpeedConfig>
{
    public override string ModuleName => "Weapon Speed";
    public override string ModuleVersion => "1.0";
    
    public WeaponSpeedConfig Config { get; set; } = new();
    
    private static readonly string[] PossibleProps = { "EyeRotation", "EyeAngles", "ViewAngles", "AbsRotation" };

    public void OnConfigParsed(WeaponSpeedConfig config)
    {
        if (config.WeaponSpeedForce < 0)
        {
            config.WeaponSpeedForce = 700f;
        }
        
        if (config.WeaponList == null || config.WeaponList.Length == 0)
        {
            config.WeaponList = new string[] { "weapon_taser" };
        }
        
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
    }

    private HookResult OnWeaponFire(EventWeaponFire eventInfo, GameEventInfo info)
    {
        if (!Config.EnablePlugin)
        {
            return HookResult.Continue;
        }

        var player = eventInfo.Userid;
        if (player == null || !player.IsValid || player.IsBot)
        {
            return HookResult.Continue;
        }

        if (!Config.WeaponList.Contains(eventInfo.Weapon))
        {
            return HookResult.Continue;
        }

        var pawn = player.PlayerPawn?.Value;
        if (pawn == null)
        {
            return HookResult.Continue;
        }

        QAngle angles = default!;
        bool foundAngle = false;
        
        foreach (var propName in PossibleProps)
        {
            var prop = pawn.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                var val = prop.GetValue(pawn);
                if (val is QAngle qa)
                {
                    angles = qa;
                    foundAngle = true;
                    break;
                }
            }
        }

        if (!foundAngle)
        {
            return HookResult.Continue;
        }

        Vector impulseDirection = AngleToForwardWithInvertedPitch(angles) * -1f;
        Vector currentVelocity = new Vector(pawn.Velocity.X, pawn.Velocity.Y, pawn.Velocity.Z);
        
        Vector impulse = new Vector(
            impulseDirection.X * Config.WeaponSpeedForce,
            impulseDirection.Y * Config.WeaponSpeedForce,
            impulseDirection.Z * Config.WeaponSpeedForce
        );
        
        Vector newVelocity = new Vector(
            currentVelocity.X + impulse.X,
            currentVelocity.Y + impulse.Y,
            currentVelocity.Z + impulse.Z
        );

        pawn.Teleport(null, null, newVelocity);
        return HookResult.Continue;
    }

    private Vector AngleToForwardWithInvertedPitch(QAngle angles)
    {
        double pitchRad = (-angles.X) * Math.PI / 180.0;
        double yawRad = angles.Y * Math.PI / 180.0;

        float x = (float)(Math.Cos(pitchRad) * Math.Cos(yawRad));
        float y = (float)(Math.Cos(pitchRad) * Math.Sin(yawRad));
        float z = (float)(Math.Sin(pitchRad));

        return new Vector(x, y, z);
    }

    [ConsoleCommand("css_weaponspeed_reload_config", "Reloads the weapon speed plugin config")]
    [RequiresPermissions("@css/admin")]
    public void OnReloadConfig(CCSPlayerController? player, CommandInfo commandInfo)
    {
        commandInfo.ReplyToCommand($"Weapon Speed Force before reload: {Config.WeaponSpeedForce}");
        commandInfo.ReplyToCommand($"Weapons before reload: [{string.Join(", ", Config.WeaponList)}]");
        
        try
        {
            var newConfig = ConfigManager.Load<WeaponSpeedConfig>("WeaponSpeed");
            OnConfigParsed(newConfig);
            
            commandInfo.ReplyToCommand($"Weapon Speed Force after reload: {Config.WeaponSpeedForce}");
            commandInfo.ReplyToCommand($"Weapons after reload: [{string.Join(", ", Config.WeaponList)}]");
            commandInfo.ReplyToCommand($"Config loaded successfully!");
        }
        catch (Exception ex)
        {
            commandInfo.ReplyToCommand($"Error loading config: {ex.Message}");
        }
    }
}