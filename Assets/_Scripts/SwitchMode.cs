using CarePackage.Interaction;
using CarePackage.Main;
using UnityEngine;

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
        GameManager.Instance.SetPlayer(switchMode.Car.GetComponent<PlayerState>());
    }
}

public class ExitCarAction : InteractAction
{
    [SerializeField] private GameObject idleCarPrefab;
    
    public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
    {
        Debug.Log("Player has exited the car!");
        var switchMode = SwitchMode.Instance;
        
        var carPosition = interactingObject.transform.position;
        if (!switchMode.IdleCarInitialized)
        {
            switchMode.IdleCar = GameObject.Instantiate(idleCarPrefab, carPosition, Quaternion.Euler(Vector3.zero));
            switchMode.IdleCarInitialized = true;
        }
        switchMode.IdleCar.transform.position = carPosition;
        switchMode.IdleCar.SetActive(true);
        
        var playerStartPos = carPosition + new Vector3(3f, 0, 0);
        if (!switchMode.FirstPersonInitialized)
        {
            switchMode.FirstPersonPlayer = GameObject.Instantiate(switchMode.FirstPersonPlayer, playerStartPos, Quaternion.identity);
            switchMode.FirstPersonInitialized = true;
        }
        interactingObject.transform.parent.gameObject.SetActive(false);
        switchMode.CarCamera.SetActive(false);
        switchMode.FirstPersonPlayer.SetActive(false);
        switchMode.FirstPersonPlayer.SetActive(true);
        GameManager.Instance.SetPlayer(switchMode.FirstPersonPlayer.GetComponent<PlayerState>());
    }
}