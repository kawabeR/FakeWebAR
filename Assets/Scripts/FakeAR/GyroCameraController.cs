using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FakeAR
{
    /// <summary>
    /// カメラの向きをジャイロ姿勢に反映する。
    /// Input SystemはWebGLの姿勢センサーに未対応（Gamepad/Joystickのみ対応）のため、
    /// WebGLビルドではブラウザのDeviceOrientationEventを直接参照するjslibブリッジを使う。
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
        [DllImport("__Internal")] private static extern float DeviceOrientationBridge_GetScreenOrientationAngle();

        private bool webglListening;
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
                DeviceOrientationBridge_StartListening();
                webglListening = true;
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
            if (!webglListening)
            {
                return "Gyro: 未開始（タップ待ち）";
            }
            float alpha = DeviceOrientationBridge_GetAlpha();
            float beta = DeviceOrientationBridge_GetBeta();
            float gamma = DeviceOrientationBridge_GetGamma();
            return string.Format("Gyro有効 a={0:F0} b={1:F0} g={2:F0}", alpha, beta, gamma);
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
            if (webglListening)
            {
                float alpha = DeviceOrientationBridge_GetAlpha();
                float beta = DeviceOrientationBridge_GetBeta();
                float gamma = DeviceOrientationBridge_GetGamma();
                float screenAngle = DeviceOrientationBridge_GetScreenOrientationAngle();
                transform.rotation = DeviceOrientationToUnityRotation(alpha, beta, gamma, screenAngle);
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

#if UNITY_WEBGL && !UNITY_EDITOR
        // W3C DeviceOrientationEvent(alpha/beta/gamma)からUnityの回転への変換。
        // three.jsのDeviceOrientationControlsと同一のアルゴリズムを移植し、
        // 右手系→左手系の変換（z, w反転）をAttitudeSensor版と同様に適用している。
        private static Quaternion DeviceOrientationToUnityRotation(float alphaDeg, float betaDeg, float gammaDeg, float screenAngleDeg)
        {
            float x = betaDeg * Mathf.Deg2Rad;
            float y = alphaDeg * Mathf.Deg2Rad;
            float z = -gammaDeg * Mathf.Deg2Rad;
            float orient = screenAngleDeg * Mathf.Deg2Rad;

            float c1 = Mathf.Cos(x * 0.5f), s1 = Mathf.Sin(x * 0.5f);
            float c2 = Mathf.Cos(y * 0.5f), s2 = Mathf.Sin(y * 0.5f);
            float c3 = Mathf.Cos(z * 0.5f), s3 = Mathf.Sin(z * 0.5f);

            Quaternion qEuler = new Quaternion(
                s1 * c2 * c3 + c1 * s2 * s3,
                c1 * s2 * c3 - s1 * c2 * s3,
                c1 * c2 * s3 - s1 * s2 * c3,
                c1 * c2 * c3 + s1 * s2 * s3);

            Quaternion qBack = new Quaternion(-0.70710678f, 0f, 0f, 0.70710678f);
            Quaternion qScreen = new Quaternion(0f, 0f, Mathf.Sin(-orient * 0.5f), Mathf.Cos(-orient * 0.5f));

            Quaternion qRaw = qEuler * qBack * qScreen;

            return new Quaternion(qRaw.x, qRaw.y, -qRaw.z, -qRaw.w);
        }
#endif
    }
}
