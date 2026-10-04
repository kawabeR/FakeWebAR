using UnityEngine;

namespace FakeAR
{
    // マイク音量(RMS)に応じて、透明な円板の「見える半径」を毎フレーム連続的に変化させる。
    //
    // 円板の模様(リング)は、頂点のUVを中心からの実距離(半径)に比例させて割り当てることで、
    // 半径が伸縮しても模様1本あたりの間隔(ringsPerUnit)は変わらない。
    // 模様を流れさせる処理は、頂点UVのV値に毎フレーム一定速度(scrollSpeed)のオフセットを
    // 直接加算することで実現している(Sprites/DefaultシェーダーはmainTextureOffsetを
    // 参照しないため、material側のオフセットではなくUV自体を動かす必要がある)。
    // これも円板の半径とは独立した一定周期の動きになる。
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RippleDiscController : MonoBehaviour
    {
        [SerializeField] private MicrophoneVolumeController micVolumeController;
        [SerializeField] private float volumeMin = 0.01f;
        [SerializeField] private float volumeMax = 0.1f;
        [SerializeField] private float radiusMin = 0.5f;
        [SerializeField] private float radiusMax = 1f;
        [SerializeField] private int segments = 64;
        [SerializeField] private float ringsPerUnit = 2f;
        [SerializeField] private float scrollSpeed = 0.3f;
        [SerializeField] private Color lineColor = new Color(0.4f, 0.85f, 1f);

        private Mesh mesh;
        private Vector3[] vertices;
        private Vector2[] uvs;
        private Material material;
        private float scrollOffset;

        private void Awake()
        {
            BuildMesh();
            BuildMaterial();
        }

        private void BuildMesh()
        {
            mesh = new Mesh { name = "RippleDisc" };

            // 0番は中心頂点。1〜segments+1番は外周(両面描画のため三角形は表裏2枚ずつ)
            vertices = new Vector3[segments + 2];
            uvs = new Vector2[segments + 2];
            int[] triangles = new int[segments * 6];

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                int vi = i + 1;
                vertices[vi] = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                uvs[vi] = new Vector2((float)i / segments, 1f);
            }

            for (int i = 0; i < segments; i++)
            {
                int t = i * 6;
                triangles[t] = 0;
                triangles[t + 1] = i + 1;
                triangles[t + 2] = i + 2;
                triangles[t + 3] = 0;
                triangles[t + 4] = i + 2;
                triangles[t + 5] = i + 1;
            }

            Color[] colors = new Color[vertices.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private void BuildMaterial()
        {
            const int texHeight = 64;
            Texture2D texture = new Texture2D(1, texHeight, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            for (int y = 0; y < texHeight; y++)
            {
                float v = (float)y / texHeight;
                // sin波ちょうど1周期でシームレスにループする、細い輪1本分の模様
                float wave = Mathf.Sin(v * Mathf.PI * 2f) * 0.5f + 0.5f;
                float alpha = Mathf.Pow(wave, 6f);
                texture.SetPixel(0, y, new Color(1f, 1f, 1f, alpha));
            }
            texture.Apply();

            // Sprites/Defaultはアルファブレンド・両面描画に対応済みで、
            // レンダーパイプラインを問わず追加設定なしで透過表示できる
            material = new Material(Shader.Find("Sprites/Default"))
            {
                mainTexture = texture,
                color = lineColor
            };

            GetComponent<Renderer>().material = material;
        }

        private void Update()
        {
            float volume = micVolumeController != null ? micVolumeController.GetVolume() : 0f;
            float t = Mathf.InverseLerp(volumeMin, volumeMax, volume);
            float radius = Mathf.Lerp(radiusMin, radiusMax, t);

            scrollOffset += scrollSpeed * Time.deltaTime;

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                int vi = i + 1;
                vertices[vi] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                uvs[vi] = new Vector2((float)i / segments, radius * ringsPerUnit + scrollOffset);
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.RecalculateBounds();
        }
    }
}
