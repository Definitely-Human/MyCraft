using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    public GameObject mainMenuObject;
    public GameObject setttingsObject;

    [Header("Main Menu UI Elements")]
    public TextMeshProUGUI seedField;


    [Header("Settings Menu UI Elements")]
    public Slider viewDistSlider;
    public TextMeshProUGUI viewDistSliderText;
    public Slider mouseSensSlider;
    public TextMeshProUGUI mouseSensSliderText;
    public Toggle threadingToggle;
    public Toggle chunkAnimToggle;

    Settings settings;

    private string settingsPath = "/Settings/settings.cfg";

    private void Awake()
    {
        if (!File.Exists(Application.dataPath + settingsPath))
        {
            Debug.Log("No settings file found, creating new one.");

            settings = new Settings();
            string jsonExport = JsonUtility.ToJson(settings);
            File.WriteAllText(Application.dataPath + settingsPath, jsonExport);
        }
        else
        {
            Debug.Log("Settings file found. Loading settings.");
            string jsonImport = File.ReadAllText(Application.dataPath + settingsPath);
            settings = JsonUtility.FromJson<Settings>(jsonImport);
        }
    }

    public void StartGame()
    {
        VoxelData.seed = Mathf.Abs(seedField.text.GetHashCode()) / VoxelData.WorldWidthInChunks / 1000;
        VoxelData.settings = settings;
        SceneManager.LoadScene("WorldScene", LoadSceneMode.Single);
    }

    public void EnterSettings()
    {
        viewDistSlider.value = settings.ViewDistanceInChunks;
        UpdateViewDistSlider();

        mouseSensSlider.value = settings.mouseSensitivity;
        UpdateMouseSensSlider();

        threadingToggle.isOn = settings.enableThreading;
        chunkAnimToggle.isOn = settings.enableAnimatedChunks;

        mainMenuObject.SetActive(false);
        setttingsObject.SetActive(true);
    }

    public void LeaveSettings()
    {
        mainMenuObject.SetActive(true);
        setttingsObject.SetActive(false);

        settings.ViewDistanceInChunks = (int)viewDistSlider.value;
        settings.mouseSensitivity = mouseSensSlider.value;
        settings.enableThreading = threadingToggle.isOn;
        settings.enableAnimatedChunks = chunkAnimToggle.isOn;

        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + settingsPath, jsonExport);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void UpdateViewDistSlider()
    {
        viewDistSliderText.text = "View Distance: " + viewDistSlider.value;
    }

    public void UpdateMouseSensSlider()
    {
        mouseSensSliderText.text = "Mouse Sensitivity: " + mouseSensSlider.value.ToString("F1");

    }
}
