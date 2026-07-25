using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace FakeAR
{
    [RequireComponent(typeof(RawImage))]
    public class WebCamBackground : MonoBehaviour
    {
        [SerializeField] private int requestedWidth = 1920;
        [SerializeField] private int requestedHeight = 1080;
        [SerializeField] private int requestedFPS = 30;

        private RawImage rawImage;
        private AspectRatioFitter aspectFitter;
        private WebCamTexture webCamTexture;

        private void Awake()
        {
            rawImage = GetComponent<RawImage>();
            aspectFitter = GetComponent<AspectRatioFitter>();
        }

        private IEnumerator Start()
        {
            yield return RequestCameraPermission();

            WebCamDevice? backCamera = FindBackCamera();
            if (backCamera == null)
            {
                Debug.LogWarning("[WebCamBackground] 使用可能なカメラが見つかりませんでした。");
                yield break;
            }

            webCamTexture = new WebCamTexture(backCamera.Value.name, requestedWidth, requestedHeight, requestedFPS);
            rawImage.texture = webCamTexture;
            webCamTexture.Play();
        }

        private IEnumerator RequestCameraPermission()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.Camera));
            }
#elif UNITY_WEBGL && !UNITY_EDITOR
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
#endif
            yield return null;
        }

        private static WebCamDevice? FindBackCamera()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                return null;
            }

            foreach (WebCamDevice device in devices)
            {
                if (!device.isFrontFacing)
                {
                    return device;
                }
            }

            // 背面カメラの判定が取れない環境向けのフォールバック
            return devices[0];
        }

        private void Update()
        {
            if (webCamTexture == null || !webCamTexture.didUpdateThisFrame)
            {
                return;
            }

            if (aspectFitter != null && webCamTexture.height > 0)
            {
                aspectFitter.aspectRatio = (float)webCamTexture.width / webCamTexture.height;
            }

            // 端末の向き（縦横）に合わせて映像の回転・反転を補正
            float rotationAngle = -webCamTexture.videoRotationAngle;
            rawImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, rotationAngle);

            float scaleY = webCamTexture.videoVerticallyMirrored ? -1f : 1f;
            rawImage.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
        }

        private void OnDestroy()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
            }
        }
    }
}
