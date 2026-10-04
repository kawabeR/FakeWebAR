using UnityEngine;

namespace FakeAR
{
    // マイク音量(RMS)に応じて波紋パーティクルの全体サイズ(Size over Lifetimeの倍率)を変化させる。
    // 開始半径(Start Size)はスクリプトからは変更せず一定のまま。
    // volumeMin以下では最小倍率、volumeMax以上では最大倍率にクランプする。
    [RequireComponent(typeof(ParticleSystem))]
    public class RippleVolumeController : MonoBehaviour
    {
        [SerializeField] private MicrophoneVolumeController micVolumeController;
        [SerializeField] private float volumeMin = 0.01f;
        [SerializeField] private float volumeMax = 0.1f;
        [SerializeField] private float overallSizeMultiplierMin = 5f;
        [SerializeField] private float overallSizeMultiplierMax = 10f;

        private ParticleSystem ps;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (micVolumeController == null) return;

            float volume = micVolumeController.GetVolume();
            float t = Mathf.InverseLerp(volumeMin, volumeMax, volume);
            float multiplier = Mathf.Lerp(overallSizeMultiplierMin, overallSizeMultiplierMax, t);

            var sizeOverLifetime = ps.sizeOverLifetime;
            var sizeCurve = sizeOverLifetime.size;
            sizeCurve.curveMultiplier = multiplier;
            sizeOverLifetime.size = sizeCurve;
        }
    }
}
