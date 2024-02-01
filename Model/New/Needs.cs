using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needs : MonoBehaviour
{
    public List<Need> NeedsList { get; protected set; }

    public Needs()
    {
        NeedsList = new List<Need>();
    }

    public void AddNeed(Need need)
    {
        NeedsList.Add(need);
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (Need need in NeedsList)
        {
            need.OnUpdate(deltaTime);
        }
    }
}
