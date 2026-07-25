using UnityEngine;
using UnityEngine.InputSystem;

namespace FakeAR
{
    /// <summary>
    /// ジャイロセンサー（AttitudeSensor）の姿勢をメインカメラの向きに反映する。
    /// ジャイロが無い環境（エディタ等）ではWASDキーで代用する。
    /// このプロジェクトはActive Input Handlingが「Input System Package」のみのため、
    /// 新Input System（UnityEngine.InputSystem）のAPIを使用する。
    /// </summary>
    public class GyroCameraController : MonoBehaviour
    {
        [SerializeField] private float editorRotateSpeed = 60f;

        private static readonly Quaternion BaseOrientation = Quaternion.Euler(90f, 0f, 0f);

        private AttitudeSensor attitudeSensor;
        private float editorYaw;
        private float editorPitch;

        private void Start()
        {
            attitudeSensor = AttitudeSensor.current;
            if (attitudeSensor != null)
            {
                InputSystem.EnableDevice(attitudeSensor);
            }

            Vector3 currentEuler = transform.eulerAngles;
            editorYaw = currentEuler.y;
            editorPitch = currentEuler.x;
        }

        private void Update()
        {
            if (attitudeSensor != null && attitudeSensor.enabled)
            {
                transform.rotation = BaseOrientation * ConvertGyroRotation(attitudeSensor.attitude.ReadValue());
            }
            else
            {
                UpdateEditorLook();
            }
        }

        // ジャイロの座標系（Z軸が奥・左手系）をUnityの座標系に変換する
        private static Quaternion ConvertGyroRotation(Quaternion q)
        {
            return new Quaternion(q.x, q.y, -q.z, -q.w);
        }

        private void UpdateEditorLook()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            float yawInput = 0f;
            float pitchInput = 0f;

            if (keyboard.aKey.isPressed) yawInput -= 1f;
            if (keyboard.dKey.isPressed) yawInput += 1f;
            if (keyboard.wKey.isPressed) pitchInput -= 1f;
            if (keyboard.sKey.isPressed) pitchInput += 1f;

            editorYaw += yawInput * editorRotateSpeed * Time.deltaTime;
            editorPitch += pitchInput * editorRotateSpeed * Time.deltaTime;
            editorPitch = Mathf.Clamp(editorPitch, -89f, 89f);

            transform.rotation = Quaternion.Euler(editorPitch, editorYaw, 0f);
        }
    }
}
