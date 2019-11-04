using UnityEngine;

public class GNShield : PartModule
{
    [KSPField(isPersistant = true)]
    public bool shieldActivated = false;

    [KSPField(guiName = "Shield Status", guiActive = true)]
    private string ES = "Deactivated";

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Shield Radius", isPersistant = true), UI_FloatRange(minValue = 5f, maxValue = 50f, stepIncrement = 5f)]
    public float ShieldRadius = 10f;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Shield layer", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 31f, stepIncrement = 1f)]
    public float Shieldlayer = 10f;

    [KSPAction("Toggle", KSPActionGroup.None, guiName = "Toggle Shield")]
    private void ActionActivate(KSPActionParam param)
    {
        if (shieldActivated == true)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    [KSPEvent(name = "Activate", guiName = "Activate Shield", active = true, guiActive = true)]
    public void Activate()
    {
            shieldActivated = true;
            Events["Deactivate"].guiActive = true;
            Events["Activate"].guiActive = false;
    }

    [KSPEvent(name = "Deactivate", guiName = "Deactivate Shield", active = true, guiActive = false)]
    public void Deactivate()
    {
        shieldActivated = false;
        Events["Deactivate"].guiActive = false;
        Events["Activate"].guiActive = true;
    }

    public override void OnUpdate()
    {
        Transform ShieldTransform = base.part.FindModelTransform("Shield");
        KSPParticleEmitter ShieldEmitter = ShieldTransform.gameObject.GetComponent<KSPParticleEmitter>();
        SphereCollider ShieldCollider = ShieldTransform.gameObject.GetComponent<SphereCollider>();
        if (shieldActivated == true)
        {
            ES = "Shield Activated";

            ShieldTransform.gameObject.SetActive(true);
            ShieldEmitter.emit = true;
        }
        else
        {
            ES = "Shield Deactivated";
            ShieldEmitter.emit = false;
            ShieldTransform.gameObject.SetActive(false);
        }
    }
}