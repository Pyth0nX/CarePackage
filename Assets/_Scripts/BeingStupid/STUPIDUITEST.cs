using System.Collections.Generic;
using CarePackage.Main;
using UnityEngine;
using TMPro;

public class STUPIDUITEST : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    [SerializeField] private Camera cam;
    [SerializeField] private bool isVisible;
    
    private List<Renderer> _objectRenderers = new();
    private TextMeshProUGUI _indicatorText;
    private float _elapsedTime;
    
    public void SetObject(GameObject obj)
    {
        if (this.obj != null) this.obj.transform.GetChild(this.obj.transform.childCount -1).gameObject.SetActive(false);
        if (obj == null)
        {
            this.obj = null;
            _indicatorText.text = "";
            return;
        }
        
        this.obj = obj;
        _indicatorText.text = "!";
        this.obj.transform.GetChild(this.obj.transform.childCount - 1).gameObject.SetActive(true);
        var objRenderer = this.obj.transform.GetChild(0);
        _objectRenderers.Clear();
        for (int i = 0; i < objRenderer.childCount; i++)
        {
            _objectRenderers.Add(objRenderer.transform.GetChild(i).GetComponent<Renderer>());
        }
    }
    
    public static STUPIDUITEST Instance;

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
        PlayerController.OnPlayerMoved += CheckObjectInView;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerMoved -= CheckObjectInView;
    }

    private void LateUpdate()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime < 0.01f) return;

        CheckObjectInView();
        _elapsedTime = 0;
    }

    private void CheckObjectInView()
    {
        if (obj == null || _indicatorText == null) return;
        if (_objectRenderers == null) return;
        
        isVisible = false;
        foreach (var render in _objectRenderers)
        {
            if (render.isVisible)
            {
                isVisible = true;
                break;
            }
        }
        _indicatorText.enabled = isVisible;
        if (!_indicatorText.enabled) return;
        
        Vector3 screenPos = cam.WorldToScreenPoint(obj.transform.position + obj.transform.up * 1.33f);
        screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
        screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);
        transform.position = screenPos;
    }
}