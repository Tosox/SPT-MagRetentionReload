using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Tosox.MagRetentionReload.Patches
{
    public class ReloadUIContextPatch : ModulePatch
    {
        private static readonly FieldInfo fFoundMagazine = 
            AccessTools.Field(typeof(ItemUiContext.Class2939), nameof(ItemUiContext.Class2939.foundMagazine));

        private static readonly FieldInfo fMagSlotAddress =
            AccessTools.Field(typeof(ItemUiContext.Class2939), nameof(ItemUiContext.Class2939.magSlotAddress));

        private static readonly FieldInfo fTraderController = 
            AccessTools.Field(typeof(ItemUiContext), "traderControllerClass");

        private static readonly MethodInfo mSwap =
            AccessTools.Method(typeof(InteractionsHandlerClass),
                nameof(InteractionsHandlerClass.Swap),
                new[]
                {
                    typeof(Item),
                    typeof(ItemAddress),
                    typeof(Item),
                    typeof(ItemAddress),
                    typeof(TraderControllerClass),
                    typeof(bool)
                });

        private static readonly MethodInfo mChooseToLocation =
            AccessTools.Method(typeof(ReloadUIContextPatch), nameof(ChooseToLocation));

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(ItemUiContext),
                nameof(ItemUiContext.ReloadWeapon)
            );
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerPatch(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var matcher = new CodeMatcher(instructions, il);

            matcher.MatchForward(false,
                new CodeMatch(ci => ci.Calls(mSwap)));
            if (!matcher.IsValid)
            {
                Logger.LogWarning("Did not find Swap call");
                return instructions;
            }

            // From the call position, match backwards to:
            //     ldloc.3
            //     ldloc.0
            //     ldfld foundMagazine
            matcher.MatchBack(false,
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(ci => ci.opcode == OpCodes.Ldfld && Equals(ci.operand, fFoundMagazine)));
            if (!matcher.IsValid)
            {
                Logger.LogWarning("Did not find Swap arg2 pattern (ldloc.3; ldloc.0; ldfld foundMagazine).");
                return instructions;
            }

            // We are at ldloc.3 now.
            // Replace ldloc.3 with:
            //   dup
            //   ChooseToLocation(currentMagazine, originalResultAddress, foundMagazine, magSlotAddress, traderControllerClass)
            matcher.SetOpcodeAndAdvance(OpCodes.Dup);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldloc_3),                  // originalResultAddress
                new CodeInstruction(OpCodes.Ldloc_0),                  // @class
                new CodeInstruction(OpCodes.Ldfld, fFoundMagazine),    // foundMagazine
                new CodeInstruction(OpCodes.Ldloc_0),                  // @class
                new CodeInstruction(OpCodes.Ldfld, fMagSlotAddress),   // magSlotAddress
                new CodeInstruction(OpCodes.Ldarg_0),                  // this
                new CodeInstruction(OpCodes.Ldfld, fTraderController), // traderControllerClass
                new CodeInstruction(OpCodes.Call, mChooseToLocation)); // -> ItemAddress

            return matcher.InstructionEnumeration();
        }

        private static ItemAddress ChooseToLocation(MagazineItemClass currentMagazine, GClass3393 originalResultAddress,
            MagazineItemClass foundMagazine, ItemAddress magSlotAddress, TraderControllerClass traderController)
        {
            var newMagAddress = foundMagazine.CurrentAddress;
            if (newMagAddress == null)
                return originalResultAddress;

            var swapResult = InteractionsHandlerClass.Swap(
                currentMagazine, newMagAddress, foundMagazine, magSlotAddress, traderController, true);
            if (swapResult.Failed)
                return originalResultAddress;

            return newMagAddress;
        }
    }
}
