using UnityEngine;

namespace FakeAR
{
    // マイク音量(RMS)に応じて波紋パーティクルの「ゴール半径」(育ちきったときの半径)を変化させる。
    // スタート半径(生まれた瞬間の半径)は固定値のまま変えない。
    // それ以外(発生間隔・寿命・形状・色など)はすべて静的(ParticleSystem側の設定のまま)。
    //
    // Size over Lifetimeモジュールは使わず、パーティクルごとに「経過時間の割合」から
    // 直接サイズを計算して設定する。モジュールのカーブは全パーティクルで共有されるため
    // 毎フレーム書き換えると途中経過の個体まで巻き込んで変化してしまう問題があった。
    // 個別のParticleインスタンスを直接操作することでこれを避けている。
    [RequireComponent(typeof(ParticleSystem))]
    public class RippleVolumeController : MonoBehaviour
    {
        [SerializeField] private MicrophoneVolumeController micVolumeController;
        [SerializeField] private float volumeMin = 0.01f;
        [SerializeField] private float volumeMax = 0.1f;
        [SerializeField] private float startRadius = 0.5f;
        [SerializeField] private float goalRadiusMin = 5f;
        [SerializeField] private float goalRadiusMax = 10f;

        private ParticleSystem ps;
        private ParticleSystem.Particle[] particles;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
        }

        private void LateUpdate()
        {
            float volume = micVolumeController != null ? micVolumeController.GetVolume() : 0f;
            float t = Mathf.InverseLerp(volumeMin, volumeMax, volume);
            float goalRadius = Mathf.Lerp(goalRadiusMin, goalRadiusMax, t);

            int count = ps.GetParticles(particles);
            for (int i = 0; i < count; i++)
            {
                float ageRatio = 1f - (particles[i].remainingLifetime / particles[i].startLifetime);
                float radius = Mathf.Lerp(startRadius, goalRadius, ageRatio);
                particles[i].startSize = radius * 2f;
            }
            ps.SetParticles(particles, count);
        }
    }
}
