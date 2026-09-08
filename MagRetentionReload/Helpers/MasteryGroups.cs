using Comfort.Common;
using EFT;
using System.Collections.Generic;

namespace Tosox.MagRetentionReload.Helpers
{
    internal static class MasteryGroups
    {
        private static Dictionary<string, string> groupsByTemplate;

        /// <summary>
        /// Resolves a weapon template to its mastering group
        /// </summary>
        internal static string Resolve(string templateId)
        {
            if (string.IsNullOrEmpty(templateId))
                return null;

            var groups = groupsByTemplate ?? Build();
            if (groups == null)
                return null;

            return groups.TryGetValue(templateId, out var groupId) ? groupId : null;
        }

        private static Dictionary<string, string> Build()
        {
            // Missing until the configuration arrives, so keep retrying instead of caching an empty map
            var masteringGroups = Singleton<GlobalConfiguration>.Instantiated
                ? Singleton<GlobalConfiguration>.Instance?.Mastering
                : null;

            if (masteringGroups == null)
                return null;

            var groups = new Dictionary<string, string>();
            foreach (var group in masteringGroups)
            {
                if (group?.Templates == null || string.IsNullOrEmpty(group.Id))
                    continue;

                foreach (var templateId in group.Templates)
                {
                    if (!string.IsNullOrEmpty(templateId))
                        groups[templateId] = group.Id;
                }
            }

            groupsByTemplate = groups;
            return groups;
        }
    }
}
