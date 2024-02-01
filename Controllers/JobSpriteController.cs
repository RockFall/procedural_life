using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSpriteController : MonoBehaviour
{
    public Sprite buildSprite;

    FurnitureSpriteController fsc;
    Dictionary<Job, GameObject> jobGameObjectMap;

    // Start is called before the first frame update
    void Start()
    {
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();

        WorldController.Instance.world.jobQueue.RegisterJobCreationCallback(OnJobCreated);
    }

    void OnJobCreated(Job job) {
        
        if (job.jobObjectType == null) {
            // This job doesn't really have an associated sprite with it, so no need to render.
            return;
        }
        
        // FIXME: We can only do furniture-building jobs.


        if (jobGameObjectMap.ContainsKey(job)) {
            Debug.LogError("OnJobCreated for a jobGo that already exists -- most likely, a job being RE-QUEUED, as opposed to created.");
            // Can't do job right now, do wander.
            return;
        }

        GameObject job_go = new GameObject();

        // Add our job/GO pair to the dictionary
        jobGameObjectMap.Add(job, job_go);

        job_go.name = "JOB_" + job.jobObjectType + "_" + job.tile.X + "_" + job.tile.Y;
        job_go.transform.position = new Vector3(job.tile.X + ((job.furniturePrototype.Width - 1) / 2f), job.tile.Y + ((job.furniturePrototype.Height - 1) / 2f));
        job_go.transform.SetParent(this.transform, true);

        SpriteRenderer job_sr = job_go.AddComponent<SpriteRenderer>(); // FIXME:
        job_sr.sprite = fsc.GetSpriteForFurniture(job.jobObjectType);
        //job_sr.sprite = buildSprite;
        job_sr.sortingLayerName = "Over Tile";
        job_sr.color = new Color(1f, 1f, 1f, 0.4f);
        job_sr.sortingLayerName = "Jobs";

        // FIXME: This hardcoding is not ideal!
        if (job.jobObjectType == "Door") {
            // By default it is connected to East & West walls.
            Tile northTile = job.tile.world.GetTileAt(job.tile.X, job.tile.Y + 1);
            Tile southTile = job.tile.world.GetTileAt(job.tile.X, job.tile.Y - 1);

            if (northTile != null && southTile != null && ((northTile.pendingFurnitureJob != null && southTile.pendingFurnitureJob != null
                && northTile.pendingFurnitureJob.jobObjectType == "Wall" && southTile.pendingFurnitureJob.jobObjectType == "Wall") || 
                (northTile.furniture != null && southTile.furniture != null && northTile.furniture.objectType == "Wall" && 
                southTile.furniture.objectType == "Wall"))) {
                job_go.transform.rotation = Quaternion.Euler(0, 0, 90);

            }
        } 

        job.RegisterJobCompleteCallback(OnJobEnded);
        job.RegisterJobCancelCallback(OnJobEnded);
    }

    public void SetToBuildSprite(Job job) {
        SpriteRenderer sr = jobGameObjectMap[job].GetComponent<SpriteRenderer>();
        sr.sprite = buildSprite;
        sr.color = new Color(1f, 1f, 1f, 0.7f);
    }

    void OnJobEnded(Job job) {
        // This executes whether a job was COMPLETED or CANCELLED.

        // FIXME: We can only do furniture-building jobs.

        GameObject job_go = jobGameObjectMap[job];

        job.UnregisterJobCompleteCallback(OnJobEnded);
        job.UnregisterJobCancelCallback(OnJobEnded);

        Destroy(job_go);
    }
}
