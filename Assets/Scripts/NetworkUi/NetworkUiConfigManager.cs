using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUiConfigManager : NetworkBehaviour
{
    #region Fields
    private GameManager manager;

    [SerializeField] private TMP_Text usernameText;

    [SerializeField] private Image selectedBodyColorImage;
    [SerializeField] private Slider redBodyColorSlider;
    [SerializeField] private Slider greenBodyColorSlider;
    [SerializeField] private Slider blueBodyColorSlider;

    private string Username => usernameText.text;
    private Color BodyColor => new(redBodyColorSlider.value, greenBodyColorSlider.value, blueBodyColorSlider.value);
    #endregion Fields

    #region Unity Callbacks
    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);

        redBodyColorSlider.value = Random.value;
        greenBodyColorSlider.value = Random.value;
        blueBodyColorSlider.value = Random.value;

        selectedBodyColorImage.color = BodyColor;
    }
    private void Start()
    {
        StartCoroutine(PollUi());
    }
    private void Update()
    {
        selectedBodyColorImage.color = BodyColor;
    }
    #endregion Unity Callbacks

    private IEnumerator PollUi()
    {
        while (true)
        {
            if (!manager.IsLocalPlayerNull)
            {
                PlayerConfigurables configurables = manager.LocalPlayer.Configurables;

                string username = Username;
                if (configurables.username != username)
                {
                    configurables.CmdSetUsername(username);
                }

                Color bodyColor = BodyColor;
                if (configurables.bodyColor != bodyColor)
                {
                    configurables.CmdSetBodyColor(bodyColor);
                }
            }
            yield return new WaitForSeconds(0.25f);
        }
    }
}
