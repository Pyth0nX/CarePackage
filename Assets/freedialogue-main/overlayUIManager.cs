using UnityEngine;

public class overlayUIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public Animator panelAnimator;
    // Update is called once per frame
    void Update()
    {
        
    }
    public void ClickInventory()
    {
        panelAnimator.SetInteger("state", 2);
    }
    public void ClickClose()
    {
        panelAnimator.SetInteger("state", 0);
    }

    public void ClickNotes()
    {
        panelAnimator.SetInteger("state", 1);
    }
}

