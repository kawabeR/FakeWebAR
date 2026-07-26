using System.Runtime.InteropServices;
using UnityEngine;

namespace FakeAR
{
    // マイク入力の音量(RMS)を取得する。WebGLではWeb Audio APIブリッジ、
    // それ以外(エディタ等)ではUnityのMicrophoneクラスを使用する。
    public class MicrophoneVolumeController : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void MicrophoneVolumeBridge_RequestPermission(string gameObjectName);
        [DllImport("__Internal")] private static extern float MicrophoneVolumeBridge_GetVolume();
        [DllImport("__Internal")] private static extern int MicrophoneVolumeBridge_IsReady();

        private bool requested;
        private bool permissionDenied;
#else
        private const int SampleWindow = 512;
        private AudioClip micClip;
#endif

        public void RequestMicPermission()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (requested) return;
            requested = true;
            MicrophoneVolumeBridge_RequestPermission(gameObject.name);
#else
            if (micClip == null && Microphone.devices.Length > 0)
            {
                micClip = Microphone.Start(null, true, 10, 44100);
            }
#endif
        }

        public void OnMicPermissionResult(string result)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            permissionDenied = result != "1";
#endif
        }

        public float GetVolume()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return MicrophoneVolumeBridge_IsReady() == 1 ? MicrophoneVolumeBridge_GetVolume() : 0f;
#else
            if (micClip == null) return 0f;
            int micPosition = Microphone.GetPosition(null) - SampleWindow;
            if (micPosition < 0) return 0f;
            float[] samples = new float[SampleWindow];
            micClip.GetData(samples, micPosition);
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++) sum += samples[i] * samples[i];
            return Mathf.Sqrt(sum / SampleWindow);
#endif
        }

        public string GetDebugStatus()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (permissionDenied) return "Mic: 許可が拒否されました";
            if (!requested) return "Mic: 未開始（タップ待ち）";
            return string.Format("Mic volume={0:F3}", GetVolume());
#else
            return string.Format("Mic volume={0:F3}", GetVolume());
#endif
        }
    }
}
