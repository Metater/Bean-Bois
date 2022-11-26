using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [SerializeField] private GameObject alwaysVisibleUi;
    [SerializeField] private GameObject cursorVisibleUi;
    [SerializeField] private GameObject cursorInvisibleUi;

    public bool IsUiDisabled { get; private set; } = true;

    public void SetUiDisabled(bool isUiDisabled)
    {
        IsUiDisabled = isUiDisabled;
        SetAllUi(!IsUiDisabled);
    }

    public void ShowUi(bool isCursorVisible)
    {
        if (IsUiDisabled)
        {
            SetAllUi(false);
        }
        else
        {
            cursorVisibleUi.SetActive(isCursorVisible);
            cursorInvisibleUi.SetActive(!isCursorVisible);
        }
    }

    private void SetAllUi(bool value)
    {
        alwaysVisibleUi.SetActive(value);
        cursorVisibleUi.SetActive(value);
        cursorInvisibleUi.SetActive(value);
    }
}
