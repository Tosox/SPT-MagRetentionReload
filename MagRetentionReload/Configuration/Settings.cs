using BepInEx.Configuration;

namespace Tosox.MagRetentionReload.Configuration
{
    internal static class Settings
    {
        private const string GeneralSection = "1. General";

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> RequireMaxMastery;

        internal static void Init(ConfigFile config)
        {
            Enabled = config.Bind(GeneralSection, "Enabled", true,
                new ConfigDescription("Untick to disable magazine retention",
                    null, Order(0)));

            RequireMaxMastery = config.Bind(GeneralSection, "Require Max Weapon Mastering", true,
                new ConfigDescription("Only retain magazines once the weapon's mastering is maxed out",
                    null, Order(1)));
        }

        private static ConfigurationManagerAttributes Order(int position)
        {
            return new ConfigurationManagerAttributes { Order = short.MaxValue - position };
        }
    }
}
