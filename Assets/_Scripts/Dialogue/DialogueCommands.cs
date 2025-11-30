using CarePackage.Delivery;
using CarePackage.Main;
using UnityEngine;
using Yarn.Unity;
namespace CarePackage.Dialogue 
{
    public class DialogueCommands : MonoBehaviour
    {
        [YarnCommand("receivePackage")]
        public static void ReceivePackage(bool receivePackage)
        {
            DialogueManager.ReceivedPackage(receivePackage);
        }

        [YarnCommand("giveItem")]
        public static void GiveItem(string itemToGive)
        {
            var itemGUID = "Items/" + itemToGive;
            var item = InventoryUtilities.LoadItem(itemGUID);
            GameManager.Instance.Player.Inventory.AddUnacceptedItem(item);
        }

    }

}
