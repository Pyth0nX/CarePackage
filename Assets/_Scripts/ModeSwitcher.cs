using CarePackage.Interaction;
using CarePackage.Main;
using UnityEngine;

public class ModeSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject carCamera;
    [SerializeField] private GameObject idleCar;
    
    public GameObject CarCamera => carCamera;
    public GameObject FirstPersonPlayer { get => _firstPersonPlayer; set => _firstPersonPlayer = value; }
    public GameObject Car { get => _car; set => _car = value; }
    public GameObject IdleCar { get => idleCar; set => idleCar = value; }
    
    private GameObject _firstPersonPlayer;
    private GameObject _car;
    private bool _idleCarInitialized;

    private void Awake()
    {
        FirstPersonPlayer = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include).gameObject;
        Car = FindFirstObjectByType<PrometeoCarController>(FindObjectsInactive.Include).gameObject;
    }

    public void EnterCarMode(Transform originalTransform)
    {
        FirstPersonPlayer.SetActive(false);
        
        CarCamera.SetActive(true);
        var carPosition = originalTransform.transform.position + originalTransform.transform.up * -1.33f;
        var carRotation = originalTransform.transform.rotation;
        IdleCar.SetActive(false);
        
        Car.transform.position = carPosition;
        Car.transform.rotation = carRotation;
        Car.SetActive(true);
    }
    
    public void EnterFirstPersonMode(Transform originalTransform)
    {
        var carPosition = originalTransform.transform.position;
        if (!_idleCarInitialized)
        {
            IdleCar = GameObject.Instantiate(IdleCar, carPosition, Quaternion.Euler(Vector3.zero));
            _idleCarInitialized = true;
        }
        IdleCar.transform.position = carPosition + originalTransform.up * 1.33f;
        IdleCar.SetActive(true);
        
        var playerStartPos = carPosition + new Vector3(3f, 0, 0);
        var playerRotation = originalTransform.transform.right;
        originalTransform.root.gameObject.SetActive(false);
        CarCamera.SetActive(false);
        
        FirstPersonPlayer.transform.position = playerStartPos;
        FirstPersonPlayer.transform.rotation = Quaternion.Euler(playerRotation);
        FirstPersonPlayer.SetActive(true);
    }
}

public class EnterCarAction : InteractAction
{
    public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
    {
        var switchMode = interactingPlayer.SwitchMode;
        switchMode.EnterCarMode(interactingObject.transform);
    }
}

public class ExitCarAction : InteractAction
{
    public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
    {
        var switchMode = interactingPlayer.SwitchMode;
        switchMode.EnterFirstPersonMode(interactingObject.transform);
    }
}