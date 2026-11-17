using BepInEx;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.Configuration;

namespace Tosox.MagRetentionReload.Compatibility
{
    // Also blocks reloads this mod declines, or UI Fixes would retain below the required mastering
    internal static class UiFixes
    {
        // Their reload key handler
        private static MethodBase _swapIfNoSpacePrefix;

        // Their context menu reload. The other patches only act on state this one sets up
        private static MethodBase _reloadInPlacePrefix;
        private static MethodBase _findSpotPrefix;
        private static MethodBase _findSpotPostfix;
        private static MethodBase _alwaysSwapPostfix;

        internal static void Init(PluginInfo pluginInfo)
        {
            if (pluginInfo?.Instance == null)
                return;

            var assembly = pluginInfo.Instance.GetType().Assembly;

            _swapIfNoSpacePrefix = Resolve(assembly, "SwapIfNoSpacePatch", "Prefix");
            _reloadInPlacePrefix = Resolve(assembly, "ReloadInPlacePatch", "Prefix");
            _findSpotPrefix = Resolve(assembly, "ReloadInPlaceFindSpotPatch", "Prefix");
            _findSpotPostfix = Resolve(assembly, "ReloadInPlaceFindSpotPatch", "Postfix");
            _alwaysSwapPostfix = Resolve(assembly, "AlwaysSwapPatch", "Postfix");

            // All or none, otherwise neither mod handles the reload end to end
            if (_swapIfNoSpacePrefix == null || _reloadInPlacePrefix == null || _findSpotPrefix == null
                || _findSpotPostfix == null || _alwaysSwapPostfix == null)
            {
                Plugin.Log.LogWarning("UI Fixes is installed but its in-place reload could not be found!");
                return;
            }

            new SwapIfNoSpacePrefixPatch().Enable();
            new ReloadInPlacePrefixPatch().Enable();
            new FindSpotPrefixPatch().Enable();
            new FindSpotPostfixPatch().Enable();
            new AlwaysSwapPostfixPatch().Enable();
        }

        private static MethodBase Resolve(Assembly assembly, string patch, string method)
        {
            return assembly.GetType("UIFixes.ReloadInPlacePatches+" + patch, false)
                ?.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
        }

        // Answer in their place, since no answer reads as a no and swallows the reload
        internal class SwapIfNoSpacePrefixPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return _swapIfNoSpacePrefix;
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

        internal class ReloadInPlacePrefixPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return _reloadInPlacePrefix;
            }

            [PatchPrefix]
            public static bool Prefix()
            {
                return !Settings.Enabled.Value;
            }
        }

        // Stops them freeing the new magazine's spot while the game looks for one
        internal class FindSpotPrefixPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return _findSpotPrefix;
            }

            [PatchPrefix]
            public static bool Prefix()
            {
                return !Settings.Enabled.Value;
            }
        }

        // Stops them putting that magazine back afterwards
        internal class FindSpotPostfixPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return _findSpotPostfix;
            }

            [PatchPrefix]
            public static bool Prefix()
            {
                return !Settings.Enabled.Value;
            }
        }

        // Stops them sorting the new magazine's spot to the front
        internal class AlwaysSwapPostfixPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return _alwaysSwapPostfix;
            }

            [PatchPrefix]
            public static bool Prefix()
            {
                return !Settings.Enabled.Value;
            }
        }
    }
}
