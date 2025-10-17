using UnityEngine;

public class BeingStupidManager : MonoBehaviour
{
    private IActivatable[] _activatables;

    [SerializeField] private GameObject root;

    private void Start()
    {
        Activationables.Instance.SetRoot(root);
    }
    
/*
    private void Awake()
    {
        _activatables = Activationables.GetActivatables().ToArray();
    }

    private void OnEnable()
    {
        foreach (var activatable in _activatables)
        {
            activatable.OnEnable();
        }
    }

    private void OnDisable()
    {
        foreach (var activatable in _activatables)
        {
            activatable.OnDisable();
        }
    }*/
}