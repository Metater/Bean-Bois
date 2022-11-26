using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkConfigUiManager : NetworkBehaviour
{
    private GameManager manager;

    [SerializeField] private GameObject configPanelGameObject;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Image selectedColorImage;
    [SerializeField] private Slider redColorSlider;
    [SerializeField] private Slider greenColorSlider;
    [SerializeField] private Slider blueColorSlider;

    private string Username => usernameText.text;
    private Color SelectedColor => new(redColorSlider.value, greenColorSlider.value, blueColorSlider.value);

    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);

        redColorSlider.value = Random.value;
        greenColorSlider.value = Random.value;
        blueColorSlider.value = Random.value;

        selectedColorImage.color = SelectedColor;
    }

    private void Start()
    {
        StartCoroutine(PollState());
    }

    private void Update()
    {
        selectedColorImage.color = SelectedColor;
    }

    private IEnumerator PollState()
    {
        while (true)
        {
            if (manager.LocalPlayer is not null)
            {
                string username = Username;
                if (manager.LocalPlayer.username != username)
                {
                    manager.LocalPlayer.SetUsername(username);
                }

                Color selectedColor = SelectedColor;
                if (manager.LocalPlayer.bodyColor != selectedColor)
                {
                    manager.LocalPlayer.SetBodyColor(selectedColor);
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    public void ToggleConfigPanel()
    {
        configPanelGameObject.SetActive(!configPanelGameObject.activeSelf);
    }
}
