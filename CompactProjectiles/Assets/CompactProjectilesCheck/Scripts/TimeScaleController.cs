using UnityEngine;

namespace CompactProjectilesCheck
{
    public class TimeScaleController : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float TimeScale = 1f;

        private void Update()
        {
            Time.timeScale = TimeScale;
        }
    }
}
