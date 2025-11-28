using CarePackage.UI;
using UnityEngine;

namespace CarePackage.Main
{
    public class MainMenuSceneController : MonoBehaviour
    {
        [SerializeField] private GameObject emptyUIPopup;
        [SerializeField] private bool allowLockMouse;

        private void Start()
        {
            PrimeTween.Tween.Delay(1f).OnComplete(() => UIManager.Instance.OpenPopupWindow(emptyUIPopup));
        }
    }
}