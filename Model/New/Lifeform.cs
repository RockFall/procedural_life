using System.Collections;
using System.Collections.Generic;

public class Lifeform
{
    public LivingEntity LivingEntityData { get; protected set; }
    public BodyParts BodyParts { get; protected set; }
    public Needs Needs { get; protected set; }
    public Behaviour Behaviour { get; protected set; }
    public LAttributes Attributes { get; protected set; }

    // public Senses Senses { get; protected set; }
    

    public void Initialize(LivingEntity livingEntityData, BodyParts organs, Needs needs, Behaviour behaviour, LAttributes attributes)
    {
        LivingEntityData = livingEntityData;
        Organs = organs;
        Needs = needs;
        Behaviour = behaviour;
        Attributes = attributes;
    }

    public void OnUpdate(float deltaTime)
    {
        Needs.OnUpdate(deltaTime);
        Behaviour.OnUpdate(deltaTime);
    }
}
