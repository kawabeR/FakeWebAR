using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FakeAR
{
    /// <summary>
    /// カメラの向きをジャイロ姿勢に反映する。
    /// WebGLビルドではレガシーInput.gyro（Unityエンジン組み込みのWebGL用実装）を使用する。
    /// 新Input SystemのAttitudeSensorはWebGLの姿勢センサーに未対応（Gamepad/Joystickのみ対応）
    /// のため使用しない。iOS Safariの許可リクエストのみ、タップ操作からjslib経由で明示的に呼ぶ。
    /// それ以外（エディタ等）ではAttitudeSensor、無ければWASDキーで代用する。
    /// </summary>
    public class GyroCameraController : MonoBehaviour
    {
        [SerializeField] private float editorRotateSpeed = 60f;

        private static readonly Quaternion BaseOrientation = Quaternion.Euler(90f, 0f, 0f);

        private AttitudeSensor attitudeSensor;
        private float editorYaw;
        private float editorPitch;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int DeviceOrientationBridge_NeedsPermission();
        [DllImport("__Internal")] private static extern void DeviceOrientationBridge_RequestPermission(string gameObjectName);
        [DllImport("__Internal")] private static extern void DeviceOrientationBridge_StartListening();
        [DllImport("__Internal")] private static extern float DeviceOrientationBridge_GetAlpha();
        [DllImport("__Internal")] private static extern float DeviceOrientationBridge_GetBeta();
        [DllImport("__Internal")] private static extern float DeviceOrientationBridge_GetGamma();

        private bool gyroStarted;
        private bool lastPermissionDenied;
#endif

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

        // タップ（GyroPermissionButton）から呼ばれる。WebGL以外では何もしない。
        public void RequestGyroPermission()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (DeviceOrientationBridge_NeedsPermission() == 1)
            {
                DeviceOrientationBridge_RequestPermission(gameObject.name);
            }
            else
            {
                OnOrientationPermissionResult("1");
            }
#endif
        }

        // ブラウザ側からSendMessageで呼ばれるコールバック（"1"=許可, "0"=拒否）。
        public void OnOrientationPermissionResult(string result)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (result == "1")
            {
                // 生のalpha/beta/gammaはデバッグ表示での突き合わせ用に取得しておく。
                // カメラ回転自体はUnity組み込みのInput.gyroで駆動する。
                DeviceOrientationBridge_StartListening();
                Input.gyro.enabled = true;
                gyroStarted = true;
            }
            else
            {
                lastPermissionDenied = true;
            }
#endif
        }

        // 動作確認用: 現在のジャイロ取得状態を文字列で返す（GyroDebugOverlayから参照）。
        public string GetDebugStatus()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (lastPermissionDenied)
            {
                return "Gyro: 許可が拒否されました";
            }
            if (!gyroStarted)
            {
                return "Gyro: 未開始（タップ待ち）";
            }
            float alpha = DeviceOrientationBridge_GetAlpha();
            float beta = DeviceOrientationBridge_GetBeta();
            float gamma = DeviceOrientationBridge_GetGamma();
            return string.Format(
                "Input.gyro.enabled={0}\nraw(a={1:F0} b={2:F0} g={3:F0})",
                Input.gyro.enabled, alpha, beta, gamma);
#else
            if (attitudeSensor != null && attitudeSensor.enabled)
            {
                return "AttitudeSensor使用中";
            }
            return "WASDフォールバック";
#endif
        }

        private void Update()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (gyroStarted && Input.gyro.enabled)
            {
                Quaternion q = Input.gyro.attitude;
                transform.rotation = new Quaternion(-q.x, -q.z, -q.y, q.w) * BaseOrientation;
                return;
            }
#endif
            if (attitudeSensor != null && attitudeSensor.enabled)
            {
                transform.rotation = BaseOrientation * ConvertGyroRotation(attitudeSensor.attitude.ReadValue());
            }
            else
            {
                UpdateEditorLook();
            }
        }

        // ジャイロの座標系（Z軸が奥・左手系）をUnityの座標系に変換する（新Input System版）
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
