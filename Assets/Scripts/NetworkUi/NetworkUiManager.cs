using Mirror;
using TMPro;
using UnityEngine;

public class NetworkUiManager : NetworkBehaviour
{
    private GameManager manager;

    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);
    }

    #region Main Text
    [SerializeField] private TMP_Text globalTextText;
    [SyncVar(hook = nameof(OnGlobalTextChange))] public string globalText = "";
    private void OnGlobalTextChange(string _, string newGlobalText)
    {
        globalTextText.text = newGlobalText;
    }
    #endregion Main Text
}
