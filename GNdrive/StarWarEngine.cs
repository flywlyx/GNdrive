using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class StarWarEngine : PartModule
{
    [KSPField]
    public float fuelefficiency = 1F;
    [KSPField]
    public float particlegrate = 1000F;
    [KSPField]
    public float maxenginecount = 2F;
    [KSPField]
    public bool IsOnRail = false;

    public Vector4 color = Vector4.zero;

    [KSPField(isPersistant = true)]
    public bool IsActivaed = false;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Inertia Percent", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 10f)]
    public float Inertia = 100f;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max Overload", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 5f, stepIncrement = 0.1f)]
    public float Overload = 1f;

    [KSPAction("Toggle Inertia control", KSPActionGroup.None)]
    private void ActionActivate(KSPActionParam param)
    {
        if (IsActivaed == true)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    [KSPEvent(name = "Activate", guiName = "Activate Inertia control", active = true, guiActive = true)]
    public void Activate()
    {
        this.part.force_activate();
        IsActivaed = true;
        Events["Deactivate"].guiActive = true;
        Events["Activate"].guiActive = false;
    }

    [KSPEvent(name = "Deactivate", guiName = "Deactivate Inertia control", active = true, guiActive = false)]
    public void Deactivate()
    {
        IsActivaed = false;
        Events["Deactivate"].guiActive = false;
        Events["Activate"].guiActive = true;
    }

    //[KSPEvent(name = "RailTrigger", guiName = "RailTrigger", active = true, guiActive = true)]
    //public void RailTrigger()
    //{
    //    if (IsOnRail)
    //    {
    //        this.vessel.GoOffRails();
    //    }
    //    else
    //    {
    //        this.vessel.GoOnRails();
    //    }
    //    IsOnRail = !IsOnRail;
    //}
    public override void OnStart(PartModule.StartState state)
    {
        part.stagingIcon = "LIQUID_ENGINE";
        if (state != StartState.Editor && state != StartState.None)
        {
            this.enabled = true;
            this.part.force_activate();
        }



    }
    public override void OnFixedUpdate()
    {
        if (IsActivaed& this.vessel.ActionGroups.groups[3])
        {
            Vector3 srfVelocity = this.vessel.GetSrfVelocity();
            Vector3 Airspeed = this.vessel.transform.InverseTransformDirection(srfVelocity);
            float DirFlag = 0;
            if (Airspeed.y<0)
            {
                DirFlag = 2;
            }
            Vector3 yawsForce = Airspeed.x * -this.vessel.transform.right;
            Vector3 pitchsForce = Airspeed.z * -this.vessel.transform.forward;
            Vector3 FrontForce = Airspeed.y * -this.vessel.transform.up* DirFlag;
            foreach (Part p in this.vessel.parts)
            {
                if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                {
                    p.AddForce((yawsForce + pitchsForce + FrontForce) * p.rb.mass*Inertia/100/Time.fixedDeltaTime);
                }
            }
        }
        //if (IsOnRail)
        //{
        //    this.vessel.GoOnRails();
        //    if (this.vessel.packed)
        //    {
        //        this.vessel.SetWorldVelocity(Vector3.zero);
        //        this.vessel.angularVelocity = Vector3.zero;
        //        this.vessel.angularMomentum = Vector3.zero;
        //    }
        //    else
        //    {
        //        this.vessel.packed = true;
        //        this.vessel.SetWorldVelocity(Vector3.zero);
        //        this.vessel.angularVelocity = Vector3.zero;
        //        this.vessel.angularMomentum = Vector3.zero;
        //    }
        //}
        if (this.vessel.ActionGroups.groups[3])
        {
            float y = -vessel.ctrlState.Y * Overload * 10;
            float x = -vessel.ctrlState.X * Overload * 10;
            float z =- vessel.ctrlState.Z * Overload * 10;
            Vector3 gee = FlightGlobals.getGeeForceAtPosition(this.vessel.transform.position);
            Vector3 controlforce = vessel.ReferenceTransform.up * z + vessel.ReferenceTransform.forward * y + vessel.ReferenceTransform.right * x;
            foreach (Part p in this.vessel.parts)
            {
                if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                {
                    p.AddForce(controlforce * p.rb.mass);
                }
            }

        }
    }

}

