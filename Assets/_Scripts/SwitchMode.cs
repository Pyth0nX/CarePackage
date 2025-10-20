using CarePackage.Interaction;
using CarePackage.Main;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class SwitchMode : MonoBehaviour
{
    [SerializeField] private GameObject firstPersonPlayer;
    [SerializeField] private GameObject carCamera;
    [SerializeField] private GameObject car;
    [SerializeField] private GameObject idleCar;
    
    public GameObject CarCamera => carCamera;
    public GameObject FirstPersonPlayer { get => firstPersonPlayer; set => firstPersonPlayer = value; }
    public GameObject Car { get => car; set => car = value; }
    public GameObject IdleCar { get => idleCar; set => idleCar = value; }
    

    public bool FirstPersonInitialized;

    public bool IdleCarInitialized;
        
    public static SwitchMode Instance;
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
}

public class EnterCarAction : InteractAction
{
    public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
    {
        var switchMode = SwitchMode.Instance;
        switchMode.FirstPersonPlayer.SetActive(false);
        switchMode.CarCamera.SetActive(true);
        var carPosition = interactingObject.transform.position;
        interactingObject.SetActive(false);
        switchMode.Car.transform.position = carPosition;
        switchMode.Car.SetActive(true);
    }
}

public class ExitCarAction : InteractAction
{
    [SerializeField] private GameObject idleCarPrefab;
    
    public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
    {
        Debug.Log("Player has exited the car!");
        var carPosition = interactingObject.transform.position;
        if (!SwitchMode.Instance.IdleCarInitialized)
        {
            SwitchMode.Instance.IdleCar = GameObject.Instantiate(idleCarPrefab, carPosition, Quaternion.Euler(Vector3.zero));
            SwitchMode.Instance.IdleCarInitialized = true;
        }
        SwitchMode.Instance.IdleCar.transform.position = carPosition;
        SwitchMode.Instance.IdleCar.SetActive(true);
        
        var playerStartPos = carPosition + new Vector3(3f, 0, 0);
        if (!SwitchMode.Instance.FirstPersonInitialized)
        {
            SwitchMode.Instance.FirstPersonPlayer = GameObject.Instantiate(SwitchMode.Instance.FirstPersonPlayer, playerStartPos, Quaternion.identity);
            SwitchMode.Instance.FirstPersonInitialized = true;
        }
        interactingObject.transform.parent.gameObject.SetActive(false);
        SwitchMode.Instance.CarCamera.SetActive(false);
        SwitchMode.Instance.FirstPersonPlayer.SetActive(false);
        SwitchMode.Instance.FirstPersonPlayer.SetActive(true);
    }
}