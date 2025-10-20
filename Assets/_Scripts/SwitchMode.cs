using CarePackage.Interaction;
using CarePackage.Main;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class SwitchMode : MonoBehaviour
{
    [SerializeField] private GameObject firstPersonPlayer;
    [SerializeField] private GameObject carCamera;
    
    public GameObject CarCamera => carCamera;
    public GameObject FirstPersonPlayer { get => firstPersonPlayer; set => firstPersonPlayer = value; }

    public static SwitchMode Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
}

public class EnterCarAction : InteractAction
{
    [SerializeField] private bool playerActive;
    
    public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
    {
        var switchMode = interactingObject.GetComponent<SwitchMode>();
        if (playerActive)
        {
            switchMode.CarCamera.SetActive(true);
            switchMode.FirstPersonPlayer.SetActive(false);
            return;
        }
        switchMode.CarCamera.SetActive(false);
        switchMode.FirstPersonPlayer.SetActive(true);
    }
}

public class ExitCarAction : InteractAction
{
    [SerializeField] private GameObject idleCarPrefab;
    
    public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
    {
        var carPosition = interactingObject.transform.position;
        var car = GameObject.Instantiate(idleCarPrefab, carPosition, Quaternion.Euler(Vector3.zero));
        
        var playerStartPos = carPosition + new Vector3(3f, 0, 0);
        SwitchMode.Instance.FirstPersonPlayer = GameObject.Instantiate(SwitchMode.Instance.FirstPersonPlayer, playerStartPos, Quaternion.identity);
    }
}