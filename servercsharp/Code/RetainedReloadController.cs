using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tosox.MagRetentionReload.Server;

[Injectable]
public class RetainedReloadController(
    ISptLogger<RetainedReloadController> logger,
    EventOutputHolder eventOutputHolder,
    HttpResponseUtil httpResponseUtil)
{
    public const string Route = "RetainedReload";

    public ValueTask<ItemEventRouterResponse> Handle(PmcData pmcData, RetainedReloadModel? body, string sessionId)
    {
        var output = eventOutputHolder.GetOutput(sessionId);

        if (body?.CurrentMagazine is null
            || body.ReplacementMagazine is null
            || body.WeaponSlot is null
            || body.RetainedSlot is null)
        {
            return ValueTask.FromResult(httpResponseUtil.AppendErrorToOutput(output, "Missing data in retained reload body"));
        }

        var items = pmcData.Inventory?.Items;
        if (items is null)
        {
            return ValueTask.FromResult(httpResponseUtil.AppendErrorToOutput(output, "PMC inventory items missing"));
        }

        if (items is not IList itemList)
        {
            return ValueTask.FromResult(httpResponseUtil.AppendErrorToOutput(output, "PMC inventory items list missing"));
        }

        var replacementMagazine = FindItemById(items, body.ReplacementMagazine);
        if (replacementMagazine is null)
        {
            return ValueTask.FromResult(httpResponseUtil.AppendErrorToOutput(output, "Could not find reload magazines in PMC inventory"));
        }

        try
        {
            MaterializeRetainedItems(itemList, replacementMagazine, body.RetainedItems);

            var currentMagazine = FindItemById(items, body.CurrentMagazine);
            if (currentMagazine is null)
            {
                return ValueTask.FromResult(httpResponseUtil.AppendErrorToOutput(output, "Could not find reload magazines in PMC inventory"));
            }

            ApplyAddress(replacementMagazine, body.WeaponSlot);
            ApplyAddress(currentMagazine, body.RetainedSlot);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to apply retained reload addresses: {ex}");
            return ValueTask.FromResult(httpResponseUtil.AppendErrorToOutput(output, "Failed to apply retained reload"));
        }

        return ValueTask.FromResult(output);
    }

    private void MaterializeRetainedItems(
        IList itemList,
        object prototypeItem,
        IReadOnlyCollection<RetainedReloadItemSnapshotModel>? retainedItems)
    {
        if (retainedItems == null || retainedItems.Count == 0)
        {
            return;
        }

        foreach (var snapshot in retainedItems)
        {
            if (snapshot?.ItemId is null || snapshot.TemplateId is null || snapshot.Address is null)
            {
                continue;
            }

            var existingItem = FindItemById(itemList, snapshot.ItemId);
            var item = existingItem ?? CreateItemLike(prototypeItem);

            SetProperty(item, snapshot.ItemId, "Id");
            SetProperty(item, snapshot.TemplateId, "TemplateId", "Tpl");
            ApplyAddress(item, snapshot.Address);
            ApplySnapshotState(item, prototypeItem, snapshot);

            if (existingItem == null)
            {
                itemList.Add(item);
            }
        }
    }

    private static void ApplyAddress(object item, RetainedReloadAddressModel address)
    {
        SetProperty(item, address.ParentItem, "ParentId");
        SetProperty(item, address.Container, "SlotId");

        var locationProperty = item.GetType().GetProperty("Location");
        if (locationProperty == null)
        {
            return;
        }

        if (address.Location == null || address.Location.Value.ValueKind == JsonValueKind.Null)
        {
            locationProperty.SetValue(item, null);
            return;
        }

        var locationValue = JsonSerializer.Deserialize(address.Location.Value.GetRawText(), locationProperty.PropertyType);
        locationProperty.SetValue(item, locationValue);
    }

    private static void ApplySnapshotState(object item, object prototypeItem, RetainedReloadItemSnapshotModel snapshot)
    {
        if (snapshot.StackCount == 1 && !snapshot.SpawnedInSession)
        {
            return;
        }

        var updProperty = item.GetType().GetProperty("Upd", BindingFlags.Public | BindingFlags.Instance);
        if (updProperty == null)
        {
            return;
        }

        var upd = updProperty.GetValue(item);
        if (upd == null)
        {
            var prototypeUpd = updProperty.GetValue(prototypeItem);
            upd = prototypeUpd != null ? CreateItemLike(prototypeUpd) : CreateUninitialized(updProperty.PropertyType);
            updProperty.SetValue(item, upd);
        }

        if (snapshot.StackCount != 1)
        {
            TrySetProperty(upd, snapshot.StackCount, "StackObjectsCount");
        }

        if (snapshot.SpawnedInSession)
        {
            TrySetProperty(upd, true, "SpawnedInSession");
        }
    }

    private static object? FindItemById(IEnumerable items, string itemId)
    {
        foreach (var item in items)
        {
            if (item != null && GetString(item, "Id") == itemId)
            {
                return item;
            }
        }

        return null;
    }

    private static string? GetString(object instance, string propertyName)
    {
        var value = instance.GetType().GetProperty(propertyName)?.GetValue(instance);
        return value?.ToString();
    }

    private static object CreateItemLike(object prototype)
    {
        return CreateUninitialized(prototype.GetType());
    }

    private static object CreateUninitialized(Type type)
    {
        var constructor = type.GetConstructor(Type.EmptyTypes);
        if (constructor != null)
        {
            return constructor.Invoke(null);
        }

        return RuntimeHelpers.GetUninitializedObject(type);
    }

    private static bool TrySetProperty(object instance, object? value, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                continue;
            }

            property.SetValue(instance, CoerceValue(property.PropertyType, value));
            return true;
        }

        return false;
    }

    private static void SetProperty(object instance, object? value, params string[] propertyNames)
    {
        if (!TrySetProperty(instance, value, propertyNames))
        {
            throw new InvalidOperationException(
                $"Property {string.Join(", ", propertyNames)} not found on {instance.GetType().FullName}"
            );
        }
    }

    private static object? CoerceValue(Type targetType, object? value)
    {
        if (value == null)
        {
            return null;
        }

        var valueType = value.GetType();
        if (targetType.IsAssignableFrom(valueType))
        {
            return value;
        }

        try
        {
            return JsonSerializer.Deserialize(JsonSerializer.Serialize(value), targetType);
        }
        catch
        {
        }

        var stringConstructor = targetType.GetConstructor(new[] { typeof(string) });
        if (stringConstructor != null)
        {
            return stringConstructor.Invoke(new[] { value.ToString() });
        }

        return Convert.ChangeType(value, targetType);
    }
}
