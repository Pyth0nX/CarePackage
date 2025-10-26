using CarePackage.Main;
using CarePackage.Persistance;
using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour, IDataPersistance
{
    [SerializeField] private float requiredMoney;
    [SerializeField] private TMP_Text requiredMoneyText;
    [SerializeField] private TMP_Text currentMoneyText;
    
    public int Bank => _money;
    public float GetRequiredMoney => _requiredMoney;
    
    private float _requiredMoney;
    private int _money;
    
    public static EconomyManager Instance;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        requiredMoneyText.text = "Required Money: " + requiredMoney + "$";
    }

    private void OnEnable()
    {
        Invoke("Enable", .1f);
    }

    private void Enable()
    {
        GameManager.Instance.OnStartGame += OnGameStarted_Implementation;
        GameManager.Instance.OnGameRestart += OnGameRestart_Implementation;
        GameManager.Instance.OnDayStarted += OnDayStarted_Implementation;
        GameManager.Instance.OnDayEnded += OnDayEnded_Implementation;
        UpdateMoneyText();
    }

    private void OnDisable()
    {
        GameManager.Instance.OnStartGame -= OnGameStarted_Implementation;
        GameManager.Instance.OnGameRestart -= OnGameRestart_Implementation;
        GameManager.Instance.OnDayStarted -= OnDayStarted_Implementation;
        GameManager.Instance.OnDayEnded -= OnDayEnded_Implementation;
    }

    private void OnGameStarted_Implementation()
    {
        _requiredMoney = requiredMoney;
    }

    private void OnGameRestart_Implementation()
    {
        _money = 0;
        _requiredMoney = requiredMoney;
        UpdateMoneyText();
    }
    
    private void OnDayStarted_Implementation()
    {
        _requiredMoney *= 1.2f;
        requiredMoneyText.text = "Required Money: " + _requiredMoney + "$";
        UpdateMoneyText();
    }

    private void OnDayEnded_Implementation()
    {
        if (_money >= GetRequiredMoney)
        {
            GameManager.Instance.Survive();
            return;
        }
        GameManager.Instance.LoseGame();
    }
    
    public void AddMoney(int amount)
    {
        _money += amount;
        UpdateMoneyText();
    }

    public void CalculateMoneyEarned(int originalPay, float timeTaken, float directDistance)
    {
        float normalizedTime = Mathf.Clamp01((360f - timeTaken) / 350f);
        float normalizedDistance = Mathf.Clamp01((directDistance - 10f) / 990f);
        
        float timeWeight = 0.7f;
        float distanceWeight = 0.3f;
        
        float performanceScore = (normalizedTime * timeWeight) + (normalizedDistance * distanceWeight);
        
        float scaledBase = Mathf.Lerp(180f, 360f, originalPay / 500f);
        float payout = scaledBase * performanceScore;
        
        float calculatedPay = Mathf.Round(payout);
        int finalPay = (int)calculatedPay;
        AddMoney(finalPay);
    }

    private void UpdateMoneyText()
    { 
        if (currentMoneyText == null) return;
        currentMoneyText.text = "Money: " + Bank + "$";
    }

    public void LoadData(GameData loadData)
    {
        _money = loadData.money;
        if (loadData.requiredMoney > _requiredMoney) _requiredMoney = loadData.requiredMoney;
    }

    public void SaveData(GameData saveData)
    {
        saveData.money = Bank;
        saveData.requiredMoney = GetRequiredMoney;
    }
}