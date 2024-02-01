using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; protected set; }

    // The world and tile data.
    public World world { get; protected set; }

    static bool loadWorld = false;

    // Called before first frame.
    void OnEnable() {
        // Check if there's only one world
        if (Instance != null) {
            Debug.LogError("There should never be two world controllers.");
        }

        Instance = this;

        if (loadWorld) {
            loadWorld = false;
            CreateWorldFromSaveFile();
            Debug.Log("Create World from Saved File");
        } else {
            CreateEmptyWorld();
        }
    }

    private void Update() {
        // TODO: Add pause, unpause, speed controls, etc etc etc...
        world.Update(Time.deltaTime);
    }

    /// <summary>
    /// Gets the tile at the unity-space coordinates
    /// </summary>
    /// <returns>The tile at world coordinate.</returns>
    /// <param name="coord">Unity World-Space coordinates.</param>
    public Tile GetTileAtWorldCoord(Vector3 coord) {
        int x = Mathf.FloorToInt(coord.x + 0.5f) ;
        int y = Mathf.FloorToInt(coord.y + 0.5f);

        return world.GetTileAt(x, y);
    }

    public Vector3 GetWorldPositionOfTile(Tile tile)
    {
        return new Vector3(tile.X, tile.Y, 0);
    }

    public void NewWorld() {
        SceneManager.LoadScene( SceneManager.GetActiveScene().name );
        CreateEmptyWorld();
    }

    public void SaveWorld() {
        XmlSerializer serializer = new XmlSerializer( typeof(World) );
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, world);
        writer.Close();

        Debug.Log(writer.ToString());

        PlayerPrefs.SetString("SaveGame00", writer.ToString());
    }

    public void LoadWorld() {
        loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CreateEmptyWorld() {
        //Create a world with empty tiles.
        world = new World(100,100);

        // Center the Camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);

    }

    void CreateWorldFromSaveFile() {
        // Create a world from our saved file data.

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));
        world = (World)serializer.Deserialize(reader);
        reader.Close();

        // Center the Camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);

    }


}
