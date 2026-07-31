using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using System.Threading.Tasks;

namespace Tosox.MagRetentionReload.Server;

[Injectable]
public class RetainedReloadCallbacks(RetainedReloadController retainedReloadController)
{
    public async ValueTask<ItemEventRouterResponse> HandleRetainedReload(PmcData pmcData, RetainedReloadModel? body, string sessionId)
    {
        return await retainedReloadController.Handle(pmcData, body, sessionId);
    }
}
