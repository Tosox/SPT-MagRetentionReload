using Comfort.Common;
using EFT;
using EFT.Communications;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using System;
using System.Runtime.CompilerServices;
using Tosox.MagRetentionReload.Net;
using Tosox.MagRetentionReload.Sync;

namespace Tosox.MagRetentionReload.Fika
{
    /// <summary>
    /// Mirrors retention across a Fika raid. Every client replays the same reload, but only the player
    /// it belongs to decides whether the magazine is retained, so that decision is sent out and the
    /// others just follow it.
    /// </summary>
    internal static class FikaSync
    {
        private static bool warnedVersionMismatch;

        internal static void Init()
        {
            // The packet lives in its own assembly, so a half copied install only shows up once the
            // first packet is built. Load it here instead, where it can still be reported properly
            try
            {
                ProbePacketAssembly();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Could not load Tosox.MagRetentionReload.Net.dll, retention will not "
                    + "be synced and magazines will end up in different places for every player. "
                    + $"Reinstall the mod with all of its files. ({ex.Message})");
                return;
            }

            // The explicit delegates are required. Without them the compiler caches them in a generated
            // class that the game walks while loading types, which force loads Fika.Core
            FikaEventDispatcher.SubscribeEvent(new Action<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated));
            FikaEventDispatcher.SubscribeEvent(new Action<FikaGameEndedEvent>(OnGameEnded));

            RetentionSync.RetentionAnnouncer = new Action<string, string>(SendRetention);

            Plugin.Log.LogInfo("Fika detected, retention sync enabled");
        }

        private static void OnNetworkManagerCreated(FikaNetworkManagerCreatedEvent args)
        {
            // Covers both roles, since the host has to receive the clients' packets as well
            args.Manager.RegisterPacket(new Action<MagRetentionPacket>(OnPacketReceived));
        }

        private static void OnGameEnded(FikaGameEndedEvent args)
        {
            // A transit keeps the same raid going, so anything still in flight stays valid
            if (args.ExitStatus == ExitStatus.Transit)
                return;

            RetentionSync.Clear();
            warnedVersionMismatch = false;
        }

        /// <summary>
        /// Runs while the reload is still being set up, which is before Fika sends the reload itself.
        /// Both travel reliably and ordered over the same connection, so this always arrives first.
        /// </summary>
        private static void SendRetention(string profileId, string magazineId)
        {
            var manager = Singleton<IFikaNetworkManager>.Instance;
            if (manager == null)
                return;

            var packet = MagRetentionPacket.Create(ModInfo.Version, profileId, magazineId);

            // Broadcasting reaches every client from the host and gets relayed by the host from a client
            manager.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
        }

        private static void OnPacketReceived(MagRetentionPacket packet)
        {
            if (packet.Version != ModInfo.Version)
            {
                OnVersionMismatch(packet.Version);
                return;
            }

            switch (packet.Kind)
            {
                case MagRetentionPacket.KindRetention:
                    RetentionSync.SetAnnouncement(packet.ProfileId, packet.MagazineId);
                    break;

                default:
                    Plugin.Log.LogWarning($"Received unknown packet kind {packet.Kind}");
                    break;
            }
        }

        /// <summary>
        /// Turns retention off altogether rather than only refusing what the other client sent.
        /// Refusing it alone would still leave this client retaining magazines that the other drops.
        /// </summary>
        private static void OnVersionMismatch(string otherVersion)
        {
            RetentionSync.Incompatible = true;

            if (warnedVersionMismatch)
                return;

            warnedVersionMismatch = true;

            Plugin.Log.LogError($"{ModInfo.Name} version mismatch, you have {ModInfo.Version} and another client has {otherVersion}");
            NotificationManager.DisplayWarningNotification(
                $"{ModInfo.Name} version mismatch! You: {ModInfo.Version}, other client: {otherVersion}. "
                + "Retention is off for this raid because different versions may handle reloads differently.",
                ENotificationDurationType.Long);

            // The other client cannot see our version until we send it something, so answer once. Both
            // sides go quiet after their own warning, which keeps this from bouncing back and forth.
            SendRetention(null, null);
        }

        // Kept out of Init so the failed type load surfaces as an exception there rather than
        // while Init itself is being compiled
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ProbePacketAssembly()
        {
            _ = typeof(MagRetentionPacket).TypeHandle;
        }
    }
}
