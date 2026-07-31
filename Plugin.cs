using BepInEx;
using Tosox.MagRetentionReload.Patches;

namespace Tosox.MagRetentionReload
{
    [BepInPlugin("de.tosox.magretentionreload", PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginName = "Mag Retention Reload";
        public const string PluginVersion = "1.0.0";
        public const string PluginAuthor = "Tosox";
        public const string PluginSource = "https://github.com/Tosox/SPT-MagRetentionReload";

        public void Awake()
        {
            new ReloadRunPatch().Enable();
            new ReloadRaiseEventsPatch().Enable();
            new ReloadRollbackPatch().Enable();
            new ReloadUIContextPatch().Enable();
            new ConvertOperationResultToOperationPatch().Enable();

            Logger.LogInfo("Plugin loaded successfully");
        }
    }
}
