using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseController : MonoBehaviour
{
    enum HoldingType
    {
        None,
        Continuous,
        Toggle
    }

    BuildModeController bmc;

    public GameObject circleCursorPrefab;

    // World Position of Mouse
    Vector3 lastFramePosition;
    Vector3 currFramePosition;

    // Area selection operation
    Vector3 dragStartPosition;
    List<GameObject> dragPreviewGameObjects;
    Inventory holdingInventory;
    bool isDragging = false;
    private HoldingType holdingType = HoldingType.None;

    // Start is called before the first frame update
    void Start()
    {
        bmc = GameObject.FindObjectOfType<BuildModeController>();
        dragPreviewGameObjects = new List<GameObject>();

        SimplePool.Preload(circleCursorPrefab, 100);
    }

    /// <summary>
    /// Gets the mouse position in world space.
    /// </summary>
    public Vector3 GetMousePosition() {
        return currFramePosition;
    }

    public Tile GetTileUnderMouse() {
        return WorldController.Instance.GetTileAtWorldCoord(currFramePosition);
    }

    // Update is called once per frame
    void Update()
    {
        currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0;

        //UpdateCursor();
        UpdateDragging();
        UpdateItemDragging();
        UpdateCameraMovement();

        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePosition.z = 0;
    }
    void UpdateDragging() {
            // ---- Start Drag
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            isDragging = true;
            dragStartPosition = currFramePosition;
        }

        if (Input.GetMouseButtonUp(1)) {
            // If RIGHT mouse buttom is released we are
            // cancelling current action
            isDragging = false;
        }

        if ( bmc.IsObjectDraggable() == false ) { 
            dragStartPosition = currFramePosition;
        }

        int start_x = Mathf.FloorToInt(dragStartPosition.x + 0.5f);
        int end_x = Mathf.FloorToInt(currFramePosition.x + 0.5f);
        int start_y = Mathf.FloorToInt(dragStartPosition.y + 0.5f);
        int end_y = Mathf.FloorToInt(currFramePosition.y + 0.5f);

        if (isDragging) {
            // Flip if draggin in the "wrong" direction
            if (end_x < start_x) {
                int tmp = end_x;
                end_x = start_x;
                start_x = tmp;
            }
            if (end_y < start_y) {
                int tmp = end_y;
                end_y = start_y;
                start_y = tmp;
            }
        }

        // Clean up old drag previews
        while (dragPreviewGameObjects.Count > 0) {
            GameObject go = dragPreviewGameObjects[0];
            dragPreviewGameObjects.RemoveAt(0);
            SimplePool.Despawn(go);
        }

        if (isDragging) {
            // ---- Keep Drag
            // Loop through all the selected tiles
            for (int x = start_x; x <= end_x; x++) {
                for (int y = start_y; y <= end_y; y++) {
                    Tile t = WorldController.Instance.world.GetTileAt(x, y);
                    if (t != null) {
                        // Display building semi transparent object
                        GameObject go = SimplePool.Spawn(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        //go.GetComponent<SpriteRenderer>().sprite = 
                        //   GameObject.FindObjectOfType<FurnitureSpriteController>().GetSpriteForFurniture(GameObject.FindObjectOfType<BuildModeController>().buildModeObjectType);
                        go.transform.SetParent(this.transform, true);
                        dragPreviewGameObjects.Add(go);
                    }
                }
            }
        }

            // ---- End Drag
        if (Input.GetMouseButtonUp(0) && isDragging) {

            isDragging = false;
            // Loop through all the selected tiles
            for (int x = start_x; x <= end_x; x++) {
                for (int y = start_y; y <= end_y; y++) {
                    Tile t = WorldController.Instance.world.GetTileAt(x, y);

                    if (t != null) {
                        // Call BuildModeController::DoBuild()
                        bmc.DoBuild(t);
                    }
                }
            }
        }
    }

    void UpdateCameraMovement() {
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1)) { // Right or Middle Mouse Buttom Hold
            Vector3 positionDifferenceFromLastFrame = lastFramePosition - currFramePosition;
            Camera.main.transform.Translate(positionDifferenceFromLastFrame);
        }

        Camera.main.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * Camera.main.orthographicSize/2;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 30f);
    }

    public void StartHoldingItem(Inventory inv, bool mustKeepPressing)
    {
        if (mustKeepPressing)
        {
            holdingType = HoldingType.Continuous;
        } else
        {
            holdingType = HoldingType.Toggle;
        }

        GameObject.FindObjectOfType<InventorySpriteController>().CreateInventoryView(inv, currFramePosition);

        holdingInventory = inv;
    }

    void UpdateItemDragging()
    {
        if (holdingInventory == null)
        {
            return;
        }

        if (holdingType == HoldingType.Continuous)
        {
            if (Input.GetMouseButton(0))
            {
                // Item at mouse position
                GameObject.FindObjectOfType<InventorySpriteController>().inventoryViewGameObjectMap[holdingInventory].transform.position = currFramePosition;
            }
            else
            {
                // Place item on World
                WorldController.Instance.world.inventoryManager.PlaceInventory(GetTileUnderMouse(), holdingInventory);
                holdingType = HoldingType.None;
                holdingInventory = null;
            }
        }
        else if (holdingType == HoldingType.Toggle)
        {
            GameObject.FindObjectOfType<InventorySpriteController>().inventoryViewGameObjectMap[holdingInventory].transform.position = currFramePosition;
            if (Input.GetMouseButtonDown(0))
            {
                holdingType = HoldingType.Continuous;
            }
        }   

    }


}
