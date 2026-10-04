using UnityEngine;

namespace FakeAR
{
    // マイク音量(RMS)に応じて波紋パーティクルの「育ちきったサイズ(全体サイズ)」を変化させる。
    // Size over Lifetimeのカーブの始点(age=0, value=1)は常に固定し、
    // 終点(age=1, value=全体サイズ)だけを音量で変えることで、
    // 生まれた瞬間のスタート半径が音量の影響を受けないようにしている。
    // (multiplierでカーブ全体を拡縮すると始点も一緒に拡縮されてしまうため採用しない)
    // volumeMin以下では最小サイズ、volumeMax以上では最大サイズにクランプする。
    [RequireComponent(typeof(ParticleSystem))]
    public class RippleVolumeController : MonoBehaviour
    {
        [SerializeField] private MicrophoneVolumeController micVolumeController;
        [SerializeField] private float volumeMin = 0.01f;
        [SerializeField] private float volumeMax = 0.1f;
        [SerializeField] private float overallSizeEndMin = 10f;
        [SerializeField] private float overallSizeEndMax = 20f;

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
            float endValue = Mathf.Lerp(overallSizeEndMin, overallSizeEndMax, t);

            var sizeOverLifetime = ps.sizeOverLifetime;
            var sizeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, endValue));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        }
    }
}
