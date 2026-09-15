using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using EFT.UI;
using Tosox.MagRetentionReload.Commands;
using Tosox.MagRetentionReload.Compatibility;
using Tosox.MagRetentionReload.Configuration;
using Tosox.MagRetentionReload.Fika;
using Tosox.MagRetentionReload.Patches;

namespace Tosox.MagRetentionReload
{
    [BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
    [BepInDependency(FikaGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(UiFixesGuid, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string FikaGuid = "com.fika.core";
        private const string UiFixesGuid = "com.tyfon.uifixes";

        internal static ManualLogSource Log { get; private set; }

        internal void Awake()
        {
            Log = Logger;
            Settings.Init(Config);

            ConsoleScreen.Processor.RegisterCommandGroup<DebugCommands>();

            new ReloadRunPatch().Enable();
            new ReloadDropModPatch().Enable();
            new ReloadRaiseEventsPatch().Enable();
            new ReloadRollbackPatch().Enable();
            new ReloadUIContextPatch().Enable();

            if (Chainloader.PluginInfos.ContainsKey(FikaGuid))
                FikaSync.Init();

            if (Chainloader.PluginInfos.TryGetValue(UiFixesGuid, out var uiFixes))
                UiFixes.Init(uiFixes);

            Logger.LogInfo("Plugin loaded successfully");
        }
    }
}
