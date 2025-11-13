using CarePackage.Delivery;
using CarePackage.Main;
using UnityEngine;
using Yarn.Unity;
namespace CarePackage.Dialogue 
{
    public class DialogueCommands : MonoBehaviour
    {
        [YarnCommand("recievePackage")]
        public static void RecievePackage(bool recievePackage)
        {
            DialogueManager.RecievedPackage(recievePackage);
        }

        [YarnCommand("giveItem")]
        public static void GiveItem(string itemToGive)
        {
            var itemGUID = "Items/" + itemToGive;
            var item = InventoryUtilities.LoadItem(itemGUID);
            GameManager.Instance.Player.Inventory.AddItem(item);
        }

    }

}
