using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.InventoryLogic;
using EFT.UI;
using System.Linq;

namespace Tosox.MagRetentionReload.Commands
{
    public class DebugCommands
    {
        // Mastering runs from 0 to 2, where 2 is the maxed out level
        private const int MaxMasteryLevel = 2;

        [ConsoleCommand("masterweapon", "", null, "Sets the mastering of the weapon in hands", new string[] {})]
        public static void MasterWeapon(
            [ConsoleArgument("2", "0..2 - mastering level")] int level)
        {
            if (level < 0 || level > MaxMasteryLevel)
            {
                ConsoleScreen.LogError($"Mastering level has to be between 0 and {MaxMasteryLevel}");
                return;
            }

            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            if (player == null || !(player.HandsController?.Item is Weapon weapon))
            {
                ConsoleScreen.LogError("No weapon in hands");
                return;
            }

            var templateId = (string)weapon.TemplateId;
            var mastering = player.Skills?.GetMastering(templateId);

            if (mastering != null)
            {
                mastering.SetCurrent(ProgressFor(level, mastering.Lvl1, mastering.Lvl2), true);
            }
            else
            {
                // Never fired, so the skill has to be created before its level can be set
                var group = Singleton<GlobalConfiguration>.Instance?.Mastering
                    ?.FirstOrDefault(g => g.Templates != null && g.Templates.Contains(templateId));

                if (group == null)
                {
                    ConsoleScreen.LogError($"{weapon.LocalizedName()} belongs to no mastering group");
                    return;
                }

                player.Skills.ChangeMasteringLevel(templateId, ProgressFor(level, group.Level2, group.Level3), true);
            }

            ConsoleScreen.Log($"Mastering of {weapon.LocalizedName()} set to level {level}");
        }

        /// <summary>
        /// How much progress a level needs, the same way the game derives the level back from it. The
        /// group thresholds are named after the level they unlock, so Level2 is what the first one costs.
        /// </summary>
        private static float ProgressFor(int level, int firstThreshold, int secondThreshold)
        {
            if (level <= 0)
                return 0f;

            return level == 1 ? firstThreshold : firstThreshold + secondThreshold;
        }
    }
}
