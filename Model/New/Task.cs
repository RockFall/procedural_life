using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task
{
    public string Name { get; protected set; }

    // In what tile the task's happening
    public Tile tile;

    // Time to complete the task
    public float TaskTime { get; protected set; }

    // Callback for a completed ou cancelled task
    Action<Task> cbTaskComplete;
    Action<Task> cbTaskCancel;
    Action<Task> cbTaskWorked;

    private Queue<Action> subtasks;
    private Func<bool> decisionPoint;

    public Dictionary<string, GameItem> GameItemRequirements { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Task"/> class.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="cbTaskComplete"></param>
    /// <param name="taskTime"></param>
    /// <param name="gameItemRequirementsList"></param>
    public Task(Tile tile, Action<Task> cbTaskComplete, float taskTime, GameItem[] gameItemRequirementsList)
    {
        this.tile = tile;
        //this.taskObjectType = TaskObjectType;
        this.cbTaskComplete += cbTaskComplete;
        this.TaskTime = taskTime;

        this.GameItemRequirements = new Dictionary<string, GameItem>();
        if (gameItemRequirementsList != null)
        {
            foreach (GameItem item in gameItemRequirementsList)
            {
                this.GameItemRequirements[item.Name] = item.Clone();

            }
        }
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    /// <param name="other"></param>
    protected Task(Task other)
    {
        this.tile = other.tile;
        //this.taskObjectType = other.taskObjectType;
        this.cbTaskComplete = other.cbTaskComplete;
        this.TaskTime = other.TaskTime;

        this.GameItemRequirements = new Dictionary<string, GameItem>();
        if (other.GameItemRequirements != null)
        {
            foreach (GameItem item in other.GameItemRequirements.Values)
            {
                this.GameItemRequirements[item.Name] = item.Clone();
            }
        }
    }

    virtual public Task Clone()
    {
        return new Task(this);
    }

    /// <summary>
    /// Adds a subtask to the task.
    /// </summary>
    /// <param name="subtask"></param>
    public void AddSubtask(Action subtask)
    {
        if (subtasks == null)
        {
            subtasks = new Queue<Action>();
        }
        subtasks.Enqueue(subtask);
    }

    /// <summary>
    /// Sets the decision point for the task.
    /// </summary>
    /// <param name="decisionFunc"></param>
    public void SetDecisionPoint(Func<bool> decisionFunc)
    {
        decisionPoint = decisionFunc;
    }

    public void PerformTask()
    {
        if (subtasks.Count > 0)
        {
            Action subtask = subtasks.Dequeue();
            subtask();
        }
        else if (decisionPoint != null)
        {
            bool decisionResult = decisionPoint();
            if (!decisionResult)
            {
                EnqueueNextSubtasks();
            }
        }
    }
    private void EnqueueNextSubtasks()
    {
        // Logic to enqueue the next set of subtasks based on decision result
    }
}
