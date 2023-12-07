using UnityEngine;

namespace MyDrone
{
    [CreateAssetMenu(fileName = "Preset", menuName = "Drone/Rate preset", order = 0)]
    public class RatePreset : ScriptableObject
    {
        public float pitchRcRate = 1f;
        public float pitchSuperRate = 0.7f;
        public float pitchExpo = 0f;

        public float yawRcRate = 1f;
        public float yawSuperRate = 0.7f;
        public float yawExpo = 0f;

        public float rollRcRate = 1f;
        public float rollSuperRate = 0.7f;
        public float rollExpo = 0f; 
    }
}