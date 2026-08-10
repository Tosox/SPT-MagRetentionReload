using BepInEx;
using Tosox.MagRetentionReload.Patches;

namespace Tosox.MagRetentionReload
{
    [BepInPlugin("de.tosox.magretentionreload", PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        internal const string PluginName = "Mag Retention Reload";
        internal const string PluginVersion = "1.2.0";
        internal const string PluginAuthor = "Tosox";
        internal const string PluginSource = "https://github.com/Tosox/SPT-MagRetentionReload";

        internal void Awake()
        {
            new ReloadRunPatch().Enable();
            new ReloadDropModPatch().Enable();
            new ReloadRaiseEventsPatch().Enable();
            new ReloadRollbackPatch().Enable();
            new ReloadUIContextPatch().Enable();

            Logger.LogInfo("Plugin loaded successfully");
        }
    }
}
