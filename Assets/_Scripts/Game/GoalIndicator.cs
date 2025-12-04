using System.Collections.Generic;
using CarePackage.Delivery;
using UnityEngine;
using TMPro;

namespace CarePackage.Main
{
    public class GoalIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject obj;
        [SerializeField] private Camera cam;

        private List<Renderer> _objectRenderers = new();
        private TextMeshProUGUI _indicatorText;
        private float _elapsedTime;
        private float _upOffset;
        private bool _isVisible;
        private bool _canBeVisible = true;

        public GameObject GoalObject => obj;
        public Transform GoalTransform => obj != null ? obj.transform : null;

        public Camera Camera
        {
            get => cam;
            set => cam = value;
        }

        public static GoalIndicator Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _indicatorText = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            Invoke("Enable", 0.01f);
        }

        private void Enable()
        {
            GameManager.onDayEntered += OnDayEntered_Implementation;
            PlayerController.OnPlayerMoved += CheckGoalObjectInView;
            AAMAP.MapManager.OnMapEnabled += ToggleRendererCanBeVisible;
        }

        private void OnDisable()
        {
            GameManager.onDayEntered -= OnDayEntered_Implementation;
            PlayerController.OnPlayerMoved -= CheckGoalObjectInView;
            AAMAP.MapManager.OnMapEnabled -= ToggleRendererCanBeVisible;
        }

        private void LateUpdate()
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime < 0.01f) return;

            CheckGoalObjectInView();
            _elapsedTime = 0;
        }

        private void OnDayEntered_Implementation(int day)
        {
            cam = GameManager.Instance.Player.SwitchMode.CarCamera;
        }

        public void SetGoalObject(GameObject goalObj, bool mapIndicator = true, bool hidePreviousMarker = true, float upOffset = 1.33f)
        {/*
            // Disable marker on previous goal object
            if (obj != null && mapIndicator && hidePreviousMarker)
            {
                obj.transform.GetChild(obj.transform.childCount - 1).gameObject.SetActive(false);

                var oldDeliverableZone = obj.GetComponentInChildren<DeliverableZone>(true);
                if (oldDeliverableZone != null) oldDeliverableZone.transform.GetChild(0).gameObject.SetActive(false);
            }

            // clean up if no new goal
            if (goalObj == null)
            {
                obj = null;
                _indicatorText.text = "";
                return;
            }

            // Set new Goal
            _upOffset = upOffset;
            obj = goalObj;
            _indicatorText.text = "!";
            if (mapIndicator) obj.transform.GetChild(obj.transform.childCount - 1).gameObject.SetActive(true);
            
            // activate deliverableZone indicator
            var newDeliverableZone = obj.GetComponentInChildren<DeliverableZone>(true);
            if (newDeliverableZone != null) newDeliverableZone.transform.GetChild(0).gameObject.SetActive(true);

            // cache Renderers to cull the indicatortext
            _objectRenderers.Clear();
            _objectRenderers.AddRange(obj.GetComponentsInChildren<Renderer>(true));*/
            
            Debug.Log("SetGoalObject called with: " + (goalObj == null ? "NULL" : goalObj.name));
            var previousObj = obj;

            // Disable marker and deliverable zone on previous goal object
            if (previousObj != null)
            {
                if (mapIndicator && hidePreviousMarker)
                {
                    previousObj.transform.GetChild(previousObj.transform.childCount - 1).gameObject.SetActive(false);
                }

                Debug.Log("trying to find old DeliveryZone for " + previousObj.transform.root.name);
                var oldDeliverableZone = previousObj.GetComponentInChildren<DeliverableZone>(true);
                if (oldDeliverableZone != null)
                {
                    Debug.Log("Disabling old DeliveryZone for " + oldDeliverableZone.transform.root.name);
                    var indicatorChild = oldDeliverableZone.transform.GetChild(0);
                    if (indicatorChild != null) indicatorChild.gameObject.SetActive(false);
                }
            }

            // Clean up if no new goal
            if (goalObj == null)
            {
                Debug.Log("Clearing goal object");
                obj = null;
                _indicatorText.text = "";
                return;
            }

            // Set new Goal
            _upOffset = upOffset;
            obj = goalObj;
            _indicatorText.text = "!";

            if (mapIndicator)
                obj.transform.GetChild(obj.transform.childCount - 1).gameObject.SetActive(true);

            // Activate deliverableZone indicator on new goal
            var newDeliverableZone = obj.GetComponentInChildren<DeliverableZone>(true);
            if (newDeliverableZone != null)
            {
                var indicatorChild = newDeliverableZone.transform.GetChild(0);
                if (indicatorChild != null) indicatorChild.gameObject.SetActive(true);
            }

            // Cache renderers
            _objectRenderers.Clear();
            _objectRenderers.AddRange(obj.GetComponentsInChildren<Renderer>(true));
        }

        private void ToggleRendererCanBeVisible(bool toggle)
        {
            _canBeVisible = !toggle;
        }

        private void CheckGoalObjectInView()
        {
            if (obj == null || _indicatorText == null) return;
            if (_objectRenderers == null) return;

            _isVisible = false;
            if (!_canBeVisible) return;
            foreach (var render in _objectRenderers)
            {
                if (render.isVisible)
                {
                    _isVisible = true;
                    break;
                }
            }

            _indicatorText.enabled = _isVisible;
            if (!_indicatorText.enabled) return;

            var worldPoint = obj.transform.position + obj.transform.up * _upOffset;
            Vector3 screenPos = cam.WorldToScreenPoint(worldPoint);
            screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
            screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);
            transform.position = screenPos;
        }
    }
}