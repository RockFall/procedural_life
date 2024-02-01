using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LAttributes
{
    // Holds a list of attributes (as a map) that a Lifeform may have

    public Dictionary<string, LAttribute> Attributes { get; protected set; }
    public LAttributes()
    {
        Attributes = new Dictionary<string, LAttribute>();
    }

    public void Add(string name, LAttribute attribute)
    {
        Attributes.Add(name, attribute);
    }

    public void Remove(string name)
    {
        Attributes.Remove(name);
    }

    public void Add(string name, float maxValue, float minValue, float value=0, string description="")
    {
        Attributes.Add(name, new LAttribute(name, maxValue, minValue, value, description));
    }
        
    public LAttribute GetAttributeByName(string name)
    {
        return Attributes[name];
    }
    public float GetAttributeValueByName(string name)
    {
        return Attributes[name].Value;
    }
}
