using UnityEngine;

namespace FakeAR
{
    // マイク音量(RMS)に応じて波紋パーティクルの「ゴール半径」(育ちきったときの半径)を変化させる。
    // スタート半径(生まれた瞬間の半径)は固定値のまま変えない。
    //
    // 音量のサンプリングはupdateInterval(秒)ごとに1回だけ行い、その値を次のサンプリングまで
    // 固定で使う。ParticleSystem側の発生間隔(Emission > Rate over Time / Bursts)と寿命も
    // 同じupdateIntervalに揃えることで、「1つの波紋が生まれてから育ちきるまでの間、
    // 常に同じ目標半径に向かって成長する」という単純な動きになる
    // (毎フレーム音量を反映すると、育っている途中のリングの見た目が音量の揺れでブレてしまうため)。
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
        [SerializeField] private float updateInterval = 2f;

        private ParticleSystem ps;
        private ParticleSystem.Particle[] particles;
        private float goalRadius;
        private float timer;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
            goalRadius = goalRadiusMin;
            timer = updateInterval; // 起動直後の最初のLateUpdateで即サンプリングする
        }

        private void LateUpdate()
        {
            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                timer = 0f;
                float volume = micVolumeController != null ? micVolumeController.GetVolume() : 0f;
                float t = Mathf.InverseLerp(volumeMin, volumeMax, volume);
                goalRadius = Mathf.Lerp(goalRadiusMin, goalRadiusMax, t);
            }

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
