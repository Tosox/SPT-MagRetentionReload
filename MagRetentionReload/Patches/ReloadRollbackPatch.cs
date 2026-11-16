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
            if (!RetainedMagazines.Operations.TryGetValue(__instance, out var operations))
                return;

            // Undo in reverse, so an item moved aside only goes back once the magazine has left its spot
            for (var i = operations.Count - 1; i >= 0; i--)
                operations[i].RollBack();
            RetainedMagazines.Operations.Remove(__instance);
        }
    }
}
