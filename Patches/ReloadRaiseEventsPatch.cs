using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.State;

namespace Tosox.MagRetentionReload.Patches
{
    public class ReloadRaiseEventsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(Player.FirearmController.GClass2006),
                nameof(Player.FirearmController.GClass2006.RaiseEvents)
            );
        }

        [PatchPostfix]
        public static void PatchPostfix(Player.FirearmController.GClass2006 __instance, TraderControllerClass controller, CommandStatus status)
        {
            // Forward events to handle the Add-result
            if (MagRetentionState.RetainedMagazine.TryGetValue(__instance, out var retainedMagazine))
            {
                retainedMagazine.RaiseEvents(controller, status);
            }
        }
    }
}
