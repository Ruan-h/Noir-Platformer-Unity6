using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private AudioSource source;

    [Header("Clipes de Audio")]
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip killClip;
    [SerializeField] private AudioClip playerDeathClip;

    [Header("Configuração de Variação")]
    [Range(0f, 0.5f)] 
    [SerializeField] private float pitchVariation = 0.1f; 

    [Header("Configuração de Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 0.6f;

    private void OnValidate()
    {
        if (source == null) source = GetComponent<AudioSource>();
    }

    public void PlayDash()
    {
        PlaySound(dashClip, 1.0f); 
    }

    public void PlayJump()
    {
        PlaySound(jumpClip, 0.7f); 
    }

    public void PlayKillConfirm()
    {
        PlaySound(killClip, 1.2f); 
    }
    
    public void PlayPlayerDeath()
    {
        if (playerDeathClip != null && source != null)
        {
            source.pitch = 1.0f - Random.Range(0f, 0.2f);
            

            source.PlayOneShot(playerDeathClip, deathVolume);

        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && source != null)
        {
            source.pitch = 1.0f + Random.Range(-pitchVariation, pitchVariation);

            source.PlayOneShot(clip, volume);
        }
    }
}
