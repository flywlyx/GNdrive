using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class Taudrive : PartModule
{
    [KSPField]
    public float fuelefficiency = 0.001F;
    [KSPField]
    public float particlegrate = 1F;
    [KSPField]
    public float maxenginecount = 2F;

    public Vector4 color = Vector4.zero;


    [KSPField(isPersistant = true)]
    public bool engineIgnited = false;
    public bool flameOut = false;
    public bool agActivated = false;
    public bool taactivated = false;
    public bool depleted = false;
    private bool modified = false;
    public float overloadtemp = 0;
    


    //    public bool staged = false;
    private ParticleEmitter emt;
    public float particleSize = 0.001f;

    private float rotation = 0F;

    private GameObject rotor;
    private GameObject stator;

    [KSPField(guiName = "Engine Status", guiActive = true)]
    private string ES = "Deactivated";

    [KSPField(guiName = "Mass", guiActive = true)]
    private string mass = "N/a";

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max Overload", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 5f, stepIncrement = 0.1f)]
    public float Overload = 1f;

    [KSPAction("Toggle", KSPActionGroup.None, guiName = "Toggle Engine")]
    private void ActionActivate(KSPActionParam param)
    {
        if (engineIgnited == true)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    [KSPEvent(name = "Activate", guiName = "Activate Engine", active = true, guiActive = true)]
    public void Activate()
    {
        this.part.force_activate();
        if (depleted == false)
        {
            engineIgnited = true;
            Events["Deactivate"].guiActive = true;
            Events["Activate"].guiActive = false;
            modified = true;
        }

    }

    [KSPEvent(name = "Deactivate", guiName = "Deactivate Engine", active = true, guiActive = false)]
    public void Deactivate()
    {
        engineIgnited = false;
        Events["Deactivate"].guiActive = false;
        Events["Activate"].guiActive = true;
        modified = true;
    }

    [KSPEvent(name = "Activateta", guiName = "Trans-AM", active = true, guiActive = false)]
    public void Activateta()
    {
        taactivated = true;
        Events["Activateta"].guiActive = false;
        modified = true;
    }

    [KSPAction("Toggleag", KSPActionGroup.None, guiName = "Toggle Antigravity")]
    private void Toggleag(KSPActionParam param)
    {
        if (agActivated == true)
        {
            Deactivateag();
        }
        else
        {
            Activateag();
        }

    }

    [KSPEvent(name = "Activateag", guiName = "Activate Antigravity", active = true, guiActive = true)]
    public void Activateag()
    {
        this.part.force_activate();
        if (depleted == false)
        {
            agActivated = true;
            Events["Deactivateag"].guiActive = true;
            Events["Activateag"].guiActive = false;
            modified = true;
        }

    }

    [KSPEvent(name = "Deactivateag", guiName = "Deactivate Antigravity", active = true, guiActive = false)]
    public void Deactivateag()
    {
        agActivated = false;
        Events["Deactivateag"].guiActive = false;
        Events["Activateag"].guiActive = true;
        modified = true;
    }

    protected Transform rotorTransform = null;

    public override void OnStart(PartModule.StartState state)
    {
        part.stagingIcon = "LIQUID_ENGINE";
        if (state != StartState.Editor && state != StartState.None)
        {
            this.enabled = true;
            this.part.force_activate();
        }
        if (base.part.FindModelTransform("rotor").gameObject != null)
        {
            stator = base.part.FindModelTransform("stator").gameObject;
            rotor = base.part.FindModelTransform("rotor").gameObject;
        }

        if (particleSize <= 0.1F)
            particleSize = 0.5F;
        overloadtemp = Overload;

        GameObject o = Instantiate(
            UnityEngine.Resources.Load("Effects/shockExhaust_red_small")) as GameObject;
        o.transform.parent = transform;
        o.transform.localScale = Vector3.one * 50;
        o.transform.localEulerAngles = Vector3.zero;
        o.transform.localPosition = Vector3.zero;

        emt = o.particleEmitter;
        emt.emit = true;
        emt.useWorldSpace = false;
        emt.minEnergy = 0.6F;
        emt.maxEnergy = 0.7F;
        emt.minSize = 0.06F * particleSize;
        emt.maxSize = 0.1F * particleSize;
        emt.minEmission = 10;
        emt.maxEmission = emt.minEmission;
        emt.rndVelocity = Vector3.one * particleSize;
        emt.localVelocity = Vector3.down;
        emt.rndVelocity = Vector3.one * 5;
        (o.GetComponent<ParticleAnimator>() as ParticleAnimator).sizeGrow = 0;

    }


    public override void OnFixedUpdate()
    {
        ES = "Deactivated";
        if (part.Resources["GNparticle"].amount < part.Resources["GNparticle"].max)
        { }
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;
        float pitch = vessel.ctrlState.pitch;
        float roll = vessel.ctrlState.roll;
        float yaw = vessel.ctrlState.yaw;
        float throttle = vessel.ctrlState.mainThrottle * Overload;
        float y = -vessel.ctrlState.Y * Overload * 10;
        float x = -vessel.ctrlState.X * Overload * 10;
        float z = throttle * 10 - vessel.ctrlState.Z * Overload * 10;
        float enginecount = 0;
        float tefactor = 1;
        float ID = GetInstanceID();

        if (Overload != overloadtemp)
        {
            modified = true;
        }
        foreach (Part p in this.vessel.Parts)
        {
            foreach (PartModule m in p.Modules)
            {
                GNdrive drive = null;
                if (m.moduleName == "Taudrive")
                {
                    drive = (GNdrive)m;
                    enginecount += 1;
                    if (modified == true && drive.GetInstanceID() != GetInstanceID())
                    {
                        if (drive.modified == true)
                        {
                            taactivated = drive.taactivated;
                            agActivated = drive.agActivated;
                            engineIgnited = drive.engineIgnited;
                            Overload = drive.Overload;
                            overloadtemp = drive.Overload;

                        }
                        else
                        {
                            drive.taactivated = taactivated;
                            drive.agActivated = agActivated;
                            drive.engineIgnited = engineIgnited;
                            drive.Overload = Overload;
                            drive.overloadtemp = Overload;
                        }
                    }
                }
            }
        }

        modified = false;

        if (engineIgnited == true)
        {
            ES = "Activated";
            Events["Deactivate"].guiActive = true;
            Events["Activate"].guiActive = false;
        }
        else
        {
            Events["Deactivate"].guiActive = false;
            Events["Activate"].guiActive = true;
        }

        if (agActivated == true)
        {
            ES = "Activated";
            Events["Deactivateag"].guiActive = true;
            Events["Activateag"].guiActive = false;
        }
        else
        {
            Events["Deactivateag"].guiActive = false;
            Events["Activateag"].guiActive = true;
            if (engineIgnited == false)
            {
                taactivated = false;
                Events["Activateta"].guiActive = true;
            }
        }

        if (taactivated == true)
        {
            ES = "Trans-AM";
        }
        if (depleted == true)
        { 
            ES = "GNpartical depleted";
        }

        Vector3 srfVelocity = vessel.GetSrfVelocity();
        float VerticalV;
        VerticalV = (float)vessel.verticalSpeed;
        Vector3 Airspeed = vessel.transform.InverseTransformDirection(srfVelocity);
        Vector3 gee = FlightGlobals.getGeeForceAtPosition(this.vessel.transform.position) / enginecount;
        Vector3 controlforce = vessel.ReferenceTransform.up * z + vessel.ReferenceTransform.forward * y + vessel.ReferenceTransform.right * x;
        if (enginecount > maxenginecount)
        {
            ES = "Unsynchronized";
            controlforce = Vector3.zero;
            gee = Vector3.zero;
        }
        else
        {
            tefactor = (float)Math.Pow((double)particlegrate, (double)enginecount - 1);
        }

        if (engineIgnited == false)
        {
            controlforce = Vector3.zero;
        }
        if (agActivated == false)
        {
            gee = Vector3.zero;
        }
        float consumption = vessel.GetTotalMass() * Mathf.Abs((-gee + controlforce).magnitude) * fuelefficiency;
        float resourceDrawn = this.part.RequestResource("ElectricCharge", consumption);
        liftratio = resourceDrawn / consumption;
        float particlegen = particlegrate * tefactor;

        if (taactivated == true)
        {
            color = new Vector4(1F, 0F, 100F / 255F, 1F);
            controlforce *= 5;
            consumption = 4 * consumption - 3 * vessel.GetTotalMass() * Mathf.Abs(gee.magnitude) * fuelefficiency;
            Events["Activateta"].guiActive = false;
            consumption = Mathf.Max(particlegen, consumption) + 4;
        }
        else
        {
            color = new Vector4(0F, 1F, 170F / 255F, 1F);
            if (engineIgnited == true)
            {
                Events["Activateta"].guiActive = true;
            }

        }

        float reschange = (particlegen - consumption) * Time.deltaTime;

        if (part.Resources["GNparticle"].amount < -reschange)
        {
            depleted = false;
            controlforce = Vector3.zero;
            gee = Vector3.zero;
            taactivated = false;
            activated = false;
            engineIgnited = false;

        }

        else
        {
            if (part.Resources["GNparticle"].amount <= part.Resources["GNparticle"].maxAmount - reschange)
            {
                part.Resources["GNparticle"].amount += reschange;
            }

            else
            {
                part.Resources["GNparticle"].amount = part.Resources["GNparticle"].maxAmount;
            }
        }


        Debug.Log("Consumption " + consumption);
        Debug.Log("particlegen " + particlegen);
        Debug.Log("tefactor " + tefactor);
        Debug.Log("Time.deltaTime " + Time.deltaTime);

        mass = vessel.GetTotalMass().ToString("R");

        if (engineIgnited == true)
        {
            foreach (Part p in this.vessel.parts)
            {
                if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rigidbody != null))
                {
                    p.rigidbody.AddForce(controlforce * p.rigidbody.mass);
                }
            }
        }


        if (agActivated == true)
        {
            foreach (Part p in this.vessel.parts)
            {
                if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rigidbody != null))
                {
                    p.rigidbody.AddForce(-gee * p.rigidbody.mass);
                }
            }
        }

        emt.minEmission = 2 * (Mathf.Abs(controlforce.magnitude) + 5);
        emt.maxEmission = emt.minEmission;
        rotor.renderer.material.SetColor("_EmissiveColor", color);
        stator.renderer.material.SetColor("_EmissiveColor", color);
        stator.light.color = color;

        rotor.transform.localEulerAngles = new Vector3(0, 0, rotation);
        rotation += 6 * (Mathf.Abs(controlforce.magnitude) + 1) * 120 * TimeWarp.deltaTime;
        while (rotation > 360) rotation -= 360;
        while (rotation < 0) rotation += 360;

    }

}

