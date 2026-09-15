using BepInEx.Configuration;

namespace Tosox.MagRetentionReload.Configuration
{
    internal static class Settings
    {
        private const string GeneralSection = "1. General";

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> RequireMaxMastery;
        internal static ConfigEntry<bool> NotifyOnDrop;

        internal static void Init(ConfigFile config)
        {
            Enabled = config.Bind(GeneralSection, "Enabled", true,
                new ConfigDescription("Untick to stop retaining your magazines, handing the feature back to UI Fixes if installed",
                    null, Order(0)));

            RequireMaxMastery = config.Bind(GeneralSection, "Require Max Weapon Mastering", false,
                new ConfigDescription("Only retain magazines once the weapon's mastering is maxed out",
                    null, Order(1)));

            NotifyOnDrop = config.Bind(GeneralSection, "Notify On Dropped Magazine", true,
                new ConfigDescription("Display a notification when a magazine is dropped during a reload",
                    null, Order(2)));
        }

        private static ConfigurationManagerAttributes Order(int position)
        {
            return new ConfigurationManagerAttributes { Order = short.MaxValue - position };
        }
    }
}
