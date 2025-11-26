using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private AudioSource bgmSource;

    [Header("Music Clips")]
    public AudioClip normalLoop;
    public AudioClip winLoop;
    public AudioClip loseLoop;
    public AudioClip titleLoop;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public void PlayNormalLoop()
    {
        PlayLoop(normalLoop);
    }

    public void PlayWinLoop()
    {
        PlayLoop(winLoop);
    }

    public void PlayLoseLoop()
    {
        PlayLoop(loseLoop);
    }

    public void PlayTitleLoop()
    {
        PlayLoop(titleLoop);
    }

    private void PlayLoop(AudioClip clip)
    {
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlaySoundFX(AudioClip clip, Transform transform)
    {
        AudioSource audioSource = Instantiate(audioSourcePrefab, transform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.Play();
        Destroy(audioSource.gameObject, clip.length);
    }
}
