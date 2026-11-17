using EFT;
using EFT.InventoryLogic;
using Tosox.MagRetentionReload.Configuration;
using Tosox.MagRetentionReload.Sync;

namespace Tosox.MagRetentionReload.Helpers
{
    internal static class RetentionRules
    {
        private const int MaxMasteryLevel = 2;

        internal static bool IsRetaining(Player owner, Weapon weapon, Magazine nextMagazine)
        {
            if (owner == null || weapon == null)
                return false;

            // Mirror the owner regardless of local settings, or this client drops a magazine they kept
            if (!owner.IsYourPlayer)
                return RetentionSync.TakeAnnouncement(owner.ProfileId, nextMagazine?.Id);

            return IsAllowed(owner.Skills, weapon);
        }

        // For the inventory screen, which may have no player
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

            // Off while a client on another version is in the raid
            if (RetentionSync.Incompatible)
                return false;

            if (!Settings.RequireMaxMastery.Value)
                return true;

            var templateId = (string)weapon.TemplateId;

            // Without a mastering group it can never be maxed, so don't gate it
            if (MasteryGroups.Resolve(templateId) == null)
                return true;

            return (skills?.GetMastering(templateId)?.Level ?? 0) >= MaxMasteryLevel;
        }
    }
}
