using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tosox.MagRetentionReload.Server;

[Injectable]
public class RetainedReloadEventRouter(RetainedReloadCallbacks retainedReloadCallbacks) : ItemEventRouterDefinition
{
    public override async ValueTask<ItemEventRouterResponse> HandleItemEvent(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        MongoId sessionId,
        ItemEventRouterResponse output)
    {
        if (url == RetainedReloadController.Route)
        {
            return await retainedReloadCallbacks.HandleRetainedReload(pmcData, body as RetainedReloadModel, sessionId);
        }

        throw new Exception($"RetainedReloadEventRouter cannot handle route {url}");
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return [new(RetainedReloadController.Route, false)];
    }

    protected override ValueTask<ItemEventRouterResponse> HandleItemEventInternal(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        MongoId sessionId,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }
}
