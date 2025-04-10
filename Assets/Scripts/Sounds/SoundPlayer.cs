using StaticData.Data;
using StaticData.Services;
using UnityEngine;


namespace Sounds
{
    public class SoundPlayer
    {
        private readonly StaticDataService staticDataService;


        public SoundPlayer(StaticDataService staticDataService)
        {
            this.staticDataService = staticDataService;
        }

        
        public void PlayMissileExplosionSound(Vector3 position)
        {
            PlaySound(staticDataService.SoundConfig.missileExplosionSound, position);
        }


        public void PlayCoinCollectedSound(Vector3 position)
        {
            PlaySound(staticDataService.SoundConfig.coinCollectedSound, position);
        }


        public void PlaySound(SoundSettings soundSettings, Vector3 position)
        {
            if (soundSettings.sounds.Length != 0)
            {
                AudioClip randomSound = soundSettings.sounds[Random.Range(0, soundSettings.sounds.Length)];
            
                GameObject spawnedSound = new GameObject($"{randomSound.name}_Sound");
                spawnedSound.transform.position = position;

                AudioSource audioSource = spawnedSound.AddComponent<AudioSource>();
            
                audioSource.clip = randomSound;
                audioSource.spatialBlend = 1;
                audioSource.pitch = Random.Range(soundSettings.minSoundPitch, soundSettings.maxSoundPitch);
                audioSource.Play();

                Object.Destroy(spawnedSound, audioSource.clip.length);   
            }
            else
            {
                Debug.LogError("There are no sounds assigned");
            }
        }
    }
}
