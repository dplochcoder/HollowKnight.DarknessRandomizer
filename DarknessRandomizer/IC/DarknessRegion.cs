using DarknessRandomizer.Data;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using UnityEngine;

namespace DarknessRandomizer.IC;

// Marks a darkness region that was placed by DarknessRandomizer, and should not be altered.
public class CustomDarknessRegion : MonoBehaviour { }

public record DarknessRegion
{
    public Darkness Darkness;
    public float X;
    public float Y;
    public float Width;
    public float Height;

    public void Deploy()
    {
        var obj = Object.Instantiate(Preloader.Instance.DarknessRegion);
        obj.AddComponent<CustomDarknessRegion>();
        obj.name = $"CustomDarknessRegion-{X}-{Y}";
        obj.transform.position = new(X, Y, 0);
        obj.transform.localScale = new(1, 1, 1);
        obj.GetComponent<BoxCollider2D>().size = new(Width, Height);

        var fsm = obj.LocateMyFSM("Darkness Region");
        fsm.FsmVariables.FindFsmInt("Darkness").Value = (int)Darkness;
        fsm.GetState("Enter").AddLastAction(new Lambda(() =>
        {
            if (!PlayerData.instance.GetBool(nameof(PlayerData.instance.hasLantern)))
            {
                GameObject.Find("/Knight/Vignette/Darkness Plates")?.SetActive(true);
            }
        }));
        fsm.GetState("Exit").AddLastAction(new Lambda(() => GameObject.Find("/Knight/Vignette/Darkness Plates")?.SetActive(false)));

        obj.SetActive(true);
    }
}
