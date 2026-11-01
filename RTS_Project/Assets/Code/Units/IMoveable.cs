using UnityEngine;

namespace Code.Units
{
    public interface IMoveable
    {
        void MoveTo(Vector3 position);
        void Stop();
    }
}