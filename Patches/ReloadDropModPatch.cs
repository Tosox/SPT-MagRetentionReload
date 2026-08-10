using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.State;

namespace Tosox.MagRetentionReload.Patches
{
    internal class ReloadDropModPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(Player.FirearmController.GClass2016),
                nameof(Player.FirearmController.GClass2016.DropMod)
            );
        }

        [PatchPrefix]
        public static bool Prefix(Player.FirearmController.GClass2016 __instance, Item droppedMod, EWeaponModType modType)
        {
            if (modType != EWeaponModType.mod_magazine || droppedMod == null)
                return true;

            var cmd = __instance.Gclass2006_0;
            if (cmd?.RemoveOldMagResult == null || cmd.RemoveOldMagResult.Item != droppedMod)
                return true;

            // Stops EFT from also spawning the retained magazine on the ground
            return !MagRetentionState.RetainedMagOps.TryGetValue(cmd, out _);
        }
    }
}
