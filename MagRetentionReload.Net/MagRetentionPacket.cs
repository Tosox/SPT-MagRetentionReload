using Fika.Core.Networking.LiteNetLib.Utils;

namespace Tosox.MagRetentionReload.Net
{
    /// <summary>
    /// Tells the other clients that this player is retaining the magazine of the reload that follows.
    /// <para/>
    /// Implementing a Fika interface makes this type unloadable without Fika, which is why it lives in
    /// its own assembly. Nothing loads it until the sync actually sends something, while the plugin
    /// assembly is loaded on every start and has to stay walkable for mods that enumerate types.
    /// </summary>
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
