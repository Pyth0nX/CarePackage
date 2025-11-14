using System.Collections.Generic;
using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    private bool _popupsOpen = false;
    
    public int GetActivePopupCount() => _activePopups.Count;
    
    public GameObject GetActivePopup(int index) => _activePopups[index];

    private List<GameObject> _activePopups = new();
    private List<GameObject> _elements = new();

    private Transform _activeOverlay;
    
    public static event Action OnInterfaceOpened;
    public static event Action<bool> OnInterfaceClosed;

    public static UIManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _activeOverlay = FindFirstObjectByType<Canvas>().transform;
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
    
    public void OpenPopupWindows(List<GameObject> popupWindows)
    {
        OpenPopupWindows(popupWindows.ToArray());
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

    public void ClosePopupWindows(List<GameObject> popupWindows)
    {
        ClosePopupWindows(popupWindows.ToArray());
    }

    public void CloseAllPopupWindows()
    {
        if (_activePopups.Count == 0) return;
        for (int i = _activePopups.Count - 1; i >= 0; i--)
        {
            ClosePopupWindow(_activePopups[i]);
        }
    }

    public void TogglePopupWindow(GameObject popupWindow)
    {
        if (_activePopups.Contains(popupWindow) && popupWindow.activeSelf)
        {
            ClosePopupWindow(popupWindow);
        }
        else if (!_activePopups.Contains(popupWindow) && !popupWindow.activeSelf)
        {
            OpenPopupWindow(popupWindow);
        }
    }

    public GameObject AddElement(GameObject elementToAdd)
    {
        if (elementToAdd == null) return null;
        var newElement = Instantiate(elementToAdd, _activeOverlay);
        _elements.Add(newElement);
        return newElement;
    }

    public void RemoveElement(GameObject elementToRemove)
    {
        if (!_elements.Contains(elementToRemove)) return;
        var elementObject = _elements[_elements.IndexOf(elementToRemove)];
        _elements.Remove(elementToRemove);
        Destroy(elementObject);
    }
}
