using EFT;
using EFT.InventoryLogic;
using Tosox.MagRetentionReload.Configuration;
using Tosox.MagRetentionReload.Sync;

namespace Tosox.MagRetentionReload.Helpers
{
    internal static class RetentionRules
    {
        private const int MaxMasteryLevel = 2;

        /// <summary>
        /// Only the machine the player belongs to decides, every other one is told the outcome
        /// </summary>
        internal static bool IsRetaining(Player owner, Weapon weapon, Magazine nextMagazine)
        {
            if (owner == null || weapon == null)
                return false;

            // Mirrored no matter what this client has configured, otherwise it would drop a magazine
            // that the player it belongs to kept
            if (!owner.IsYourPlayer)
                return RetentionSync.TakeAnnouncement(owner.ProfileId, nextMagazine?.Id);

            return IsAllowed(owner.Skills, weapon);
        }

        /// <summary>
        /// Variant for the inventory screen, which has a profile but not always a player, and which
        /// only ever runs for the local player anyway
        /// </summary>
        internal static bool IsAllowed(Profile profile, Weapon weapon)
        {
            if (profile == null || weapon == null)
                return false;

            return IsAllowed(profile.Skills, weapon);
        }

        private static bool IsAllowed(SkillManager skills, Weapon weapon)
        {
            if (!Settings.Enabled.Value)
                return false;

            // Nobody retains while the raid holds clients that would handle it differently
            if (RetentionSync.Incompatible)
                return false;

            if (!Settings.RequireMaxMastery.Value)
                return true;

            var templateId = (string)weapon.TemplateId;

            // Weapons that belong to no mastering group can never reach the required level, so gating
            // them would turn retention off for them for good
            if (MasteryGroups.Resolve(templateId) == null)
                return true;

            return (skills?.GetMastering(templateId)?.Level ?? 0) >= MaxMasteryLevel;
        }
    }
}
