using System;
using System.Collections.Generic;

namespace Tosox.MagRetentionReload.Sync
{
    internal static class RetentionSync
    {
        // The magazine each player announced a retention for, keyed by profile id. A player only ever
        // has one reload in flight, so a newer announcement simply replaces the previous one.
        private static readonly Dictionary<string, string> Announcements = new Dictionary<string, string>();

        /// <summary>
        /// Sends a retention out to the other clients. Assigned by the Fika sync, and left null without
        /// Fika, which keeps the reload patches clear of anything that needs Fika to be installed.
        /// </summary>
        internal static Action<string, string> RetentionAnnouncer { get; set; }

        /// <summary>
        /// Set once a client on a different version of the mod turns up. Retention stays off until the
        /// raid ends, because a client that handles reloads differently cannot be mirrored safely.
        /// </summary>
        internal static bool Incompatible { get; set; }

        internal static void SetAnnouncement(string profileId, string magazineId)
        {
            if (!string.IsNullOrEmpty(profileId))
                Announcements[profileId] = magazineId;
        }

        /// <summary>
        /// Whether the player announced a retention for exactly this magazine. The announcement is
        /// forgotten either way, so one that never got used cannot be applied to a later reload.
        /// </summary>
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
