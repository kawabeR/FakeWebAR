using UnityEngine;

namespace FakeAR
{
    // マイク音量(RMS)に応じて波紋パーティクルの半径を変化させる。
    // volumeMin以下では最小半径、volumeMax以上では最大半径にクランプする。
    [RequireComponent(typeof(ParticleSystem))]
    public class RippleVolumeController : MonoBehaviour
    {
        [SerializeField] private MicrophoneVolumeController micVolumeController;
        [SerializeField] private float volumeMin = 0.01f;
        [SerializeField] private float volumeMax = 0.1f;
        [SerializeField] private float startSizeMin = 0.2f;
        [SerializeField] private float startSizeMax = 1.5f;

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
            float size = Mathf.Lerp(startSizeMin, startSizeMax, t);

            var main = ps.main;
            main.startSize = size;
        }
    }
}
