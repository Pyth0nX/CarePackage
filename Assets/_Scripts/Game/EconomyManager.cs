using System;
using CarePackage.Main;
using TMPro;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private int money;
    [SerializeField] private float requiredMoney;
    [SerializeField] private TMP_Text moneyText;
    
    public static EconomyManager Instance;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        GameManager.Instance.OnDayStarted += OnDayStarted_Implementation;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnDayStarted -= OnDayStarted_Implementation;
    }

    public int Bank => money;
    public float GetRequiredMoney => requiredMoney;
    
    public void AddMoney(int amount)
    {
        money += amount;
    }

    public void OnDayStarted_Implementation()
    {
        requiredMoney *= 1.2f;
        moneyText.text = money.ToString();
    }
}