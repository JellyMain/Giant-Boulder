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

        
        public void PlayMissileExplosionSound()
        {
            PlaySound(staticDataService.SoundConfig.missileExplosionSound);
        }


        public void PlayCoinCollectedSound()
        {
            PlaySound(staticDataService.SoundConfig.coinCollectedSound);
        }


        public void PlaySound(SoundSettings soundSettings)
        {
            if (soundSettings.sounds.Length != 0)
            {
                AudioClip randomSound = soundSettings.sounds[Random.Range(0, soundSettings.sounds.Length)];
            
                GameObject spawnedSound = new GameObject($"{randomSound.name}_Sound");

                AudioSource audioSource = spawnedSound.AddComponent<AudioSource>();
            
                audioSource.clip = randomSound;
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