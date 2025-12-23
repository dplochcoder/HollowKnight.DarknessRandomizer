using PurenailCore.ModUtil;
using UnityEngine;

namespace DarknessRandomizer;

public class Preloader : PurenailCore.ModUtil.Preloader
{
    public static readonly Preloader Instance = new();

    [Preload("Cliffs_01", "Darkness Region (3)")]
    public GameObject DarknessRegion { get; private set; }

    [Preload("White_Palace_03_hub", "doorWarp")]
    public GameObject DreamWarp { get; private set; }

    [Preload("White_Palace_03_hub", "dream_beam_animation")]
    public GameObject DreamBeamAnim { get; private set; }

    [Preload("Abyss_05", "door_dreamReturn_reality")]
    public GameObject DreamReturn { get; private set; }
}
