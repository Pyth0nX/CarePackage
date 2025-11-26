using UnityEngine;

public class IndicatorBehavior : MonoBehaviour
{
    [SerializeField] private HUDIndicator.IndicatorOnScreen onScreenIndicator;
    [SerializeField] private HUDIndicator.IndicatorOffScreen offScreenIndicator;
    [SerializeField] private bool startActive;

    private void Start()
    {
        ToggleIndicator(startActive);
    }

    public void ToggleIndicator(bool activate)
    {
        if (onScreenIndicator != null) 
            onScreenIndicator.enabled = activate;
        if (offScreenIndicator != null) 
            offScreenIndicator.enabled = activate;
    }
}