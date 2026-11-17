using EFT;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tosox.MagRetentionReload.Helpers
{
    internal static class PlayerExtensions
    {
        private static readonly Dictionary<Type, FieldInfo> ObservedAiFields = new Dictionary<Type, FieldInfo>();

        internal static Player GetPlayer(this ItemController itemController)
        {
            return (itemController as Player.PlayerInventoryController)?.Player;
        }

        // Observed bots only say so through a field Fika adds, looked up by name to work without Fika
        internal static bool IsAi(this Player player)
        {
            if (player == null)
                return false;

            if (player.IsAI)
                return true;

            var type = player.GetType();
            if (!ObservedAiFields.TryGetValue(type, out var field))
            {
                field = type.GetField("IsObservedAI", BindingFlags.Public | BindingFlags.Instance);
                ObservedAiFields[type] = field;
            }

            return field != null && (bool)field.GetValue(player);
        }
    }
}
