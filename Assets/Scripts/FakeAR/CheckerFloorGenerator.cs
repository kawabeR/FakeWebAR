using UnityEngine;

namespace FakeAR
{
    // 動作確認用: カメラ映像に対して3Dオブジェクトが正しく合成されているか、
    // ジャイロでの視点変化が反映されているかを目視確認するためのチェッカー柄床。
    [RequireComponent(typeof(MeshRenderer))]
    public class CheckerFloorGenerator : MonoBehaviour
    {
        [SerializeField] private int cellsPerSide = 8;
        [SerializeField] private int textureSize = 256;
        [SerializeField] private Color colorA = Color.white;
        [SerializeField] private Color colorB = Color.red;

        private void Awake()
        {
            Texture2D texture = new Texture2D(textureSize, textureSize);
            texture.filterMode = FilterMode.Point;
            int cellSize = textureSize / cellsPerSide;

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    bool isColorA = ((x / cellSize) + (y / cellSize)) % 2 == 0;
                    texture.SetPixel(x, y, isColorA ? colorA : colorB);
                }
            }
            texture.Apply();

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
            Material material = new Material(shader);
            material.mainTexture = texture;

            GetComponent<Renderer>().material = material;
        }
    }
}
