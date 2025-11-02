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
    private bool _isVisible;
    
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
        PlayerController.OnPlayerMoved += CheckGoalObjectInView;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerMoved -= CheckGoalObjectInView;
    }

    private void LateUpdate()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime < 0.01f) return;

        CheckGoalObjectInView();
        _elapsedTime = 0;
    }
    
    public void SetGoalObject(GameObject goalObj)
    {
        if (obj != null) obj.transform.GetChild(obj.transform.childCount -1).gameObject.SetActive(false);
        if (goalObj == null)
        {
            obj = null;
            _indicatorText.text = "";
            return;
        }
        
        obj = goalObj;
        _indicatorText.text = "!";
        obj.transform.GetChild(obj.transform.childCount - 1).gameObject.SetActive(true);
        var objRenderer = obj.transform.GetChild(0);
        _objectRenderers.Clear();
        for (int i = 0; i < objRenderer.childCount; i++)
        {
            _objectRenderers.Add(objRenderer.transform.GetChild(i).GetComponent<Renderer>());
        }
    }

    private void CheckGoalObjectInView()
    {
        if (obj == null || _indicatorText == null) return;
        if (_objectRenderers == null) return;
        
        _isVisible = false;
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
        
        Vector3 screenPos = cam.WorldToScreenPoint(obj.transform.position + obj.transform.up * 1.33f);
        screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
        screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);
        transform.position = screenPos;
    }
}