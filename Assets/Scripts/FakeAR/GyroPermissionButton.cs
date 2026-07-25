using UnityEngine;
using UnityEngine.EventSystems;

namespace FakeAR
{
    // 画面タップでジャイロ（DeviceOrientation）の使用許可をリクエストし、自身を非表示にする。
    [RequireComponent(typeof(RectTransform))]
    public class GyroPermissionButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GyroCameraController gyroCameraController;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (gyroCameraController != null)
            {
                gyroCameraController.RequestGyroPermission();
            }

            gameObject.SetActive(false);
        }
    }
}
