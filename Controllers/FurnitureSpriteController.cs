using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Experimental.Rendering.Universal;

public class FurnitureSpriteController : MonoBehaviour {

    public GameObject lightLampGO;

    Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    Dictionary<string, Sprite> furnitureSprites;

    // The world and tile data.
    World world { get { return WorldController.Instance.world; } }

    private void Awake()
    {
        // Load all sprites from "Resources/" and store them in the sprite dictionary
        LoadSprites();
    }

    // Called before first frame.
    void Start() {
        // Instantiate Dictionary that tracks which GameObject is rendering which Tile or Furniture.
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        // Register our Callbacks
        world.RegisterFurnitureCreated(OnFurnitureCreated);

        // Loop through all existing furniture (i.e. from save) and call OnCreated
        foreach(Furniture furniture in world.furnitures) {
            OnFurnitureCreated(furniture);
        }
    }

    void LoadSprites() {
        
        furnitureSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Furniture/");

        foreach (Sprite s in sprites) {
            Debug.Log("Loading: " + s.name);
            furnitureSprites[s.name] = s;
        }
    }

    public void OnFurnitureCreated(Furniture furn) {
        // Create a visual GameObject linked to this data

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // Creates a new GameObject and adds it to our scene.
        GameObject furn_go = new GameObject();

        // Add Tile/GameObject pair to dictionary.
        furnitureGameObjectMap.Add(furn, furn_go);

        furn_go.name = furn.objectType + "_" + furn.tile.X + "_" + furn.tile.Y;
        furn_go.transform.position = new Vector3(furn.tile.X + ((furn.Width - 1)/2f), furn.tile.Y + ((furn.Height - 1) / 2f));
        furn_go.transform.SetParent(this.transform, true);

        // FIXME: This hardcoding is not ideal!
        if (furn.objectType == "Door") {
            // By default it is connected to East & West walls.
            Tile northTile = world.GetTileAt(furn.tile.X, furn.tile.Y + 1);
            Tile southTile = world.GetTileAt(furn.tile.X, furn.tile.Y - 1);

            if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null
                && northTile.furniture.objectType == "Wood Wall" && southTile.furniture.objectType == "Wood Wall") {
                furn_go.transform.rotation = Quaternion.Euler(0, 0, 90);

            }
        }

        SpriteRenderer furn_sr = furn_go.AddComponent<SpriteRenderer>();
        furn_sr.sprite = GetSpriteForFurniture(furn);
        furn_sr.sortingLayerName = "Furniture";

        if (furn.objectType == "Stockpile") { // FIXME: Hard-coded color assign
            furn_sr.color = new Color(UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), .3f);
            Tile t = furn.tile;
            int x = t.X;
            int y = t.Y;
            // Tile at North
            t = world.GetTileAt(x, y+1);
            if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
                furn_sr.color = furnitureGameObjectMap[t.furniture].GetComponent<SpriteRenderer>().color;
            }
            // Tile at East
            t = world.GetTileAt(x + 1, y);
            if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
                furn_sr.color = furnitureGameObjectMap[t.furniture].GetComponent<SpriteRenderer>().color;
            }
            // Tile at South
            t = world.GetTileAt(x, y - 1);
            if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
                furn_sr.color = furnitureGameObjectMap[t.furniture].GetComponent<SpriteRenderer>().color;
            }
            // Tile at West
            t = world.GetTileAt(x - 1, y);
            if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
                furn_sr.color = furnitureGameObjectMap[t.furniture].GetComponent<SpriteRenderer>().color;
            }
        }

        if (furn.objectType == "Lamp") {
            GameObject furn_light = Instantiate(lightLampGO, furn_go.transform);
            furn_light.transform.position += new Vector3(0,.5f,0);
            
        }

        // Register our callback so that our GameObject gets updated whenever it's info changes
        furn.RegisterOnChangedCallback(OnFurnitureChanged);
    }

    void OnFurnitureChanged(Furniture furn) {
        // Make sure the furniture's graphics are correct.

        if (furnitureGameObjectMap.ContainsKey(furn) == false) {
            Debug.LogError("OnFurnitureChanged -- trying to change visuals for furniture not in our map");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];
        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
    }

    public Sprite GetSpriteForFurniture(Furniture furniture) {

        string spriteName = furniture.objectType;

        // Check if it links to neighbour
        if (furniture.linksToNeighbour == false) {
            // If this is a door, check for its openness and update the sprite.
            // FIXME: All this hardcoding needs to be generalized later.
            if (furniture.objectType == "Door") {
                if (furniture.GetParameter("openness") < 0.1f) {
                    // Door is closed
                    spriteName = "Door";
                } else if (furniture.GetParameter("openness") < 0.5f) {
                    // Door is a bit open
                    spriteName = "Door_openness_1";
                } else if (furniture.GetParameter("openness") < 0.9f) {
                    // Door is almost open
                    spriteName = "Door_openness_2";
                } else {
                    // Door is fully open
                    spriteName = "Door_openness_3";
                }
            }
            return furnitureSprites[spriteName];
        }

        // Otherwise, the sprite name is more complicated.

        spriteName += " ";

        // Check for neighbours North, East, South, West

        int x = furniture.tile.X;
        int y = furniture.tile.Y;

        Tile t;
        //Tile t2;
        //Tile t3;

        // Tile at North
        t = world.GetTileAt(x, y + 1);
        if (t != null && t.furniture != null && t.furniture.objectType == furniture.objectType) {
            spriteName += "N";
        }
        // Tile at Northeast
        /*t = world.GetTileAt(x + 1, y + 1);
        t2 = world.GetTileAt(x, y + 1);
        t3 = world.GetTileAt(x + 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furniture.objectType 
            && t2 != null && t2.furniture != null && t2.furniture.objectType == furniture.objectType
            && t3 != null && t3.furniture != null && t3.furniture.objectType == furniture.objectType) {
            spriteName += "NE";
        }*/
        // Tile at East
        t = world.GetTileAt(x + 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furniture.objectType) {
            spriteName += "E";
        }
        // Tile at Southeast
        /*t = world.GetTileAt(x + 1, y - 1);
        t2 = world.GetTileAt(x, y - 1);
        t3 = world.GetTileAt(x + 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furniture.objectType
            && t2 != null && t2.furniture != null && t2.furniture.objectType == furniture.objectType
            && t3 != null && t3.furniture != null && t3.furniture.objectType == furniture.objectType) {
            spriteName += "SE";
        }*/
        // Tile at South
        t = world.GetTileAt(x, y - 1);
        if (t != null && t.furniture != null && t.furniture.objectType == furniture.objectType) {
            spriteName += "S";
        }
        // Tile at Southwest
        /*t = world.GetTileAt(x - 1, y - 1);
        t2 = world.GetTileAt(x, y - 1);
        t3 = world.GetTileAt(x - 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furniture.objectType
            && t2 != null && t2.furniture != null && t2.furniture.objectType == furniture.objectType
            && t3 != null && t3.furniture != null && t3.furniture.objectType == furniture.objectType) {
            spriteName += "SW";
        }*/
        // Tile at West
        t = world.GetTileAt(x - 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furniture.objectType) {
            spriteName += "W";
        }
        // Tile at Northwest
        /*t = world.GetTileAt(x - 1, y + 1);
        t2 = world.GetTileAt(x, y + 1);
        t3 = world.GetTileAt(x - 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furniture.objectType
            && t2 != null && t2.furniture != null && t2.furniture.objectType == furniture.objectType
            && t3 != null && t3.furniture != null && t3.furniture.objectType == furniture.objectType) {
            spriteName += "NW";
        }*/

        if (furnitureSprites.ContainsKey(spriteName) == false) {
            if (furnitureSprites.ContainsKey(furniture.objectType) == true)
            {
                return furnitureSprites[furniture.objectType];
            }
            Debug.LogError("GetSpriteForFurniture -- No sprites with name: " + spriteName);
            return null;
        }

        return furnitureSprites[spriteName];
    }

    public Sprite GetSpriteForFurniture(string furnitureType) {
        if (furnitureSprites.ContainsKey(furnitureType)) {
            return furnitureSprites[furnitureType];
        }

        if (furnitureSprites.ContainsKey(furnitureType + "_")) {
            return furnitureSprites[furnitureType + "_"];
        }

        Debug.LogError("GetSpriteForFurniture -- No sprites with name: " + furnitureType);
        return null;
    }

    public Dictionary<string, Sprite> getFurnitureSprites()
    {
        return furnitureSprites;
    }
}
