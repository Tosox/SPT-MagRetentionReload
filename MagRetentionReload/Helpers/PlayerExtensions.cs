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

        /// <summary>
        /// The player an item controller belongs to, or null when it belongs to nobody
        /// </summary>
        internal static Player GetPlayer(this ItemController itemController)
        {
            return (itemController as Player.PlayerInventoryController)?.Player;
        }

        /// <summary>
        /// Whether the player is a bot. Bots mirrored from another client only say so through a field
        /// that Fika adds, which is looked up by name so that this works without Fika as well.
        /// </summary>
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
