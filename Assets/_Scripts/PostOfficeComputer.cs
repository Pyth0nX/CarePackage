using System.Collections.Generic;
using CarePackage.Main;
using UnityEngine;
using UnityEngine.UI;

namespace CarePackage.Delivery
{
    public class PostOfficeComputer : MonoBehaviour
    {
        [SerializeField] private Button[] buttons;
        [SerializeField] private GameObject checkInButton;
        [SerializeField] private GameObject desktop;

        [SerializeField] private GameObject mailPrefab;
        [SerializeField] private GameObject mailContainer;
        [SerializeField] private GameObject mailWindow;

        public void CheckInClicked()
        {
            UIManager.Instance.OpenPopupWindow(desktop);
//        GameManager.StartDay();
        }
    
        public void CheckOutClicked()
        {
            GameObject[] popups = new GameObject[2] { desktop, UIManager.Instance.GetActivePopup(1) };
            UIManager.Instance.ClosePopupWindows(popups);
            //    GameManager.SkipDay();
        }

        public void OpenMailClicked()
        {
            UIManager.Instance.TogglePopupWindow(mailWindow);
            InitializeAllMails();
        }

        private void InitializeAllMails()
        {
            List<GameObject> s = GameManager.Instance.Player.Inventory.GetUnacceptedItems();
            for (int i = 0; i < s.Count; i++)
            {
                Debug.Log(s[i]);
                var newMail = Instantiate(mailPrefab, mailContainer.transform);
                var mailBtn =  newMail.transform.GetChild(0).GetComponent<Button>();
                mailBtn.onClick.AddListener(() => OnMailClicked(i));
            }
        }

        private void OnMailClicked(int index)
        {
            GameManager.Instance.Player.Inventory.AcceptItem(GameManager.Instance.Player.Inventory.GetUnacceptedItems()[index]);
        }
    }
}