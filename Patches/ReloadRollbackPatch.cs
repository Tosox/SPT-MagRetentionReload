using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.State;

namespace Tosox.MagRetentionReload.Patches
{
    public class ReloadRollbackPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(Player.FirearmController.GClass2006),
                nameof(Player.FirearmController.GClass2006.RollBack)
            );
        }

        [PatchPrefix]
        public static void Prefix(Player.FirearmController.GClass2006 __instance)
        {
            if (!MagRetentionState.RetainedMagOps.TryGetValue(__instance, out var op))
                return;

            op.RollBack();
            MagRetentionState.RetainedMagOps.Remove(__instance);
        }
    }
}
