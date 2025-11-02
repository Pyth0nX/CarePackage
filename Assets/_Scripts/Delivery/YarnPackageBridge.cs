using UnityEngine;
using Yarn.Unity;

namespace CarePackage.Delivery
{
    public class YarnPackageBridge : MonoBehaviour
    {
        [Header("References")]
        public DialogueRunner dialogueRunner;
        public PackageObject packageObject;

        private void Start()
        {
            if (dialogueRunner == null)
            {
                dialogueRunner = Object.FindFirstObjectByType<DialogueRunner>();
            }

            if (packageObject == null)
            {
                packageObject = Object.FindFirstObjectByType<PackageObject>();
            }

            UpdateYarnPackageState();
        }

        public void UpdateYarnPackageState()
        {
            if (dialogueRunner == null || packageObject == null) return;

            //Converts from enum to string 
            string stateName = packageObject.GetPackageStateName();

            // Send value to yarn
            dialogueRunner.VariableStorage.SetValue("$packageState", stateName);

            // Number version
            dialogueRunner.VariableStorage.SetValue("$damage", (int)packageObject.GetPackageState());
        }
    }
}