using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Tosox.MagRetentionReload.Fika;
using Tosox.MagRetentionReload.Patches;

namespace Tosox.MagRetentionReload
{
    [BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
    [BepInDependency(FikaGuid, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string FikaGuid = "com.fika.core";

        internal static ManualLogSource Log { get; private set; }

        internal static ConfigEntry<bool> RequireMaxMastery { get; private set; }

        internal void Awake()
        {
            Log = Logger;

            RequireMaxMastery = Config.Bind(
                "General",
                "Require Max Weapon Mastering",
                true,
                "Only retain magazines once the weapon's mastering is maxed out"
            );

            new ReloadRunPatch().Enable();
            new ReloadDropModPatch().Enable();
            new ReloadRaiseEventsPatch().Enable();
            new ReloadRollbackPatch().Enable();
            new ReloadUIContextPatch().Enable();

            if (Chainloader.PluginInfos.ContainsKey(FikaGuid))
                FikaSync.Init();

            Logger.LogInfo("Plugin loaded successfully");
        }
    }
}
