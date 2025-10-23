using System;
using System.Collections;
using System.Collections.Generic;
using CarePackage.Delivery;
using CarePackage.Interaction;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CarePackage.Main
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private SO_Mail mailBase;
        [SerializeField] private GameObject mailboxes;
        public PlayerState Player;

        public static GameManager Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            StartCoroutine(AssignRandomNumbers());
        }

        void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            } 
        }

        public IEnumerator AssignRandomNumbers()
        {
            yield return new WaitForSecondsRealtime(10f);
            int mailboxesCount = mailboxes.transform.childCount;
            List<Interactable> interactables = new List<Interactable>();
            List<int> storedMails = new();
            for (int i = 0; i < mailboxesCount; i++)
            {
                var mailbox = mailboxes.transform.GetChild(i).GetComponent<Interactable>();
                interactables.Add(mailbox);
            }

            List<int> uniqueNumbers = new List<int>();
            for (int i = 0; i < mailboxesCount; i++)
            {
                uniqueNumbers.Add(i);
            }

            Shuffle(uniqueNumbers);
            for (int i = 0; i < mailboxesCount; i++)
            {
                int assignedNumber = uniqueNumbers[i];
                Debug.Log($"Child {interactables[i].name} assigned number: {assignedNumber}");

                // Example: store the number in a custom component
                var action = interactables[i].InteractAction;
                if (action is DeliverMail mailAction)
                {
                    mailAction.WantedLetter = assignedNumber;
                }
            }
            Shuffle(uniqueNumbers);
            for (int i = 0; i < mailboxes.transform.childCount; i++)
            {
                var newMail = Instantiate(mailBase);
                newMail.id = uniqueNumbers[i]; // random number;
                Player.DeliveryManager.AddDelivery(newMail);
            }
            

            yield return null;
        }
    }

}
