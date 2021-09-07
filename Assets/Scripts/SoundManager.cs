using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Audio players components.
    public AudioSource EffectsSource;
    public AudioSource MusicSource;

    // Random pitch adjustment range.
    public float LowPitchRange = .95f;
    public float HighPitchRange = 1.05f;

    public float targetAudioVolume;
    public List<AudioClip> audioClips;

    // Singleton instance.
    public static SoundManager Instance;

    // Initialize the singleton instance.
    private void Awake()
    {
        Debug.Log("AWAKE");
        if (!PlayerPrefs.HasKey("volume-music"))
        {
            MusicSource.volume = 1;
            EffectsSource.volume = 1;
        }
        else
        {
            MusicSource.volume = PlayerPrefs.GetFloat("volume-music");
            EffectsSource.volume = PlayerPrefs.GetFloat("volume-effects");
        }

        Debug.Log("Asign");
        // If there is not already an instance of SoundManager, set it to this.
        if (Instance == null)
        {
            Instance = this;
        }
        //If an instance already exists, destroy whatever this object is to enforce the singleton.
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        //Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
        DontDestroyOnLoad(gameObject);
    }

    // Play a single clip through the sound effects source.
    public void Play(AudioClip clip)
    {
        EffectsSource.clip = clip;
        EffectsSource.Play();
    }

    // Play a single clip through the music source.
    public void PlayMusic(AudioClip clip, bool force = false)
    {
        if ((MusicSource.clip == clip)&&(force == false)){ return; }
        MusicSource.clip = clip;
        MusicSource.Play();
        MusicSource.loop = true;
    }

    // Play a random clip from an array, and randomize the pitch slightly.
    public void RandomSoundEffect(params AudioClip[] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(LowPitchRange, HighPitchRange);

        EffectsSource.pitch = randomPitch;
        EffectsSource.clip = clips[randomIndex];
        EffectsSource.Play();
    }

    IEnumerator playSoundWithDelay(AudioClip clip, float delay, float volume)
    {
        yield return new WaitForSeconds(delay);
        EffectsSource.PlayOneShot(clip, volume);
    }

    public void PlaySFX(string sfxname, bool unique = false, float volume = 1, float delay = 0)
    {
        AudioClip ac = audioClips.Find(p => (p.name == sfxname));
        if (ac != null) {
            if ((unique)&&(EffectsSource.isPlaying)) { return; }

            if (delay <= 0)
            {
                EffectsSource.PlayOneShot(ac, volume);
            }
            else
            {
                StartCoroutine(playSoundWithDelay(ac, delay, volume));
            }
        }
        else
        {
            Debug.Log("No SFX for " + sfxname);
        }
    }

    public void PlayMusic(string clipName)
    {
        AudioClip ac = audioClips.Find(p => (p.name == clipName));
        if (ac != null) { 
            PlayMusic(ac);
            nextSong = ac;
        }
        else
        {
            Debug.Log("No music for " + clipName);
        }
    }

    public AudioClip nextSong;
    public void PlayMusicNext(string clipName)
    {
        AudioClip ac = audioClips.Find(p => (p.name == clipName));
        if (ac != null) {
            nextSong = ac;
            MusicSource.loop = false;
        }
        else
        {
            Debug.Log("No music for " + clipName);
        }
    }

    public void Update()
    {
        if ((MusicSource.isPlaying == false)&&(nextSong != null))
        {
            PlayMusic(nextSong);
            nextSong = null;
        }
    }
}