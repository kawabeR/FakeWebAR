using UnityEngine;
using UnityEngine.Serialization;

namespace FakeAR
{
    public class ContinuousRotator : MonoBehaviour
    {
        [FormerlySerializedAs("worldAxis")] [SerializeField] private Vector3 axis = Vector3.up;
        [SerializeField] private float degreesPerSecond = 30f;
        [SerializeField] private Space rotationSpace = Space.World;

        private void Update()
        {
            transform.Rotate(axis, degreesPerSecond * Time.deltaTime, rotationSpace);
        }
    }
}
