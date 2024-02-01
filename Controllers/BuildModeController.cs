using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class BuildModeController : MonoBehaviour
{
    bool buildModeIsFurnitures = false;
    TileType buildModeTile = TileType.Floor;
    public string buildModeObjectType { get; protected set; }

    GameObject furniturePreview;
    FurnitureSpriteController fsc;

    MouseController mouseController;

    // Start is called before the first frame update
    void Start()
    {
        fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();
        mouseController = GameObject.FindObjectOfType<MouseController>();

        furniturePreview = new GameObject();
        furniturePreview.transform.SetParent(this.transform);
        furniturePreview.AddComponent<SpriteRenderer>();
        furniturePreview.SetActive(false);
    }

    private void Update() {
        if (buildModeIsFurnitures == true && buildModeObjectType != null && buildModeObjectType != "") {
            // Show a transparent preview of the object that is color-coded based on buildability

            ShowFurnitureSpriteAtTile( buildModeObjectType, mouseController.GetTileUnderMouse() );
        } else {
            furniturePreview.SetActive(false);
        }
    }

    void ShowFurnitureSpriteAtTile (string furnitureType, Tile t) {
        furniturePreview.SetActive(true);

        SpriteRenderer job_sr = furniturePreview.GetComponent<SpriteRenderer>();
        job_sr.sprite = fsc.GetSpriteForFurniture(furnitureType);

        if (WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t)) {
            job_sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        } else {
            job_sr.color = new Color(1f, 0.5f, 0.5f, 0.25f);
        }

        job_sr.sortingLayerName = "Jobs";

        Furniture proto = t.world.furniturePrototypes[furnitureType];

        furniturePreview.transform.position = new Vector3(t.X + ((proto.Width - 1) / 2f), t.Y + ((proto.Height - 1) / 2f));
    }

    public void SetMode_BuildFloor() {
        buildModeIsFurnitures = false;
        buildModeTile = TileType.Floor;
    }

    public void SetMode_Bulldoze() {
        buildModeIsFurnitures = false;
        buildModeTile = TileType.Empty;
    }

    public void SetMode_BuildFurniture(string objectType) {
        buildModeIsFurnitures = true;
        buildModeObjectType = objectType;
    }

    public void SetupPathfindingExample() {
        WorldController.Instance.world.SetupPathfindingExample();
    }

    public void DoBuild ( Tile t) {
        if (buildModeIsFurnitures) {
            // Create Furniture and assign it to the tile

            // FIXME:
            //WorldController.Instance.World.PlaceFurniture(buildModeObjectType, t);

            string furnitureType = buildModeObjectType;

            // Can we build the furniture in the selected tile?
            if (WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t) && t.pendingFurnitureJob == null) {
                // This tile is valid for this furniture

                // Create the job
                Job j;

                if (WorldController.Instance.world.furnitureJobPrototypes.ContainsKey(furnitureType)) {
                    // Make a clone of the job prototype.
                    j = WorldController.Instance.world.furnitureJobPrototypes[furnitureType].Clone();

                    // Assign the correct tile.
                    j.tile = t;
                } else {
                    Debug.LogError("There is no furniture job prototype for '" + furnitureType + "'!");
                    j = new Job(t, furnitureType, FurnitureActions.JobComplete_FurnitureBuilding, 0.32f, null);
                }

                j.furniturePrototype = WorldController.Instance.world.furniturePrototypes[furnitureType]; 

                // FIXME: manually and explicity flagging to prevent conflicts is BAD
                t.pendingFurnitureJob = j;

                j.RegisterJobCancelCallback((theJob) => { theJob.tile.pendingFurnitureJob = null; });

                // Add the job created to the queue
                WorldController.Instance.world.jobQueue.Enqueue(j);
            }
        } else {
            // We are in tile-changing mode
            t.Type = buildModeTile;
        }
    }

    public bool IsObjectDraggable() {
        if (buildModeIsFurnitures == false) {
            // Floor or Bulldoze
            return true;
        }

        Furniture proto = WorldController.Instance.world.furniturePrototypes[buildModeObjectType];
        return proto.linksToNeighbour;
    }

}
