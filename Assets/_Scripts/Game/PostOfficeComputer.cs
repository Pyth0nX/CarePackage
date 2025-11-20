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
            UI.UIManager.Instance.ClosePopupWindow(checkInButton);
            UI.UIManager.Instance.OpenPopupWindow(desktop);
            if (!GameManager.Instance.tutorialDone)
                Task.TaskManager.PushTaskUpdate(new Task.Task("Select Jobs to deliver"));
//        GameManager.StartDay();
        }
    
        public void CheckOutClicked()
        {
            int popupCount = UI.UIManager.Instance.GetActivePopupCount();
            GameObject[] popups = new GameObject[popupCount];
            for (int i = 0; i < popupCount; i++)
            {
                popups[i] = UI.UIManager.Instance.GetActivePopup(i);
            }
            UI.UIManager.Instance.ClosePopupWindows(popups);
            //    GameManager.SkipDay();
        }

        public void OpenMailClicked()
        {
            UI.UIManager.Instance.TogglePopupWindow(mailWindow);
            InitializeAllMails();
        }

        private void InitializeAllMails()
        {
            foreach (Transform child in mailContainer.transform)
            {
                Destroy(child.gameObject);
            }
            
            List<SO_Item> unreadMails = GameManager.Instance.Player.Inventory.GetUnacceptedItems();
            foreach (var item in unreadMails)
            {
                var newMail = Instantiate(mailPrefab, mailContainer.transform);
                Button mailBtn =  newMail.transform.GetChild(0).GetComponent<Button>();
                Image mailImage = newMail.transform.GetChild(0).GetComponent<Image>();
                
                SO_Item index = item;
                
                mailImage.sprite = index.ItemData.icon;
                mailBtn.onClick.AddListener(() => OnMailClicked(index, mailBtn));
            }
            /*
            for (int i = 0; i < s.Count; i++)
            {
                Debug.Log($"Added mail with {i}");
                var newMail = Instantiate(mailPrefab, mailContainer.transform);
                Button mailBtn =  newMail.transform.GetChild(0).GetComponent<Button>();
                int index = i;
                mailBtn.onClick.AddListener(() => OnMailClicked(index, mailBtn));
            }*/
        }

        private void OnMailClicked(SO_Item index, Button parent)
        {
            if (index == null) return;
            GameManager.Instance.Player.Inventory.AcceptItem(index);
            parent.onClick.RemoveAllListeners();
            Destroy(parent.transform.parent.gameObject);
        }
    }
}