using System.Collections.Generic;
using CarePackage.Main;
using UnityEngine;
using TMPro;

public class GoalIndicator : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    [SerializeField] private Camera cam;
    
    private List<Renderer> _objectRenderers = new();
    private TextMeshProUGUI _indicatorText;
    private float _elapsedTime;
    private float _upOffset;
    private bool _isVisible;
    private bool _canBeVisible = true;

    public GameObject GoalObject => obj;
    public Transform GoalTransform => obj.transform;
    public Camera Camera { get => cam; set => cam = value; }
    
    public static GoalIndicator Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        _indicatorText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        Invoke("Enable", 0.01f);
    }
    
    private void Enable()
    {
        GameManager.onDayEntered += OnDayEntered_Implementation;
        PlayerController.OnPlayerMoved += CheckGoalObjectInView;
        AAMAP.MapManager.OnMapEnabled += ToggleRendererCanBeVisible;
    }

    private void OnDisable()
    {
        GameManager.onDayEntered -= OnDayEntered_Implementation;
        PlayerController.OnPlayerMoved -= CheckGoalObjectInView;
        AAMAP.MapManager.OnMapEnabled -= ToggleRendererCanBeVisible;
    }

    private void LateUpdate()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime < 0.01f) return;

        CheckGoalObjectInView();
        _elapsedTime = 0;
    }

    private void OnDayEntered_Implementation(int day)
    {
        cam = GameManager.Instance.Player.SwitchMode.CarCamera;
    }
    
    public void SetGoalObject(GameObject goalObj, bool mapIndicator = true, bool hidePreviousMarker = true, float upOffset = 1.33f)
    {
        Debug.Log("Setting GoalObject to " + goalObj);
        if (obj != null && mapIndicator && hidePreviousMarker) obj.transform.GetChild(obj.transform.childCount -1).gameObject.SetActive(false);
        if (goalObj == null)
        {
            obj = null;
            _indicatorText.text = "";
            return;
        }
        
        _upOffset = upOffset;
        obj = goalObj;
        _indicatorText.text = "!";
        if (mapIndicator) obj.transform.GetChild(obj.transform.childCount - 1).gameObject.SetActive(true);
        
        _objectRenderers.Clear();
        _objectRenderers.AddRange(obj.GetComponentsInChildren<Renderer>(true));
    }

    private void ToggleRendererCanBeVisible(bool toggle)
    {
        _canBeVisible = !toggle;
    }

    private void CheckGoalObjectInView()
    {
        if (obj == null || _indicatorText == null) return;
        if (_objectRenderers == null) return;
        
        _isVisible = false;
        if (!_canBeVisible) return;
        foreach (var render in _objectRenderers)
        {
            if (render.isVisible)
            {
                _isVisible = true;
                break;
            }
        }
        _indicatorText.enabled = _isVisible;
        if (!_indicatorText.enabled) return;

        var worldPoint = obj.transform.position + obj.transform.up * _upOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPoint);
        screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
        screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);
        transform.position = screenPos;
    }
}