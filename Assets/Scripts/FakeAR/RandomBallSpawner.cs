using UnityEngine;

namespace FakeAR
{
    /// <summary>
    /// カメラ映像の上に3Dオブジェクト（赤い球）をオーバーレイ表示するデモ用スポナー。
    /// </summary>
    public class RandomBallSpawner : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float spawnDistance = 8f;
        [SerializeField] private float spawnSpread = 3f;
        [SerializeField] private float ballLifetime = 6f;
        [SerializeField] private float ballScale = 0.5f;
        [SerializeField] private Color ballColor = Color.red;

        private float timer;
        private Material ballMaterial;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            ballMaterial = new Material(shader) { color = ballColor };
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < spawnInterval)
            {
                return;
            }

            timer = 0f;
            SpawnBall();
        }

        private void SpawnBall()
        {
            if (targetCamera == null)
            {
                return;
            }

            Vector2 randomOffset = Random.insideUnitCircle * spawnSpread;
            Vector3 spawnPosition = targetCamera.transform.position
                + targetCamera.transform.forward * spawnDistance
                + targetCamera.transform.right * randomOffset.x
                + targetCamera.transform.up * randomOffset.y;

            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "FakeAR_Ball";
            ball.transform.position = spawnPosition;
            ball.transform.localScale = Vector3.one * ballScale;
            ball.GetComponent<Renderer>().sharedMaterial = ballMaterial;

            Collider ballCollider = ball.GetComponent<Collider>();
            if (ballCollider != null)
            {
                Destroy(ballCollider);
            }

            Destroy(ball, ballLifetime);
        }
    }
}
