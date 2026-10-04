using UnityEngine;

namespace FakeAR
{
    // マイク音量(RMS)に応じて球体の半径を毎フレーム連続的に変化させる。
    // 波紋(RippleVolumeController)とは異なりインターバルで区切らず、常にリアルタイムに追従する。
    public class VolumeSphereController : MonoBehaviour
    {
        [SerializeField] private MicrophoneVolumeController micVolumeController;
        [SerializeField] private float volumeMin = 0.01f;
        [SerializeField] private float volumeMax = 0.1f;
        [SerializeField] private float radiusMin = 0.5f;
        [SerializeField] private float radiusMax = 2f;

        private void Update()
        {
            float volume = micVolumeController != null ? micVolumeController.GetVolume() : 0f;
            float t = Mathf.InverseLerp(volumeMin, volumeMax, volume);
            float radius = Mathf.Lerp(radiusMin, radiusMax, t);

            // 標準のSphereメッシュは半径0.5(直径1)なので、scale = radius * 2で実際の半径と一致する
            transform.localScale = Vector3.one * (radius * 2f);
        }
    }
}
