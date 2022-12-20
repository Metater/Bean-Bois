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

    [Range(0, 1f)] [SerializeField] private float uiPollPeriod;

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
        InvokeRepeating(nameof(PollUi), 0, uiPollPeriod);
    }
    private void Update()
    {
        selectedBodyColorImage.color = BodyColor;
    }
    #endregion Unity Callbacks

    // Not a coroutine, because those are cancelled on gameobject disable
    // And it is disabled by the network identity while disconnected
    private void PollUi()
    {
        if (!manager.IsLocalPlayerNull)
        {
            PlayerConfigurables configurables = manager.LocalPlayer.Configurables;

            PollUsernameUi(configurables);
            PollBodyColorUi(configurables);
        }
    }

    #region Username
    [SerializeField] private TMP_Text usernameText;
    private string Username => usernameText.text;
    private string setUsername = null;
    private void PollUsernameUi(PlayerConfigurables configurables)
    {
        string username = Username;
        if (setUsername == null || setUsername != username)
        {
            configurables.CmdSetUsername(username);
            setUsername = username;
        }
    }
    #endregion Username

    #region Body Color
    [SerializeField] private Image selectedBodyColorImage;
    [SerializeField] private Slider redBodyColorSlider;
    [SerializeField] private Slider greenBodyColorSlider;
    [SerializeField] private Slider blueBodyColorSlider;
    private Color BodyColor => new(redBodyColorSlider.value, greenBodyColorSlider.value, blueBodyColorSlider.value);
    private Color? setBodyColor = null;
    private void PollBodyColorUi(PlayerConfigurables configurables)
    {
        Color bodyColor = BodyColor;
        if (setBodyColor == null || setBodyColor.Value != bodyColor)
        {
            configurables.CmdSetBodyColor(bodyColor);
            setBodyColor = bodyColor;
        }
    }
    #endregion Body Color
}
