using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace DarknessRandomizer.Rando;

public class SettingsProxy : RandoSettingsProxy<RandomizationSettings, string>
{
    public override string ModKey => nameof(DarknessRandomizer);

    public override VersioningPolicy<string> VersioningPolicy => new StrictModVersioningPolicy(DarknessRandomizer.Instance);

    public override bool TryProvideSettings(out RandomizationSettings? settings)
    {
        settings = DarknessRandomizer.GS.RandomizationSettings;
        return settings.IsEnabled;
    }

    public override void ReceiveSettings(RandomizationSettings? settings) => ConnectionMenu.Instance.ApplySettings(settings ?? new());
}
