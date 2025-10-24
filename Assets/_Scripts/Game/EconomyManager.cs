using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private int money;
    [SerializeField] private int requiredMoney;
    
    public EconomyManager Instance;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    
    public int Bank => money;
    public int GetRequiredMoney => requiredMoney;
    
    public void AddMoney(int amount)
    {
        money += amount;
    }
}