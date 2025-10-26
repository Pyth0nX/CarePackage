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
        if (requiredMoneyText != null) requiredMoneyText.text = "Required Money: " + requiredMoney + "$";
    }

    private void OnEnable()
    {
        Invoke("Enable", .01f);
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
        UpdateMoneyText();    }

    private void OnGameRestart_Implementation()
    {
        _money = 0;
        _requiredMoney = requiredMoney;
        UpdateMoneyText();
    }
    
    private void OnDayStarted_Implementation()
    {
        _requiredMoney *= 1.2f;
        if (requiredMoneyText != null) requiredMoneyText.text = "Required Money: " + _requiredMoney + "$";
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
        float normalizedTime = Mathf.Clamp01((120f - timeTaken) / 350f);
        float normalizedDistance = Mathf.Clamp01((directDistance - 10f) / 590f);
        
        float timeWeight = 0.7f;
        float distanceWeight = 0.3f;
        
        float performanceScore = (normalizedTime * timeWeight) + (normalizedDistance * distanceWeight);
        performanceScore = Mathf.Clamp01(performanceScore);
        
        float scaledBase = Mathf.Lerp(20f, 150f, performanceScore);
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
        if (loadData.requiredMoney >= requiredMoney) _requiredMoney = loadData.requiredMoney;
        else _requiredMoney = requiredMoney;
    }

    public void SaveData(GameData saveData)
    {
        saveData.money = Bank;
        saveData.requiredMoney = GetRequiredMoney;
    }
}