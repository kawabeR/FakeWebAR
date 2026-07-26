using UnityEngine;

namespace FakeAR
{
    public class OrbitAroundTransform : MonoBehaviour
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private Vector3 worldAxis = Vector3.up;
        [SerializeField] private float degreesPerSecond = 30f;

        private void Update()
        {
            if (pivot == null) return;
            transform.RotateAround(pivot.position, worldAxis, degreesPerSecond * Time.deltaTime);
        }
    }
}
