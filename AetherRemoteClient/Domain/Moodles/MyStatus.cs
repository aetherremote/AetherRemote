using MoodlesStatusInfo = (
    System.Guid GUID,
    int IconID,
    string Title,
    string Description,
    Moodles.Data.MyStatus.StatusType Type,
    string Applier,
    bool Dispelable,
    int Stacks,
    bool Persistent,
    int Days,
    int Hours,
    int Minutes,
    int Seconds,
    bool NoExpire,
    bool AsPermanent,
    System.Guid StatusOnDispell,
    string CustomVFXPath,
    bool StackOnReapply,
    int StacksIncOnReapply);

using System;
using MemoryPack;

namespace Moodles.Data;

/// <summary>
///     This class is pulled directly from moodles, and is used to access the underlying moodles objects
///     https://github.com/kawaii/Moodles/blob/main/Moodles/Data/MyStatus.cs
/// </summary>

[Serializable]
[MemoryPackable]
public partial class MyStatus
{
    internal string ID => GUID.ToString();
    public Guid GUID = Guid.NewGuid();
    public int IconID;
    public string Title = "";
    public string Description = "";
    public long ExpiresAt;
    public StatusType Type;
    public string Applier = "";
    public bool Dispelable = false;
    public int Stacks = 1;
    public Guid StatusOnDispell = Guid.Empty;
    public string CustomFXPath = "";
    public bool StackOnReapply = false;
    public int StacksIncOnReapply = 1;


    [MemoryPackIgnore] public bool Persistent = false;

    [NonSerialized] internal int TooltipShown = -1;

    [MemoryPackIgnore] public int Days = 0;
    [MemoryPackIgnore] public int Hours = 0;
    [MemoryPackIgnore] public int Minutes = 0;
    [MemoryPackIgnore] public int Seconds = 0;
    [MemoryPackIgnore] public bool NoExpire = false;
    [MemoryPackIgnore] public bool AsPermanent = false;

    public bool ShouldSerializeGUID() => GUID != Guid.Empty;
    public bool ShouldSerializePersistent() => ShouldSerializeGUID();
    public bool ShouldSerializeExpiresAt() => ShouldSerializeGUID();

    internal uint AdjustedIconID => (uint)(IconID + Stacks - 1);
    internal long TotalDurationSeconds => Seconds * 1000 + Minutes * 1000 * 60 + Hours * 1000 * 60 * 60 + Days * 1000 * 60 * 60 * 24;

    public bool IsValid(out string error)
    {
        if(IconID == 0)
        {
            error = ("Icon is not set");
            return false;
        }
        if (IconID < 200000)
        {
            error = ("Icon is a Pre 7.1 Moodle!");
            return false;
        }
        if (Title.Length == 0)
        {
            error = ("Title is not set");
            return false;
        }
        if(TotalDurationSeconds < 1 && !NoExpire)
        {
            error = ("Duration is not set");
            return false;
        }
        error = null;
        return true;
    }

    public MoodlesStatusInfo ToStatusInfoTuple()
        => (GUID, IconID, Title, Description, Type, Applier, Dispelable, Stacks, Persistent, Days, Hours, 
        Minutes, Seconds, NoExpire, AsPermanent, StatusOnDispell, CustomFXPath, StackOnReapply, StacksIncOnReapply);

    public static MyStatus FromStatusInfoTuple(MoodlesStatusInfo statusInfo)
    {
        return new MyStatus
        {
            GUID = statusInfo.GUID,
            IconID = statusInfo.IconID,
            Title = statusInfo.Title,
            Description = statusInfo.Description,
            Type = statusInfo.Type,
            Applier = statusInfo.Applier,
            Dispelable = statusInfo.Dispelable,
            Stacks = statusInfo.Stacks,
            Persistent = statusInfo.Persistent,
            Days = statusInfo.Days,
            Hours = statusInfo.Hours,
            Minutes = statusInfo.Minutes,
            Seconds = statusInfo.Seconds,
            NoExpire = statusInfo.NoExpire,
            AsPermanent = statusInfo.AsPermanent,
            StatusOnDispell = statusInfo.StatusOnDispell,
            CustomFXPath = statusInfo.CustomVFXPath,
            StackOnReapply = statusInfo.StackOnReapply,
            StacksIncOnReapply = statusInfo.StacksIncOnReapply
        };
    }
    
    public enum StatusType
    {
        Positive,
        Negative,
        Special
    }
}

