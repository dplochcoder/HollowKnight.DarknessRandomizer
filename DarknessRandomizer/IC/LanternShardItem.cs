using ItemChanger;
using ItemChanger.Components;
using ItemChanger.Tags;
using System;
using UnityEngine;

namespace DarknessRandomizer.IC;

public static class LanternShards
{
    public const string PDName = "numLanternShards";

    public const int TotalNumShards = 4;

    public static int GetPDShardCount() => PlayerData.instance.GetInt(PDName);
}

internal class LanternShardUIDef(bool isFinal) : UIDef
{
    public readonly bool IsFinal = isFinal;
    private readonly ISprite sprite = new EmbeddedSprite("LanternShard");
    private readonly ISprite bigSprite = new EmbeddedSprite("LumaflyLantern");

    public override string GetPreviewName() => "Lantern Shard";

    public override string GetPostviewName()
    {
        if (IsFinal) return $"Lantern Shard (#{LanternShards.TotalNumShards})";

        int count = LanternShards.GetPDShardCount();
        return count >= LanternShards.TotalNumShards ? "Lantern Shard" : $"Lantern Shard (#{count})";
    }

    public override string GetShopDesc() => LanternShards.GetPDShardCount() switch
    {
        0 => "I suppose this piece of trash is worth something?",
        1 => "What are you going to do with two pieces of trash?",
        2 => "Are you going to weld these together or something? How?!",
        3 => "Wow, you actually found the whole thing. I'm impressed.",
        _ => "I hear that with two lanterns you can explore advanced darkness.",
    };

    public override Sprite GetSprite() => sprite.Value;

    public override void SendMessage(MessageType type, Action? callback = null)
    {
        if (IsFinal && (type & MessageType.Big) == MessageType.Big)
        {
            BigItemPopup.Show(
                bigSprite.Value,
                "Assembled the",
                new LanguageString("UI", "INV_NAME_LANTERN").Value,
                null,
                null,
                "The last shard is collected, the whole assembled.",
                "Fear the darkness no longer.",
                callback);
            return;
        }

        if ((type & MessageType.Corner) == MessageType.Corner)
        {
            ItemChanger.Internal.MessageController.Enqueue(GetSprite(), GetPostviewName());
        }

        callback?.Invoke();
    }

    public override UIDef Clone() => new LanternShardUIDef(IsFinal);
}

public class AbstractLanternShardItem : AbstractItem
{
    private const string InteropMessage = ConnectionMetadataInjector.SupplementalMetadata.InteropTagMessage;
    private const string InteropItemPoolGroup = nameof(ConnectionMetadataInjector.Util.PoolGroup.Keys);

    protected AbstractLanternShardItem(string name)
    {
        this.name = name;

        var interop = AddTag<InteropTag>();
        interop.Message = InteropMessage;
        interop.Properties["PoolGroup"] = InteropItemPoolGroup;
        interop.Properties["ModSource"] = DarknessRandomizer.Instance.GetName();
    }

    public override void GiveImmediate(GiveInfo info) => PlayerData.instance.SetInt(LanternShards.PDName, LanternShards.GetPDShardCount() + 1);

    public override bool Redundant() => PlayerData.instance.GetBool(nameof(PlayerData.instance.hasLantern));
}

public class AbstractBaseLanternShardItem : AbstractLanternShardItem
{
    public string FinalShardItemName;

    protected AbstractBaseLanternShardItem(string name, string finalName) : base(name)
    {
        FinalShardItemName = finalName;
        UIDef = new LanternShardUIDef(false);
        ModifyItem += MaybeCompleteLantern;
    }

    private void MaybeCompleteLantern(GiveEventArgs args)
    {
        if (LanternShards.GetPDShardCount() == LanternShards.TotalNumShards - 1)
        {
            args.Item = Finder.GetItem(FinalShardItemName);
        }
    }
}

public class AbstractFinalLanternShardItem : AbstractLanternShardItem
{
    public string LanternItemName;

    protected AbstractFinalLanternShardItem(string name, string lanternName) : base(name)
    {
        LanternItemName = lanternName;
        UIDef = new LanternShardUIDef(true);
    }

    public override void GiveImmediate(GiveInfo info)
    {
        base.GiveImmediate(info);
        Finder.GetItem(LanternItemName).GiveImmediate(info);
    }
}

public class FinalLanternShardItem : AbstractFinalLanternShardItem
{
    public const string ItemName = "Final_Lantern_Shard";

    public FinalLanternShardItem() : base(ItemName, ItemNames.Lumafly_Lantern) { }

    public override AbstractItem Clone() => new FinalLanternShardItem();
}

public class LanternShardItem : AbstractBaseLanternShardItem
{
    public const string ItemName = "Lantern_Shard";

    public LanternShardItem() : base(ItemName, FinalLanternShardItem.ItemName) { }

    public override AbstractItem Clone() => new LanternShardItem();

    public static void DefineICRefs()
    {
        Finder.DefineCustomItem(new FinalLanternShardItem());
        Finder.DefineCustomItem(new LanternShardItem());
    }
}
