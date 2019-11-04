using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class PidController
{
    private float mKp, mKd, mKi;
    private float mOldVal, mOldTime, mOldD;
    private float mClamp;
    private float[] mBuffer = null;
    private int mPtr;
    private float mSum;
    private float mValue;

    public PidController(float Kp, float Ki, float Kd,
                          int integrationBuffer, float clamp)
    {
        mKp = Kp;
        mKi = Ki;
        mKd = Kd;
        mClamp = clamp;
        if (integrationBuffer >= 1)
            mBuffer = new float[integrationBuffer];
        Reset();
    }

    public void Reset()
    {
        mSum = 0;
        mOldTime = -1;
        mOldD = 0;
        if (mBuffer != null)
            for (int i = 0; i < mBuffer.Length; i++)
                mBuffer[i] = 0;
        mPtr = 0;
    }

    public float Control(float v)
    {
        if (Time.fixedTime > mOldTime)
        {
            if (mOldTime >= 0)
            {
                mOldD = (v - mOldVal) / (Time.fixedTime - mOldTime);

                float i = v / (Time.fixedTime - mOldTime);
                if (mBuffer != null)
                {
                    mSum -= mBuffer[mPtr];
                    mBuffer[mPtr] = i;
                    mPtr++;
                    if (mPtr >= mBuffer.Length)
                        mPtr = 0;
                }
                mSum += i;
            }

            mOldTime = Time.fixedTime;
            mOldVal = Value;
        }

        mValue = mKp * v + mKi * mSum + mKd * mOldD;

        if (mClamp > 0)
        {
            if (mValue > mClamp)
                mValue = mClamp;
            if (mValue < -mClamp)
                mValue = -mClamp;
        }

        return mValue;
    }

    public float Value
    {
        get { return mValue; }
    }

    public static implicit operator float(PidController v)
    {
        return v.mValue;
    }

    private float Step(float value, bool up)
    {
        float order = Mathf.Log10(value);
        if (order < 0)
            order--;
        order = (int)order;
        float exp = Mathf.Pow(10, order);
        float num = Mathf.Round(value / exp);
        if (up)
        {
            if (num >= 10)
                num += 10;
            else
                num++;
        }
        else {
            if (num <= 1)
                num -= 0.1F;
            else
                num--;
        }
        return num * exp;
    }

    public void Calibrate()
    {
        if (Input.GetKeyDown("[7]"))
        {
            mKp = Step(mKp, false);
            Debug.Log("Kp: " + mKp);
        }
        if (Input.GetKeyDown("[9]"))
        {
            mKp = Step(mKp, true);
            Debug.Log("Kp: " + mKp);
        }

        if (Input.GetKeyDown("[4]"))
        {
            mKi = Step(mKi, false);
            Debug.Log("Ki: " + mKi);
        }
        if (Input.GetKeyDown("[6]"))
        {
            mKi = Step(mKi, true);
            Debug.Log("Ki: " + mKi);
        }

        if (Input.GetKeyDown("[1]"))
        {
            mKd = Step(mKd, false);
            Debug.Log("Kd: " + mKd);
        }
        if (Input.GetKeyDown("[3]"))
        {
            mKd = Step(mKd, true);
            Debug.Log("Kd: " + mKd);
        }
    }

    public void Calibrateclamp(float newclamp)
    {
        mClamp = newclamp;
    }
}
