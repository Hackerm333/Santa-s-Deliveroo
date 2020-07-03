using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource sourceSfx;
    [SerializeField] private AudioSource sourceMusic;

    [Header("AudioClips")] 
    public AudioClip selectionClip;
    public AudioClip deselection;
    public AudioClip itemDeliveredClip;
    public AudioClip itemCollectedClip;
    public AudioClip unitCapturedClip;
    public AudioClip newDestination;
    public AudioClip cantPickItem;
    private void Start()
    {
        GameManager.Instance.onGameStateChanged.AddListener(HandleGameStateChanged);
    }

    private void HandleGameStateChanged(GameManager.GameState arg0, GameManager.GameState arg1)
    {
        if (GameManager.Instance.currentGameState == GameManager.GameState.Paused)
        {
            sourceMusic.Pause();
            sourceSfx.Pause();
        }
        else
        {
            sourceMusic.UnPause();
            sourceSfx.UnPause();
        }
    }

    public void PlayAudio(AudioClip clip)
    {
        sourceSfx.clip = clip;
        sourceSfx.Play();
    }

    public void PlayMusic()
    {
        sourceMusic.Play();
    }

    public void StopSounds()
    {
        sourceMusic.Stop();
        sourceSfx.Stop();
    }
}