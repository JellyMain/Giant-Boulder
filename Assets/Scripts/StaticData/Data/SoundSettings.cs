using System;
using UnityEngine;


namespace StaticData.Data
{
    [Serializable]
    public class SoundSettings
    {
        public AudioClip[] sounds;
        public float minSoundPitch = 0.8f;
        public float maxSoundPitch = 1.1f;
    }
}
