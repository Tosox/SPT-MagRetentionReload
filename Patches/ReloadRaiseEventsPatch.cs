using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.State;

namespace Tosox.MagRetentionReload.Patches
{
    internal class ReloadRaiseEventsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(Player.FirearmController.ReloadExternalMagResult),
                nameof(Player.FirearmController.ReloadExternalMagResult.RaiseEvents)
            );
        }

        [PatchPostfix]
        public static void PatchPostfix(Player.FirearmController.ReloadExternalMagResult __instance, ItemController controller, CommandStatus status)
        {
            if (!MagRetentionState.RetainedMagOps.TryGetValue(__instance, out var op))
                return;

            op.RaiseEvents(controller, status);
            if (status == CommandStatus.Succeed || status == CommandStatus.Failed)
                MagRetentionState.RetainedMagOps.Remove(__instance);
        }
    }
}
