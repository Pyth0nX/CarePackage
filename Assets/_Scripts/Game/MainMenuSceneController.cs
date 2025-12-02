using CarePackage.UI;
using UnityEngine;

namespace CarePackage.Main
{
    public class MainMenuSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private GameObject emptyUIPopup;
        [SerializeField] private bool allowLockMouse;

        private void Start()
        {
            OnEnter();
        }

        public void OnEnter()
        {
            PrimeTween.Tween.Delay(1f).OnComplete(() => UIManager.Instance.OpenPopupWindow(emptyUIPopup));
        }

        public void OnExit() {}
    }
}