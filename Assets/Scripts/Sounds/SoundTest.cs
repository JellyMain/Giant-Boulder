using Sirenix.OdinInspector;
using UnityEngine;


public class SoundTest : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    
    
    [Button]
    public void PlaySound()
    {
        audioSource.Play();
    }
}
