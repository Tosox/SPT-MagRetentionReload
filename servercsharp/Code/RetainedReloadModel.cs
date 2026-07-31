using SPTarkov.Server.Core.Models.Eft.Common.Request;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tosox.MagRetentionReload.Server;

public record RetainedReloadModel : BaseInteractionRequestData
{
    [JsonPropertyName("CurrentMagazine")]
    public string? CurrentMagazine { get; set; }

    [JsonPropertyName("ReplacementMagazine")]
    public string? ReplacementMagazine { get; set; }

    [JsonPropertyName("WeaponSlot")]
    public RetainedReloadAddressModel? WeaponSlot { get; set; }

    [JsonPropertyName("RetainedSlot")]
    public RetainedReloadAddressModel? RetainedSlot { get; set; }

    [JsonPropertyName("RetainedItems")]
    public List<RetainedReloadItemSnapshotModel>? RetainedItems { get; set; }
}

public record RetainedReloadAddressModel
{
    [JsonPropertyName("parentItem")]
    public string? ParentItem { get; set; }

    [JsonPropertyName("container")]
    public string? Container { get; set; }

    [JsonPropertyName("location")]
    public JsonElement? Location { get; set; }
}

public record RetainedReloadItemSnapshotModel
{
    [JsonPropertyName("ItemId")]
    public string? ItemId { get; set; }

    [JsonPropertyName("TemplateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("StackCount")]
    public int StackCount { get; set; }

    [JsonPropertyName("SpawnedInSession")]
    public bool SpawnedInSession { get; set; }

    [JsonPropertyName("Address")]
    public RetainedReloadAddressModel? Address { get; set; }
}
