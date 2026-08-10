using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.State;

namespace Tosox.MagRetentionReload.Patches
{
    internal class ReloadRollbackPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(Player.FirearmController.ReloadExternalMagResult),
                nameof(Player.FirearmController.ReloadExternalMagResult.RollBack)
            );
        }

        [PatchPrefix]
        public static void Prefix(Player.FirearmController.ReloadExternalMagResult __instance)
        {
            if (!MagRetentionState.RetainedMagOps.TryGetValue(__instance, out var op))
                return;

            op.RollBack();
            MagRetentionState.RetainedMagOps.Remove(__instance);
        }
    }
}
