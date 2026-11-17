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
    internal static class FikaSync
    {
        private static bool warnedVersionMismatch;

        internal static void Init()
        {
            // Load the packet assembly now, so a broken install is reported here and not on the first send
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

            // Explicit delegates, since cached lambdas land in a class that loads Fika.Core too early
            FikaEventDispatcher.SubscribeEvent(new Action<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated));
            FikaEventDispatcher.SubscribeEvent(new Action<FikaGameEndedEvent>(OnGameEnded));

            RetentionSync.RetentionAnnouncer = new Action<string, string>(SendRetention);

            Plugin.Log.LogInfo("Fika detected, retention sync enabled");
        }

        private static void OnNetworkManagerCreated(FikaNetworkManagerCreatedEvent args)
        {
            // Both roles, the host receives the clients' packets too
            args.Manager.RegisterPacket(new Action<MagRetentionPacket>(OnPacketReceived));
        }

        private static void OnGameEnded(FikaGameEndedEvent args)
        {
            // A transit continues the same raid
            if (args.ExitStatus == ExitStatus.Transit)
                return;

            RetentionSync.Clear();
            warnedVersionMismatch = false;
        }

        // Sent before Fika's reload over the same ordered channel, so it always arrives first
        private static void SendRetention(string profileId, string magazineId)
        {
            var manager = Singleton<IFikaNetworkManager>.Instance;
            if (manager == null)
                return;

            var packet = MagRetentionPacket.Create(ModInfo.Version, profileId, magazineId);

            // Broadcast, so the host also relays a client's packet to everyone
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

        // Off entirely, refusing only their packets would still retain what they drop
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

            // Reply once so the other side learns our version. Each side warns only once, so no loop
            SendRetention(null, null);
        }

        // Separate method so a missing assembly throws when called, not when Init is compiled
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ProbePacketAssembly()
        {
            _ = typeof(MagRetentionPacket).TypeHandle;
        }
    }
}
