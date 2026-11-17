using System;
using System.Collections.Generic;

namespace Tosox.MagRetentionReload.Sync
{
    internal static class RetentionSync
    {
        // One reload in flight per player, so a newer announcement replaces the older one
        private static readonly Dictionary<string, string> Announcements = new Dictionary<string, string>();

        // Null without Fika, which keeps the reload patches free of Fika types
        internal static Action<string, string> RetentionAnnouncer { get; set; }

        internal static bool Incompatible { get; set; }

        internal static void SetAnnouncement(string profileId, string magazineId)
        {
            if (!string.IsNullOrEmpty(profileId))
                Announcements[profileId] = magazineId;
        }

        // Forgotten either way, so an unused one can't apply to a later reload
        internal static bool TakeAnnouncement(string profileId, string magazineId)
        {
            if (string.IsNullOrEmpty(profileId) || !Announcements.TryGetValue(profileId, out var announced))
                return false;

            Announcements.Remove(profileId);
            return !string.IsNullOrEmpty(magazineId) && announced == magazineId;
        }

        internal static void Clear()
        {
            Announcements.Clear();
            Incompatible = false;
        }
    }
}
