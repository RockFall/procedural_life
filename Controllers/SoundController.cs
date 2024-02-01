using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    float soundCoolDown;

    // Start is called before the first frame update
    void Start()
    {
        WorldController.Instance.world.RegisterFurnitureCreated(OnFurnitureCreated);
        WorldController.Instance.world.RegisterTileChanged(OnTileChanged);
    }

    // Update is called once per frame
    void Update()
    {
        soundCoolDown -= Time.deltaTime;
    }

    void OnTileChanged(Tile tile_data) {
        if (soundCoolDown > 0)
            return;

        // FIXME:
        AudioClip ac = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);

        soundCoolDown = 0.1f;
    }

    public void OnFurnitureCreated(Furniture furn) {
        if (soundCoolDown > 0)
            return;

        // FIXME:
        AudioClip ac = Resources.Load<AudioClip>("Sounds/"+ furn.objectType +"_OnCreated");

        if (ac == null) {
            // Use default sound
            ac = Resources.Load<AudioClip>("Sounds/Wall_OnCreated");
        }

        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCoolDown = 0.1f;
    }
}
