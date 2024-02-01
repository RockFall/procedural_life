using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyParts : MonoBehaviour
{
    // Holds a list of BodyParts (as a map) that a Lifeform may have

    public Dictionary<string, BodyPart> BodyPartsMap { get; protected set; }
    public BodyParts()
    {
        BodyPartsMap = new Dictionary<string, BodyPart>();
    }

    public void Add(string name, BodyPart BodyPart)
    {
        BodyPartsMap.Add(name, BodyPart);
    }

    public void Remove(string name)
    {
        BodyPartsMap.Remove(name);
    }

    public void Add(string name, float maxValue, float minValue, float value=0, string description="")
    {
        BodyPartsMap.Add(name, new BodyPart(name, maxValue, minValue, value, description));
    }
        
    public BodyPart GetBodyPartsByName(string name)
    {
        return BodyPartsMap[name];
    }
    public float GetBodyPartsValueByName(string name)
    {
        return BodyPartsMap[name];
    }

}
