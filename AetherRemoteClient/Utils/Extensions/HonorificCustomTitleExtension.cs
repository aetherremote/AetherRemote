using AetherRemoteClient.Dependencies.Honorific.Domain;
using AetherRemoteCommon.Dependencies.Honorific.Domain;

namespace AetherRemoteClient.Utils.Extensions;

public static class HonorificCustomTitleExtension
{
    public static HonorificDto ToHonorificDto(this HonorificCustomTitle honorific)
    {
        return new HonorificDto(honorific.Title, honorific.IsPrefix, honorific.Color, honorific.Glow);
    }
    
    public static HonorificCustomTitle ToHonorificDto(this HonorificDto honorific)
    {
        return new HonorificCustomTitle(honorific.Title, honorific.IsPrefix, honorific.Color, honorific.Glow);
    }
}