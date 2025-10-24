using CarePackage.Main;
using CarePackage.Persistance;
using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour, IDataPersistance
{
    [SerializeField] private int money;
    [SerializeField] private float requiredMoney;
    [SerializeField] private TMP_Text moneyText;
    
    public int Bank => money;
    public float GetRequiredMoney => requiredMoney;
    
    public static EconomyManager Instance;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        Invoke("Enable", .1f);
    }

    private void Enable()
    {
        GameManager.Instance.OnDayStarted += OnDayStarted_Implementation;
        GameManager.Instance.OnDayEnded += OnDayEnded_Implementation;
        UpdateMoneyText();
    }

    private void OnDisable()
    {
        GameManager.Instance.OnDayStarted -= OnDayStarted_Implementation;
        GameManager.Instance.OnDayEnded -= OnDayEnded_Implementation;
    }
    
    public void OnDayStarted_Implementation()
    {
        requiredMoney *= 1.2f;
        UpdateMoneyText();
    }

    public void OnDayEnded_Implementation()
    {
        if (money >= requiredMoney) return;
        GameManager.Instance.LoseGame();
    }
    
    public void AddMoney(int amount)
    {
        money += amount;
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
        moneyText.text = "Money: " + Bank + "$";
    }

    public void LoadData(GameData loadData)
    {
        money = loadData.money;
        requiredMoney = loadData.requiredMoney;
    }

    public void SaveData(GameData saveData)
    {
        saveData.money = Bank;
        saveData.requiredMoney = GetRequiredMoney;
    }
}