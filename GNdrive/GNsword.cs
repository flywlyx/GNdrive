using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GNsword : PartModule
{
    [KSPField]
    public float fuelconsumption = 1F;

    [KSPField]
    public float Maxbladelength = 12F;
    [KSPField]
    public float BladeHeat = 4000F;

    private Transform BladeProjector = null;
    private Transform swordEMI = null;
    private KSPParticleEmitter emt = null;
    private bool BladeActivated = false;

    [KSPAction("Toggle", KSPActionGroup.None, guiName = "Toggle Field")]
    private void ActionActivate(KSPActionParam param)
    {
        if (BladeActivated == true)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    [KSPEvent(name = "Activate", guiName = "Activate Blade", active = true, guiActive = true)]
    public void Activate()
    {
            BladeActivated = true;
            Events["Deactivate"].guiActive = true;
            Events["Activate"].guiActive = false;
            this.enabled = true;
            this.part.force_activate();
    }
    [KSPEvent(name = "Activate", guiName = "Deactivate Blade", active = true, guiActive = false)]
    public void Deactivate()
    {
        BladeActivated = false;
        Events["Deactivate"].guiActive = false;
        Events["Activate"].guiActive = true;
    }

    public override void OnStart(PartModule.StartState state)
    {
        BladeProjector = base.part.FindModelTransform("BladeProjector");
        swordEMI = base.part.FindModelTransform("swordEMI");
        emt = swordEMI.gameObject.GetComponent("KSPParticleEmitter") as KSPParticleEmitter;
        if (state != StartState.Editor && state != StartState.None)
        {
            this.enabled = true;
            this.part.force_activate();
        }
        if (HighLogic.LoadedSceneIsEditor)
        {
            emt.emit = false;
        }
    }

    public override void OnFixedUpdate()
    {
        if (BladeActivated == true)
        {
            float Truebladelength = Maxbladelength;
            if (Physics.Raycast(BladeProjector.position, BladeProjector.TransformDirection(Vector3.up), out RaycastHit rayHit, Maxbladelength))
            {
                Part part = null;
                try
                {
                    part = rayHit.collider.GetComponentInParent<Part>();
                    //Debug.Log(part.partName);
                }
                catch (NullReferenceException) { }
                if (part && part.vessel != this.vessel)
                {
                    part.temperature += BladeHeat;
                    //Debug.Log(part.temperature);
                    if (part.physicalSignificance == Part.PhysicalSignificance.NONE)
                    {
                        part.explode();
                    }
                }
                //Debug.Log(rayHit.distance);
                Truebladelength = rayHit.distance;
            }
            if (Truebladelength>Maxbladelength)
            {
                Truebladelength = Maxbladelength;
            }
            emt.emit = true;
            emt.shape1D = Truebladelength;
            swordEMI.localPosition = Vector3.up * Truebladelength / 2f;
        }
        else
        {
            emt.emit = false;
            Deactivate();
        }

    }

}