using BepInEx;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.Configuration;

namespace Tosox.MagRetentionReload.Compatibility
{
    /// <summary>
    /// Takes magazine retention over from UI Fixes, which reloads magazines in place out of the box.
    /// This mod answers for the feature as a whole while it is enabled, the reloads it decides not to
    /// retain included, so that a weapon below the required mastering is not retained by them instead.
    /// </summary>
    internal static class UiFixes
    {
        // Their reload key handler
        private static MethodBase _swapIfNoSpacePrefix;

        // Their inventory context menu reload. The patch methods that are left out only ever act on
        // the state ReloadInPlacePatch.Prefix sets up, so stopping that one stops them along with it.
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

            // All of them or none, because suppressing a part of their reload would leave the two mods
            // sharing a feature that neither of them handles end to end
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

        /// <summary>
        /// Their reload key handler answers whether the game's own reload may run, so stopping it has
        /// to answer in its place. Staying silent would read as a no and swallow the reload entirely.
        /// </summary>
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

        /// <summary>
        /// Stops their context menu reload from starting, which leaves it to the game and therefore to
        /// this mod. Turning this mod off lets their patch through again untouched.
        /// </summary>
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

        /// <summary>
        /// Stops them from freeing up the new magazine's spot while the game looks for one
        /// </summary>
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

        /// <summary>
        /// Stops them from putting that magazine back afterwards
        /// </summary>
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

        /// <summary>
        /// Stops them from sorting the new magazine's own spot to the front of the candidates
        /// </summary>
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
