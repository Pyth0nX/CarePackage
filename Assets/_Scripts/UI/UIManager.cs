using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace CarePackage.UI
{
    public enum ToggleMode
    {
        Single,
        Multi,
        Unlimited
    }
    
    [Serializable]
    public struct PopupGroup 
    {
        [SerializeField] public GameObject[] popups;

        public PopupGroup(GameObject inPopup) : this(new []{inPopup}) {}
        
        public PopupGroup(GameObject[] inPopups) 
        {
            popups = inPopups;
        }
    }

    public class UIManager : MonoBehaviour
    {
        [SerializeField] private ToggleMode toggleMode = ToggleMode.Single;
        [SerializeField] private int maxToggles = 3;
        [SerializeField] private Transform hud;
        
        [SerializeField] private UnityEngine.InputSystem.PlayerInput playerInput;
        
        private List<HoverableElement> _toggledElements = new();
        
        private bool _popupsOpen = false;

        public int GetActivePopupCount() => _activePopups.Count;

        public GameObject GetActivePopup(int index) => _activePopups[index];
        public GameObject LastClicked { get; private set; }
        public GameObject HUD => hud.gameObject;

        private SettingsMenuController _settingsMenu;
        private List<GameObject> _activePopups = new();
        private List<GameObject> _elements = new();
#if UNITY_EDITOR
        [SerializeField] private List<PopupGroup> historyDebug = new();
#endif
        private Stack<PopupGroup> _popupHistory = new();
        private Stack<PopupGroup> _popupRedo = new();

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
            _settingsMenu = FindFirstObjectByType<SettingsMenuController>(FindObjectsInactive.Include);
        }

        public void SetPlayerInput(UnityEngine.InputSystem.PlayerInput inPlayerInput)
        {
            playerInput = inPlayerInput;
        }

        public void OpenPopupWindow(GameObject popupWindow)
        {
            var group = new PopupGroup(popupWindow);
            popupWindow.SetActive(true);
            if (_activePopups.Contains(popupWindow)) return;
            _activePopups.Add(popupWindow);
            OnInterfaceOpened?.Invoke();
            
            _popupHistory.Push(group);
#if UNITY_EDITOR
            historyDebug.Add(group);
#endif
            UpdateInputSchema();
        }

        public void OpenPopupWindows(GameObject[] popupWindows)
        {
            var group = new PopupGroup(popupWindows);
            foreach (var popup in popupWindows)
            {
                //OpenPopupWindow(popupWindow);
                popup.SetActive(true);
                if (!_activePopups.Contains(popup)) _activePopups.Add(popup);
            }
            _popupHistory.Push(group);
#if UNITY_EDITOR
            historyDebug.Add(group);
#endif
            _popupRedo.Clear();
            OnInterfaceOpened?.Invoke();
            UpdateInputSchema();
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
            
            _popupHistory = new Stack<PopupGroup>(_popupHistory.Where(g => !g.popups.Contains(popupWindow)));
#if UNITY_EDITOR
            historyDebug = _popupHistory.ToList();
#endif
            
            bool morePopups = _activePopups.Count > 0;
            OnInterfaceClosed?.Invoke(morePopups);
            UpdateInputSchema();
        }

        public void ClosePopupWindows(GameObject[] popupWindows)
        {
            foreach (GameObject popupWindow in popupWindows)
            {
                //ClosePopupWindow(popupWindow);
                popupWindow.SetActive(false);
                if (_activePopups.Contains(popupWindow)) _activePopups.Remove(popupWindow);
                
                _popupHistory = new Stack<PopupGroup>(_popupHistory.Where(g => !g.popups.Contains(popupWindow)));
#if UNITY_EDITOR
                historyDebug = _popupHistory.ToList();
#endif
            }
            bool morePopups = _activePopups.Count > 0;
            OnInterfaceClosed?.Invoke(morePopups);
            UpdateInputSchema();
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
        
        public void UndoLastPopupGroup()
        {
            if (_popupHistory.Count == 0) return;

            var group = _popupHistory.Pop();
#if UNITY_EDITOR
            historyDebug.RemoveAt(historyDebug.Count - 1);
#endif

            ClosePopupWindows(group.popups);
            _popupRedo.Push(group);
        }
        
        public void RedoLastPopupGroup() 
        {
            if (_popupRedo.Count == 0) return;

            var group = _popupRedo.Pop();
            OpenPopupWindows(group.popups);
            _popupHistory.Push(group);
#if UNITY_EDITOR
            historyDebug.Add(group);
#endif
        }

        public GameObject AddElement(GameObject elementToAdd)
        {
            if (elementToAdd == null) return null;
            var newElement = Instantiate(elementToAdd, hud);
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
        
        public void RegisterToggle(HoverableElement element)
        {
            switch (toggleMode)
            {
                case ToggleMode.Single:
                    foreach (var toggleable in _toggledElements)
                    {
                        toggleable.Toggle(false);
                    }
                    _toggledElements.Clear();
                    _toggledElements.Add(element);
                    _toggledElements[0].Toggle(false, true);
                    break;
                case ToggleMode.Multi:
                    if (!_toggledElements.Contains(element))
                    {
                        if (_toggledElements.Count >= maxToggles)
                        {
                            _toggledElements[0].Toggle(false);
                            UnregisterToggle(_toggledElements[0]);
                        }
                        _toggledElements.Add(element);
                        element.Toggle(false, true);
                    }
                    break;
                case ToggleMode.Unlimited:
                    if (!_toggledElements.Contains(element))
                    {
                        _toggledElements.Add(element);
                        element.Toggle(false, true);
                    }
                    break;
                default:
                    break;
            }
        }
        
        public void UnregisterToggle(HoverableElement element)
        {
            if (_toggledElements.Contains(element)) _toggledElements.Remove(element);
        }

        public void ToggleAllToggleables(bool toggle)
        {
            foreach (var toggleable in _toggledElements)
            {
                toggleable.Toggle(false, toggle);
            }
            _toggledElements.Clear();
        }
        
        public void ToggleHUD(bool toggle)
        {
            HUD.SetActive(toggle);
        }
        
        private void UpdateInputSchema() 
        {
            var schema = _activePopups.Count > 0 ? "UI" : "Player";
            SetInputSchema(schema);
        }
        
        public void SetInputSchema(string schema) 
        {
            playerInput.SwitchCurrentActionMap(schema);
        }
        
        public void OnUndo(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Debug.Log("Undo performed");
                UndoLastPopupGroup();
            }
        }
        
        public void OnEscape(UnityEngine.InputSystem.InputAction.CallbackContext context) 
        {
            if (!context.started) return;
            SetInputSchema("UI");

            if (_activePopups.Count > 0) 
            {
                UndoLastPopupGroup();
            } 
            else 
            {
                if (_settingsMenu != null) 
                {
                    if (!_settingsMenu.IsOpen) 
                    {
                        _settingsMenu.Open();
                        OpenPopupWindow(_settingsMenu.gameObject);
                    } 
                    else 
                    {
                        _settingsMenu.Close();
                        ClosePopupWindow(_settingsMenu.gameObject);
                    }
                }
            }
        }
    }
}