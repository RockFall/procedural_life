using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileSpriteController : MonoBehaviour {

    public Sprite floorSprite; // FIXME:
    public Sprite emptySprite; // FIXME:

    Dictionary<Tile, GameObject> tileGameObjectMap;

    // The world and tile data.
    World world { get { return WorldController.Instance.world; } }

    // Called before first frame.
    void Start() {

        // Instantiate Dictionary that tracks which GameObject is rendering which Tile or Furniture.
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        // Create a GameObject for each tile.
        for (int x = 0; x < world.Width; x++) {
            for (int y = 0; y < world.Height; y++) {
                // Get tile data.
                Tile tile_data = world.GetTileAt(x, y);

                // Creates a new GameObject and adds it to our scene.
                GameObject tile_go = new GameObject();

                // Add Tile/GameObject pair to dictionary.
                tileGameObjectMap.Add(tile_data, tile_go);

                // Configure Tile_x_y and position it in the world
                tile_go.name = "Tile_" + x + "_" + y;
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y);
                tile_go.transform.SetParent(this.transform, true);

                // Add a Sprite Renderer and default sprite
                SpriteRenderer tile_sr = tile_go.AddComponent<SpriteRenderer>();
                tile_sr.sprite = emptySprite;
                tile_sr.sortingLayerName = "Tiles";
                //tile_sr.material = (Material)Resources.Load("Shader/DissolveMaterial");

                OnTileChanged(tile_data);
            }
        }

        // Register our Callbacks
        world.RegisterTileChanged(OnTileChanged);
    }

    //NOT IN USE - Destroys GameObjects but keeps Tile Data.
    void DestroyAllTileGameObjects() {
        while (tileGameObjectMap.Count > 0) {
            Tile tile_data = tileGameObjectMap.Keys.First();
            GameObject tile_go = tileGameObjectMap[tile_data];

            tileGameObjectMap.Remove(tile_data);

            tile_data.UnregisterTileTypeChangedCallback(OnTileChanged);

            Destroy(tile_go);
        }
    }


    // Function called automatically whenever a tile's data gets changed.
    void OnTileChanged(Tile tile_data) {

        if (tileGameObjectMap.ContainsKey(tile_data) == false) {
            Debug.LogError("tileGameObjectMap doesn't contain the tile_data -- did you forget to add the tile to the dictionary? Or maybe forget to unregister a callback?");
            return;
        }

        GameObject tile_go = tileGameObjectMap[tile_data];

        if (tile_go == null) {
            Debug.LogError("tileGameObjectMap's returned GameObject is null -- did you forget to add the GO to the dictionary? Or maybe forget to unregister a callback?");
            return;
        }

        if (tile_data.Type == TileType.Floor) {
            tile_go.GetComponent<SpriteRenderer>().sprite = floorSprite;
        } else if (tile_data.Type == TileType.Empty) {
            tile_go.GetComponent<SpriteRenderer>().sprite = emptySprite;
        } else {
            Debug.LogError("OnTileTypeChanged - Unrecognized tile type.");
        }
    }
}
