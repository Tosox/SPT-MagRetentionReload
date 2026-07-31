using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.RetainedReload;

namespace Tosox.MagRetentionReload.Patches
{
    public class ConvertOperationResultToOperationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(TraderControllerClass),
                nameof(TraderControllerClass.ConvertOperationResultToOperation)
            );
        }

        [PatchPrefix]
        public static bool Prefix(
            TraderControllerClass __instance,
            IRaiseEvents operationResult,
            ref BaseInventoryOperationClass __result)
        {
            if (!(operationResult is RetainedReloadResult retainedReloadResult))
            {
                return true;
            }

            __result = new RetainedReloadOperation(__instance.method_12(), __instance, retainedReloadResult);
            return false;
        }
    }
}
