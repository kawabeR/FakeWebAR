using UnityEngine;
using UnityEngine.UI;

namespace FakeAR
{
    [RequireComponent(typeof(Text))]
    public class MicVolumeDebugOverlay : MonoBehaviour
    {
        [SerializeField] private MicrophoneVolumeController micVolumeController;
        private Text label;

        private void Awake()
        {
            label = GetComponent<Text>();
        }

        private void Update()
        {
            if (micVolumeController == null) return;
            label.text = micVolumeController.GetDebugStatus();
        }
    }
}
