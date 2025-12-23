using ItemChanger.Internal;

namespace DarknessRandomizer.IC;

public class EmbeddedSprite : ItemChanger.EmbeddedSprite
{
    private static readonly SpriteManager manager = new(typeof(EmbeddedSprite).Assembly, "DarknessRandomizer.Resources.Sprites.");

    public EmbeddedSprite(string key) => this.key = key;

    public override SpriteManager SpriteManager => manager;
}
