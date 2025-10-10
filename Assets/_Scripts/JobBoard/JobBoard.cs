using UnityEngine;
using UnityEngine.UI;

namespace CarePackage.Job
{
    public class JobBoard : MonoBehaviour
    {
        [SerializeField] private Button[] jobButtons;

        private void OnEnable()
        {
            foreach (var button in jobButtons)
            {
                button.onClick.AddListener((() => OnJobClicked(button.gameObject)));

            }
        }

        private void OnDisable()
        {
            foreach (var button in jobButtons)
            {
                button.onClick.RemoveAllListeners();
                Debug.Log("You have unclicked " + button.gameObject.name);
            }
        }

        private void OnJobClicked(GameObject button)
        {
            Debug.Log("You have clicked " + button);
        }
    }
}