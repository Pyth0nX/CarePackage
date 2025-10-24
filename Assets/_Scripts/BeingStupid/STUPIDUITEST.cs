using UnityEngine;
using TMPro;

public class STUPIDUITEST : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    
    public void SetObject(GameObject obj)
    {
        this.obj = obj;
    }

    private TextMeshProUGUI indicatorText;
    private bool IndicatorSet => indicatorText.text == "!";
    
    public static STUPIDUITEST Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        indicatorText = GetComponent<TextMeshProUGUI>();
    }

    private void FixedUpdate()
    {
        if (obj != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(obj.transform.position + obj.transform.up * 1.33f);
            if (!IndicatorSet) indicatorText.text = "!";
        }
        else if (obj == null)
        {
            indicatorText.text = "";
        }
    }
}
