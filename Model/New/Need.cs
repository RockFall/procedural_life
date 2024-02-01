using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Need
{
    public string Name { get; protected set; }
    public float Value { get; protected set; } // Current value 0-100
    public float DecayRate { get; protected set; } // How much the value decreases per second
    //public float DecayRateModifier { get; protected set; } // How much the decay rate is modified by (e.g. 0.5 = 50%)
    //public float DecayRateModifierDuration { get; protected set; } // How long the decay rate modifier lasts
    //public float DecayRateModifierTimer { get; protected set; } // How long the decay rate modifier has been active

    public Need(string name, float value, float decayRate)
    {
        Name = name;
        Value = value;
        DecayRate = decayRate;
    }

    public void OnUpdate(float deltaTime)
    {
        Value -= DecayRate * deltaTime;
        Value = Mathf.Clamp(Value, 0, 100);
    }

    /// <summary>
    /// Satisfies the need by the given amount
    /// </summary>
    /// <param name="amount"></param>
    public void Satisfy(float amount)
    {
        Value += amount;
        Value = Mathf.Clamp(Value, 0, 100);
    }
}
