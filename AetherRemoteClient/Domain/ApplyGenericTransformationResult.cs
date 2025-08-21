using AetherRemoteClient.Domain.Enums;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Domain;

public class ApplyGenericTransformationResult(ApplyGenericTransformationErrorCode success, JObject? glamourerJObject)
{
    public readonly ApplyGenericTransformationErrorCode Success = success;
    public readonly JObject? GlamourerJObject = glamourerJObject;
}