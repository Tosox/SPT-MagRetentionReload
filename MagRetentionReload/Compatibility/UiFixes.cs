using BepInEx;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.Configuration;

namespace Tosox.MagRetentionReload.Compatibility
{
    internal static class UiFixes
    {
        private static MethodBase _inPlaceReloadPrefix;

        /// <summary>
        /// Takes magazine retention over from UI Fixes, which reloads magazines in place out of the box
        /// </summary>
        internal static void Init(PluginInfo pluginInfo)
        {
            if (pluginInfo?.Instance == null)
                return;

            _inPlaceReloadPrefix = pluginInfo.Instance.GetType().Assembly
                .GetType("UIFixes.ReloadInPlacePatches+SwapIfNoSpacePatch", false)
                ?.GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static);

            if (_inPlaceReloadPrefix == null)
            {
                Plugin.Log.LogWarning("UI Fixes is installed but its in-place reload could not be found!");
                return;
            }

            new InPlaceReloadPatch().Enable();
        }

        internal class InPlaceReloadPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return _inPlaceReloadPrefix;
            }

            [PatchPrefix]
            public static bool Prefix(ref bool __result)
            {
                if (!Settings.Enabled.Value)
                    return true;

                __result = true;
                return false;
            }
        }
    }
}
