using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using Tosox.MagRetentionReload.Services;

namespace Tosox.MagRetentionReload.Patches
{
    public class ReloadUIContextPatch : ModulePatch
    {
        private static readonly FieldInfo fTraderController =
            AccessTools.Field(typeof(ItemUiContext), "traderControllerClass");

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(ItemUiContext),
                nameof(ItemUiContext.ReloadWeapon)
            );
        }

        [PatchPrefix]
        public static bool Prefix(ItemUiContext __instance, Weapon weapon, IEnumerable<CompoundItem> collections)
        {
            var traderController = (TraderControllerClass)fTraderController.GetValue(__instance);
            return ReloadUIContextHandler.Handle(__instance, weapon, collections, traderController);
        }
    }
}
