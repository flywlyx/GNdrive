using System;
using UnityEngine;


public class Taudrive : PartModule
{
    [KSPField]
    public float fuelefficiency = 1F;
    [KSPField]
    public float particlegrate = 800F;
    [KSPField]
    public float ConvertRatio = 1F;
    public Vector4 color = Vector4.zero;


    [KSPField(isPersistant = true)]
    public bool engineIgnited = false;
    public bool flameOut = false;
    public bool agActivated = false;
    public bool depleted = false;
    public bool ecActivated = false;
    Transform EMITransform;
    KSPParticleEmitter Emitter;


    //    public bool staged = false;
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
        if (depleted == false)
        {
            engineIgnited = true;
            Events["Deactivate"].guiActive = true;
            Events["Activate"].guiActive = false;
        }
    }

    [KSPEvent(name = "Deactivate", guiName = "Deactivate Engine", active = true, guiActive = false)]
    public void Deactivate()
    {
        engineIgnited = false;
        Events["Deactivate"].guiActive = false;
        Events["Activate"].guiActive = true;
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
        if (depleted == false)
        {
            agActivated = true;
            Events["Deactivateag"].guiActive = true;
            Events["Activateag"].guiActive = false;
        }

    }

    [KSPEvent(name = "Deactivateag", guiName = "Deactivate Antigravity", active = true, guiActive = false)]
    public void Deactivateag()
    {
        agActivated = false;
        Events["Deactivateag"].guiActive = false;
        Events["Activateag"].guiActive = true;
    }

    [KSPEvent(name = "Activateec", guiName = "Activate Converter", active = true, guiActive = true)]
    public void Activateec()
    {
            ecActivated = true;
            Events["Deactivateec"].guiActive = true;
            Events["Activateec"].guiActive = false;
    }

    [KSPEvent(name = "Deactivateec", guiName = "Deactivate Converter", active = true, guiActive = false)]
    public void Deactivateec()
    {
        ecActivated = false;
        Events["Deactivateec"].guiActive = false;
        Events["Activateec"].guiActive = true;
    }

    protected Transform rotorTransform = null;

    public override void OnStart(PartModule.StartState state)
    {
        part.stagingIcon = "LIQUID_ENGINE";
        base.OnStart(state);
        {
            if (state != StartState.Editor && state != StartState.None)
            {
                this.enabled = true;
                this.part.force_activate();
            }
            else
            {
                this.enabled = false;
            }
            if (base.part.FindModelTransform("rotor").gameObject != null)
            {
                stator = base.part.FindModelTransform("stator").gameObject;
                rotor = base.part.FindModelTransform("rotor").gameObject;
            }

            EMITransform = base.part.FindModelTransform("EMI");
            Emitter = EMITransform.gameObject.GetComponent<KSPParticleEmitter>();
            Emitter.emit = false;


        }
    }


    public override void OnFixedUpdate()
    {
        ES = "Deactivated";
        if (depleted == true && part.Resources["GNparticle"].amount == part.Resources["GNparticle"].maxAmount)
        {
            depleted = false;
        }
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

        foreach (Part p in this.vessel.Parts)
        {
            foreach (PartModule m in p.Modules)
            {
                Taudrive drive = null;
                GNdrive gdrive = null;
                if (m.moduleName == "Taudrive")
                {
                    drive = (Taudrive)m;
                    if (drive.agActivated == true)
                    { 
                        enginecount += 1; 
                    }
                    //if (modified == true && drive.GetInstanceID() != GetInstanceID())
                    //{
                    //    if (drive.modified == true)
                    //    {
                    //        agActivated = drive.agActivated;
                    //        engineIgnited = drive.engineIgnited;
                    //        Overload = drive.Overload;
                    //        overloadtemp = drive.Overload;
                    //
                    //    }
                    //    else
                    //    {
                    //        drive.agActivated = agActivated;
                    //        drive.engineIgnited = engineIgnited;
                    //        drive.Overload = Overload;
                    //        drive.overloadtemp = Overload;
                    //    }
                    //}
                }
                else
                    if (m.moduleName == "GNdrive")
                    {
                        gdrive = (GNdrive)m;
                        if (gdrive.agActivated == true)
                        {
                            enginecount += 1;
                        }
                    }
            }
        }

        if (engineIgnited == true)
        {
            ES = "Activated";
            Events["Deactivate"].guiActive = true;
            Events["Activate"].guiActive = false;
            Emitter.emit = true;
        }
        else
        {
            Events["Deactivate"].guiActive = false;
            Events["Activate"].guiActive = true;
            Emitter.emit = false;
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
        }

        if (depleted == true)
        {
            ES = "GNparticle depleted";
            Deactivate();
            Deactivateag();
        }

        Vector3 srfVelocity = vessel.GetSrfVelocity();
        float VerticalV;
        VerticalV = (float)vessel.verticalSpeed;
        Vector3 Airspeed = vessel.transform.InverseTransformDirection(srfVelocity);
        Vector3 gee = FlightGlobals.getGeeForceAtPosition(this.vessel.transform.position) / enginecount;
        Vector3 controlforce = vessel.ReferenceTransform.up * z + vessel.ReferenceTransform.forward * y + vessel.ReferenceTransform.right * x;
        //if (enginecount > maxenginecount)
        //{
        //    ES = "Unsynchronized";
        //   controlforce = Vector3.zero;
        //    gee = Vector3.zero;
        //}

        if (engineIgnited == false)
        {
            controlforce = Vector3.zero;
        }
        if (agActivated == false)
        {
            gee = Vector3.zero;
        }

        double consumption = vessel.GetTotalMass() * Mathf.Abs((-gee + controlforce).magnitude) * fuelefficiency * TimeWarp.fixedDeltaTime;
        if (ecActivated == true)
        {
            double elcconsume = particlegrate * ConvertRatio * TimeWarp.fixedDeltaTime;
            double elcDrawn = this.part.RequestResource("ElectricCharge", elcconsume);
            double ratio = elcDrawn / elcconsume;
            double GNDrawn = this.part.RequestResource("GNparticle", -(elcconsume / ConvertRatio) * ratio);
            double backcharge = this.part.RequestResource("ElectricCharge",-GNDrawn * ConvertRatio - elcDrawn);
        }

        double GNconsumtion = this.part.RequestResource("GNparticle", consumption);
        color = new Vector4(1F, 0F, 36F / 255F, 1F);

        if (consumption !=0 && Math.Round(GNconsumtion,5) < Math.Round(consumption,5))
        {
            depleted = true;
            controlforce = Vector3.zero;
            gee = Vector3.zero;
            agActivated = false;
            engineIgnited = false;
            Deactivate();
            Deactivateag();
            part.Resources["GNparticle"].amount = 0;

        }

        //Debug.Log("Consumption " + consumption);
        //Debug.Log("tefactor " + tefactor);
        //Debug.Log("Time.deltaTime " + TimeWarp.deltaTime);

        mass = vessel.GetTotalMass().ToString("R");

        if (engineIgnited == true)
        {
            foreach (Part p in this.vessel.parts)
            {
                if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                {
                    p.AddForce(controlforce * p.rb.mass);
                }
            }
        }


        if (agActivated == true)
        {
            foreach (Part p in this.vessel.parts)
            {
                if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                {
                    p.AddForce(-gee * p.rb.mass);
                }
            }
        }
        rotor.GetComponent<Renderer>().material.SetColor("_EmissiveColor", color);
        stator.GetComponent<Renderer>().material.SetColor("_EmissiveColor", color);
        stator.GetComponent<Light>().color = color;

        rotor.transform.localEulerAngles = new Vector3(0, rotation, 0);
        rotation += 6 * (Mathf.Abs(controlforce.magnitude) + 1) * 120 * TimeWarp.deltaTime;
        while (rotation > 360) rotation -= 360;
        while (rotation < 0) rotation += 360;

    }

}

