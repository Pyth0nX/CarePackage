using CarePackage.Main;
using UnityEngine;
using System;

namespace CarePackage.Interaction.Miscellaneous
{
    [Serializable]
    public class PickupAction : Pickup, IInteractAction
    {
        [SerializeField] private bool hideInstedOfDestroy;
        [SerializeField] private bool removeAfterUse;

        public PickupAction(bool inHideInstedOfDestroy = false, bool inDisapearAfterUse = false) : base()
        {
            hideInstedOfDestroy = inHideInstedOfDestroy;
            removeAfterUse = inDisapearAfterUse;
        }

        public PickupAction(bool inHideInstedOfDestroy, bool inDisapearAfterUse, Vector3 inOffset) : this(
            inHideInstedOfDestroy, inDisapearAfterUse)
        {
            Offset = inOffset;
        }

        public PickupAction(bool inHideInstedOfDestroy, bool inDisapearAfterUse, Vector3 inOffset,
            IPickupExtension inPickupExtension) : this(inHideInstedOfDestroy, inDisapearAfterUse, inOffset)
        {
            ExtendedLogic = new[] { inPickupExtension };
        }

        public PickupAction(bool inHideInstedOfDestroy, bool inDisapearAfterUse, Vector3 inOffset,
            IPickupExtension[] inPickupExtensions) : this(inHideInstedOfDestroy, inDisapearAfterUse, inOffset)
        {
            ExtendedLogic = inPickupExtensions;
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            interactingPlayer.Pickup(this, interactingObject);
        }

        public void OnPickedUp(PlayerState interactingPlayer)
        {
            if (!removeAfterUse) return;
            if (hideInstedOfDestroy) OwningObject.SetActive(false);
            else GameObject.Destroy(OwningObject);
        }

        public void OnDropped(PlayerState interactingPlayer)
        {
            interactingPlayer.SetPickup(null, null);
        }
    }

    [Serializable]
    public class ToggleObjectAction : IInteractAction
    {
        [SerializeField] private bool enable = true;
        [SerializeField] private bool toggleItself;
        [SerializeField] private bool targetSelf = true;
        [SerializeField] private GameObject objectToToggle;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (targetSelf)
            {
                if (!toggleItself) interactingObject.SetActive(enable);
                else interactingObject.SetActive(!interactingObject.activeInHierarchy);
                return;
            }

            if (!toggleItself) objectToToggle.SetActive(enable);
            else objectToToggle.SetActive(!objectToToggle.activeInHierarchy);
        }
    }

    [Serializable]
    public class SwitchSceneAction : IInteractAction
    {
        [SerializeField] private string sceneName;

        public SwitchSceneAction(string inSceneName)
        {
            sceneName = inSceneName;
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            SceneController.Instance.LoadScene(sceneName);
        }
    }

    [Serializable]
    public class LaunchPickupAction : IInteractAction
    {
        private GameObject _owningObject;
        private float _minDuration = 0.6f;
        private float _maxDuration = 5f;
        private float _maxForce = 60f;
        private float _minForce = 10f;
        private float _heldDuration;

        public LaunchPickupAction() : this(null, 0f) {}

        public LaunchPickupAction(GameObject inOwningObject, float inHeldDuration)
        {
            _owningObject = inOwningObject;
            _heldDuration = inHeldDuration;
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            Debug.Log("Held Laucnh key for: " + _heldDuration);
            if (_heldDuration < _minDuration) return;
            LaunchPickupInternal(interactingPlayer, GetForceByDuration(_heldDuration));
        }

        void LaunchPickupInternal(PlayerState interactingPlayer, float launchForce)
        {
            Rigidbody rb;
            var addedRigidBody = false;
            if (!_owningObject.TryGetComponent<Rigidbody>(out rb))
            {
                rb = _owningObject.AddComponent<Rigidbody>();
                addedRigidBody = true;
            }

            interactingPlayer.DropPickup();
            rb.AddForce(GetViewAngleOfPlayer(interactingPlayer.ActivePlayer) * launchForce, ForceMode.Impulse);
            if (addedRigidBody) GameObject.Destroy(rb);
        }

        private Vector3 GetViewAngleOfPlayer(GameObject player)
        {
            if (player == null) return Vector3.zero;
            var view = player.transform.GetChild(2);
            return view.TransformDirection(Vector3.forward);
        }
        
        private float GetForceByDuration(float heldDuration)
        {
            float clampedDuration = Mathf.Clamp(heldDuration, _minDuration, _maxDuration);
            float t = (clampedDuration - _minDuration) / (_maxDuration - _minDuration);
            return Mathf.Lerp(_minForce, _maxForce, t);
        }
    }

    [Serializable]
    public class TweenSliderAction
    {
        private GameObject _sliderPrefab;
        private UnityEngine.UI.Slider _slider;
        private float _minDuration = 0.6f;
        private float _maxDuration = 5f;
        private PrimeTween.Tween _tween;
        
        public TweenSliderAction(GameObject inSliderPrefab)
        {
            _sliderPrefab = inSliderPrefab;
            var sliderObject = UIManager.Instance.AddElement(_sliderPrefab);
            _slider = sliderObject.GetComponent<UnityEngine.UI.Slider>();
        }

        public void StartTweening()
        {
            float elapsed = _minDuration;
            float totalDuration = _maxDuration - _minDuration;
            float startValue = Mathf.Clamp01(elapsed / totalDuration);
            
            _slider.value = startValue;
            _tween = PrimeTween.Tween.UISliderValue(_slider, 1f, totalDuration, PrimeTween.Ease.Linear);
        }

        public void StopTweening()
        {
            _tween.Stop();
            PrimeTween.Tween.Delay(.5f).OnComplete(() =>
            {
                UIManager.Instance.RemoveElement(_slider.gameObject);
            });
        }
    }
}