using UnityEngine;
using System;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public Action OnInterfaceOpened;
    public Action<bool> OnInterfaceClosed;
    private bool _popupsOpen = false;

    private List<GameObject> _activePopups = new();

    public static UIManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void OpenPopupWindow(GameObject popupWindow)
    {
        popupWindow.SetActive(true);
        if (_activePopups.Contains(popupWindow)) return;
        _activePopups.Add(popupWindow);
        OnInterfaceOpened?.Invoke();
    }

    public void OpenPopupWindows(GameObject[] popupWindows)
    {
        foreach (var popupWindow in popupWindows)
        {
            OpenPopupWindow(popupWindow);
        }
    }

    public void ClosePopupWindow(GameObject popupWindow)
    {
        popupWindow.SetActive(false);
        if (!_activePopups.Contains(popupWindow)) return;
        _activePopups.Remove(popupWindow);
        bool morePopups = _activePopups.Count > 0;
        OnInterfaceClosed?.Invoke(morePopups);
    }

    public void ClosePopupWindows(GameObject[] popupWindows)
    {
        foreach (GameObject popupWindow in popupWindows)
        {
            ClosePopupWindow(popupWindow);
        }
    }
}
