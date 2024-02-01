using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class JobQueue {
    Queue<Job> jobQueue;

    Action<Job> cbJobCreated;

    public JobQueue() {
        jobQueue = new Queue<Job>();
    }

    public void Enqueue(Job j) {
        if (j.jobTime < 0) {
            // Insta-complete the job.
            j.DoWork(0);
            return;
        }
        jobQueue.Enqueue(j);

        if (cbJobCreated != null) {
            cbJobCreated(j);
        }
    }

    public Job Dequeue() {
        if (jobQueue.Count == 0) {
            return null;
        }

        return jobQueue.Dequeue();
    }

    public void Remove (Job j) {
        // TODO: Optimze
        List<Job> jobs = new List<Job>(jobQueue);
        if (jobs.Contains(j) == false) {
            //Debug.LogError("Trying to remove a job that doesn't exist on the queue");
            // Most likely the job wasn't on the queue because the character is holding it.
            return;
        }
        jobs.Remove(j);
        jobQueue = new Queue<Job>(jobs);
    }
 
    public void RegisterJobCreationCallback(Action<Job> cb) {
        cbJobCreated += cb;
    }

    public void UnregisterJobCreationCallback(Action<Job> cb) {
        cbJobCreated -= cb;
    }
}
