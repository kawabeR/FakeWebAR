using UnityEngine;
using UnityEngine.UI;

namespace FakeAR
{
    // 動作確認用: ジャイロの許可状態・生の姿勢データ・カメラ回転値を画面隅に常時表示する。
    [RequireComponent(typeof(Text))]
    public class GyroDebugOverlay : MonoBehaviour
    {
        [SerializeField] private GyroCameraController gyroCameraController;

        private Text label;

        private void Awake()
        {
            label = GetComponent<Text>();
        }

        private void Update()
        {
            if (gyroCameraController == null)
            {
                return;
            }

            Vector3 camEuler = gyroCameraController.transform.rotation.eulerAngles;
            label.text = gyroCameraController.GetDebugStatus() +
                         string.Format("\ncamRot=({0:F0}, {1:F0}, {2:F0})", camEuler.x, camEuler.y, camEuler.z);
        }
    }
}
