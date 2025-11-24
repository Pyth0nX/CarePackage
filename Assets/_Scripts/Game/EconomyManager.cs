using CarePackage.Persistance;
using UnityEngine;
using TMPro;

namespace CarePackage.Main
{

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
            if (requiredMoneyText != null) requiredMoneyText.text = "Required Money: " + _requiredMoney + "$";
        }

        private void OnEnable()
        {
            Invoke("Enable", .01f);
        }

        private void Enable()
        {
            GameManager.Instance.onGameRestart += OnGameRestart_Implementation;
            GameManager.Instance.onDayStarted += OnDayStarted_Implementation;
            GameManager.Instance.onDayEnded += OnDayEnded_Implementation;
            UpdateMoneyText();
        }

        private void OnDisable()
        {
            GameManager.Instance.onGameRestart -= OnGameRestart_Implementation;
            GameManager.Instance.onDayStarted -= OnDayStarted_Implementation;
            GameManager.Instance.onDayEnded -= OnDayEnded_Implementation;
        }

        private void OnGameRestart_Implementation()
        {
            _money = 0;
            UpdateMoneyText();
        }

        private void OnDayStarted_Implementation(int day)
        {
            _requiredMoney = requiredMoney * (day > 1 ? day * 1.2f : 1);
            if (requiredMoneyText != null) requiredMoneyText.text = "Required Money: " + _requiredMoney + "$";
            UpdateMoneyText();
        }

        private void OnDayEnded_Implementation(int day)
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

        public void CalculateMoneyEarned(Delivery.FPackageData packageInfo, float timeTaken, float directDistance)
        {
            float normalizedTime = Mathf.Clamp01((120f - timeTaken) / 110f);
            float normalizedDistance = Mathf.Clamp01((directDistance - 10f) / 590f);
            
            float timeWeight = 0.7f;
            float distanceWeight = 0.3f;

            float performanceScore = (normalizedTime * timeWeight) + (normalizedDistance * distanceWeight);
            performanceScore = Mathf.Clamp01(performanceScore);
            
            float scaledBase = Mathf.Lerp(packageInfo.MinPay, packageInfo.MaxPay, performanceScore);
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
        }

        public void SaveData(GameData saveData)
        {
            saveData.money = Bank;
        }
    }
}