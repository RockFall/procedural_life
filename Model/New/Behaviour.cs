using System.Collections;
using System.Collections.Generic;

public class Behaviour
{
    private Lifeform owner;
    private JobQueue jobQueue;
    private Job currentJob;


    public void Setup(Lifeform lifeform)
    {
        owner = lifeform;
        jobQueue = new JobQueue();
    }

    public void OnUpdate(float deltaTime)
    {


    }
}
