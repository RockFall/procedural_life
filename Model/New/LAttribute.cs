using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LAttribute
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public float MaxValue { get; protected set; }
    public float MinValue { get; protected set; }
    public float Value { get; protected set; }

    public LAttribute(string name="", float maxValue=0, float minValue=0, float value = 0, string description="")
    {
        Name = name;
        Value = value;
        MaxValue = maxValue;
        MinValue = minValue;
        Description = description;
    }

    public void Add(float value)
    {
        Value += value;
        if (Value > MaxValue)
        {
            Value = MaxValue;
        }
    }

    public void Subtract(float value)
    {
        Value -= value;
        if (Value < MinValue)
        {
            Value = MinValue;
        }
    }

    public void Set(float value)
    {
        Value = value;
        if (Value > MaxValue)
        {
            Value = MaxValue;
            Debug.Log("Trying to set a value (" + Value + ") for" + Name + "greater than the max value (" + MaxValue + ")");
        }
        else if (Value < MinValue)
        {
            Value = MinValue;
            Debug.Log("Trying to set a value (" + Value + ") for" + Name + "less than the min value (" + MinValue + ")");
        }
    }
    
}
