using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using UnityEngine;

namespace DarknessRandomizer.IC;

public record DreamnailWarpGlow : Deployer
{
    public DreamnailWarpGlow()
    {
        SceneName = Data.SceneName.DreamNail.ToString();
        X = 13;
        Y = 13.2f;
    }

    private const float SCALE = 0.85f;

    public override GameObject Instantiate()
    {
        var obj = Object.Instantiate(Preloader.Instance.DreamBeamAnim);
        obj.transform.localScale = new(SCALE, SCALE, SCALE);
        return obj;
    }
}

public record DreamnailWarp : Deployer
{
    public DreamnailWarp()
    {
        SceneName = Data.SceneName.DreamNail.ToString();
        X = 13;
        Y = 13.8f;
    }

    public override GameObject Instantiate()
    {
        var obj = Object.Instantiate(Preloader.Instance.DreamWarp);
        var sceneName = Data.SceneName.GroundsDreamNailEntrance.ToString();

        var state = obj.LocateMyFSM("Door Control").GetState("Change Scene");
        var cmp = state.GetFirstActionOfType<CallMethodProper>();
        cmp.parameters[0].SetValue(sceneName);
        cmp.parameters[1].SetValue(DreamnailWarpTarget.GATE_NAME);
        var bst = state.GetFirstActionOfType<BeginSceneTransition>();
        bst.sceneName = sceneName;
        bst.entryGateName = DreamnailWarpTarget.GATE_NAME;

        return obj;
    }
}

public record DreamnailWarpTarget : Deployer
{
    public const string GATE_NAME = "door_darknessRandoDreamnailExit";

    public DreamnailWarpTarget()
    {
        SceneName = Data.SceneName.GroundsDreamNailEntrance.ToString();
        X = 52;
        Y = 7.8f;
    }

    public override GameObject Instantiate() => Object.Instantiate(Preloader.Instance.DreamReturn);

    public override GameObject Deploy()
    {
        var obj = base.Deploy();
        obj.name = GATE_NAME;
        return obj;
    }
}
