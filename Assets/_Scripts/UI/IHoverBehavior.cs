using UnityEngine.EventSystems;

namespace CarePackage.UI
{
    public interface IHoverBehavior
    {
        public void OnHovered(PointerEventData eventData);
        public void OnClicked(PointerEventData eventData);
        public void OnUnhovered(PointerEventData eventData);
        public void Reset();
    }

    public enum ScaleAxis { XY, X, Y }
}