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
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using System.Reflection;
using static CounterStrikeSharp.API.Core.Listeners;

namespace WeaponSpeed;

public class WeaponSpeedConfig : BasePluginConfig
{
    [JsonPropertyName("WeaponSpeedForce")] public float WeaponSpeedForce { get; set; } = 700f;
    [JsonPropertyName("EnablePlugin")] public bool EnablePlugin { get; set; } = true;
    [JsonPropertyName("WeaponList")] public string[] WeaponList { get; set; } = new string[] { "weapon_deagle:800" };
    [JsonPropertyName("VipWeaponList")] public string[] VipWeaponList { get; set; } = new string[] { "weapon_taser:900" };
    [JsonPropertyName("DisableDamageWeapons")] public string[] DisableDamageWeapons { get; set; } = new string[] { "weapon_taser" };
    [JsonPropertyName("EnableDamageControl")] public bool EnableDamageControl { get; set; } = true;
    [JsonPropertyName("VipFlag")] public string VipFlag { get; set; } = "@css/vip";
}

[MinimumApiVersion(80)]
public class WeaponSpeed : BasePlugin, IPluginConfig<WeaponSpeedConfig>
{
    public override string ModuleName => "Weapon Speed";
    public override string ModuleVersion => "1.2";

    public WeaponSpeedConfig Config { get; set; } = new();

    private Dictionary<string, float> ParsedWeaponList = new();
    private Dictionary<string, float> ParsedVipWeaponList = new();

    private static readonly string[] PossibleProps = { "EyeRotation", "EyeAngles", "ViewAngles", "AbsRotation" };

    public void OnConfigParsed(WeaponSpeedConfig config)
    {
        Config = config;
        ParsedWeaponList = ParseWeaponSpeedList(Config.WeaponList);
        ParsedVipWeaponList = ParseWeaponSpeedList(Config.VipWeaponList);
    }

    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventWeaponFire>(OnWeaponFire);

        if (Config.EnableDamageControl)
        {
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
        }
    }

    public override void Unload(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
    }

    private Dictionary<string, float> ParseWeaponSpeedList(string[] list)
    {
        var dict = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in list)
        {
            var parts = entry.Split(':', 2);
            var weaponName = parts[0].Trim().ToLower();

            float speed = Config.WeaponSpeedForce;
            if (parts.Length == 2 && float.TryParse(parts[1], out var parsedSpeed))
            {
                speed = parsedSpeed;
            }

            dict[weaponName] = speed;
        }

        return dict;
    }

    private bool TryGetWeaponSpeed(CCSPlayerController player, string weaponName, out float speed)
    {
        weaponName = weaponName.ToLower();

        if (ParsedWeaponList.TryGetValue(weaponName, out speed))
        {
            return true;
        }

        if (ParsedVipWeaponList.TryGetValue(weaponName, out speed) &&
            AdminManager.PlayerHasPermissions(player, Config.VipFlag))
        {
            return true;
        }

        speed = 0f;
        return false;
    }

    private HookResult OnTakeDamage(DynamicHook hook)
    {
        if (!Config.EnableDamageControl)
        {
            return HookResult.Continue;
        }

        if (hook.GetParam<CEntityInstance>(0).DesignerName is not "player")
        {
            return HookResult.Continue;
        }

        CTakeDamageInfo info = hook.GetParam<CTakeDamageInfo>(1);
        CBaseEntity? weapon = info.Ability.Value;

        if (weapon == null)
            return HookResult.Continue;

        string weaponName = GetDesignerName(weapon.As<CBasePlayerWeapon>());
        if (Config.DisableDamageWeapons.Contains(weaponName))
        {
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    private string GetDesignerName(CBasePlayerWeapon weapon)
    {
        if (weapon?.As<CCSWeaponBase>().VData is not { } weaponVData)
            return string.Empty;

        return weaponVData.Name;
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

        if (!TryGetWeaponSpeed(player, eventInfo.Weapon, out float weaponSpeed))
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
            impulseDirection.X * weaponSpeed,
            impulseDirection.Y * weaponSpeed,
            impulseDirection.Z * weaponSpeed
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
    [RequiresPermissions("@css/root")]
    public void OnReloadConfig(CCSPlayerController? player, CommandInfo commandInfo)
    {
        commandInfo.ReplyToCommand($"Reloading config...");

        try
        {
            var newConfig = ConfigManager.Load<WeaponSpeedConfig>("WeaponSpeed");
            OnConfigParsed(newConfig);

            commandInfo.ReplyToCommand($"✔ Config reloaded successfully.");
            commandInfo.ReplyToCommand($"• Default Speed: {Config.WeaponSpeedForce}");
            commandInfo.ReplyToCommand($"• WeaponList: [{string.Join(", ", Config.WeaponList)}]");
            commandInfo.ReplyToCommand($"• VipWeaponList: [{string.Join(", ", Config.VipWeaponList)}]");
            commandInfo.ReplyToCommand($"• DisableDamageWeapons: [{string.Join(", ", Config.DisableDamageWeapons)}]");
            commandInfo.ReplyToCommand($"• VipFlag: {Config.VipFlag}");
        }
        catch (Exception ex)
        {
            commandInfo.ReplyToCommand($"❌ Error loading config: {ex.Message}");
        }
    }
}