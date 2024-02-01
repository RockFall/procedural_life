using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public Button godLightButtom;
    public GameObject godLight;

    public Button buildTabButtom;
    public Button gameSettingTabButtom;

    public GameObject buildTab;
    public GameObject gameSettingsTab;

    // Start is called before the first frame update
    void Start()
    {
        buildTabButtom.onClick.AddListener(delegate { OpenTab(buildTab); });
        gameSettingTabButtom.onClick.AddListener(delegate { OpenTab(gameSettingsTab); });
        godLightButtom.onClick.AddListener(delegate { ChangeGodLight(godLight); });
    }

    // Update is called once per frame
    void Update() {
    }

    void OpenTab(GameObject btn) {
        btn.SetActive(!btn.activeSelf);
    }

    void ChangeGodLight(GameObject godLight) {
        godLight.SetActive(!godLight.activeSelf);
    }
}
