using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Interaction.Car
{
    public class EnterCarAction : InteractAction
    {
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var switchMode = interactingPlayer.SwitchMode;
            switchMode.EnterCarMode(interactingObject.transform);
        }
    }

    public class ExitCarAction : InteractAction
    {
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var switchMode = interactingPlayer.SwitchMode;
            switchMode.EnterFirstPersonMode(interactingObject.transform.root);
        }
    }
    
    public class OpenCarAction : InteractAction
    {
        [SerializeField] private GameObject carHoodClosed;
        [SerializeField] private GameObject carHoodOpen;

        private bool opened;
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            opened = !opened;
            carHoodClosed.SetActive(!opened);
            carHoodOpen.SetActive(opened);
        }
    }
}
