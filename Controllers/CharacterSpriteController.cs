using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Experimental.Rendering.LWRP;

public class CharacterSpriteController : MonoBehaviour
{
    // Temporary light effect
    //public GameObject characterLight;

    // Dictionarys storing the CHARACTER/GAMEOBJECT and the SPRITE_NAME/SPRITE
    Dictionary<Character, GameObject> characterGameObjectMap;
    Dictionary<string, Sprite> characterSprites;

    // The world and tile data.
    World world { get { return WorldController.Instance.world; } }


    // Start is called before the first frame update
    void Start()
    {
        // Load all sprites from "Resources/" and store them in the sprite dictionary
        LoadSprites();

        // Instantiate Dictionary that tracks which GameObject is rendering which Character.
        characterGameObjectMap = new Dictionary<Character, GameObject>();

        // Register our Callbacks
        world.RegisterCharacterCreated(OnCharacterCreated);

        // Check pre-existing characters, which won't do the callback.
        foreach (Character c in world.characters) {
            OnCharacterCreated(c);
        }
        
    }

    // Load All Sprites from Resources
    void LoadSprites() {
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Character/");

        foreach (Sprite s in sprites) {
            characterSprites[s.name] = s;
        }
    }

    public void OnCharacterCreated(Character ch) {
        // Creates a new GameObject and adds it to our scene.
        GameObject ch_go = new GameObject();

        // Add Character/GameObject pair to dictionary.
        characterGameObjectMap.Add(ch, ch_go);

        ch_go.name = "Character";
        ch_go.transform.position = new Vector3(ch.X, ch.Y);
        ch_go.transform.SetParent(this.transform, true);

        SpriteRenderer ch_sr = ch_go.AddComponent<SpriteRenderer>();
        ch_sr.sprite = characterSprites["Worker"];
        ch_sr.sortingLayerName = "Characters";
        //ch_sr.material = (Material)Resources.Load("Shader/DissolveMaterial");

        //GameObject ch_lightGO = Instantiate(characterLight, ch_go.transform);

        // Register our callback so that our GameObject gets updated whenever it's info changes
        ch.RegisterOnChangedCallback(OnCharacterChanged);
    }

    
    void OnCharacterChanged(Character c) {
        // Make sure the character's graphics are correct.

        if (characterGameObjectMap.ContainsKey(c) == false) {
            Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map");
            return;
        }

        GameObject char_go = characterGameObjectMap[c];
        //char_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

        char_go.transform.position = new Vector3(c.X, c.Y, char_go.transform.position.z);
    }
}
