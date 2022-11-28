using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUiConfigManager : NetworkBehaviour
{
    private GameManager manager;

    [SerializeField] private float uiPollPeriod;

    [SerializeField] private TMP_Text usernameText;

    [SerializeField] private Image selectedBodyColorImage;
    [SerializeField] private Slider redBodyColorSlider;
    [SerializeField] private Slider greenBodyColorSlider;
    [SerializeField] private Slider blueBodyColorSlider;

    private string Username => usernameText.text;
    private Color BodyColor => new(redBodyColorSlider.value, greenBodyColorSlider.value, blueBodyColorSlider.value);

    #region Unity Callbacks
    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);

        redBodyColorSlider.value = Random.value;
        greenBodyColorSlider.value = Random.value;
        blueBodyColorSlider.value = Random.value;
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
            yield return new WaitForSeconds(uiPollPeriod);
        }
    }
}
