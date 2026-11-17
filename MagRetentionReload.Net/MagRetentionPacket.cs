using Fika.Core.Networking.LiteNetLib.Utils;

namespace Tosox.MagRetentionReload.Net
{
    // Own assembly, since implementing a Fika interface makes the type unloadable without Fika
    public struct MagRetentionPacket : INetSerializable
    {
        public const byte KindRetention = 0;

        public byte Kind;
        public string Version;
        public string ProfileId;
        public string MagazineId;

        public static MagRetentionPacket Create(string version, string profileId, string magazineId)
        {
            return new MagRetentionPacket
            {
                Kind = KindRetention,
                Version = version,
                ProfileId = profileId,
                MagazineId = magazineId
            };
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Kind);
            writer.Put(Version ?? string.Empty);
            writer.Put(ProfileId ?? string.Empty);
            writer.Put(MagazineId ?? string.Empty);
        }

        public void Deserialize(NetDataReader reader)
        {
            Kind = reader.GetByte();
            Version = reader.GetString();
            ProfileId = reader.GetString();
            MagazineId = reader.GetString();
        }
    }
}
