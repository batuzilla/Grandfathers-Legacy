using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    const string MusicKey = "vol_music";
    const string SfxKey = "vol_sfx";

    static AudioManager inst;

    public static AudioManager Instance
    {
        get
        {
            if (inst == null)
            {
                var go = new GameObject("AudioManager");
                inst = go.AddComponent<AudioManager>();
            }
            return inst;
        }
    }

    AudioSource music;
    AudioSource sfx;
    float musicVol = 0.5f;
    float sfxVol = 0.8f;
    float fade = 1f;
    AudioClip current;
    Coroutine routine;

    public float MusicVolume
    {
        get { return musicVol; }
        set { musicVol = Mathf.Clamp01(value); PlayerPrefs.SetFloat(MusicKey, musicVol); ApplyMusic(); }
    }

    public float SfxVolume
    {
        get { return sfxVol; }
        set { sfxVol = Mathf.Clamp01(value); PlayerPrefs.SetFloat(SfxKey, sfxVol); }
    }

    void Awake()
    {
        if (inst != null && inst != this) { Destroy(gameObject); return; }
        inst = this;
        DontDestroyOnLoad(gameObject);

        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = false;
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;

        musicVol = PlayerPrefs.GetFloat(MusicKey, 0.5f);
        sfxVol = PlayerPrefs.GetFloat(SfxKey, 0.8f);
        ApplyMusic();
    }

    void ApplyMusic()
    {
        if (music != null) music.volume = musicVol * fade;
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == current && (clip == null || music.isPlaying)) return;
        current = clip;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SwitchMusic(clip, loop));
    }

    public void StopMusic()
    {
        PlayMusic(null);
    }

    IEnumerator SwitchMusic(AudioClip clip, bool loop)
    {
        while (fade > 0f && music.isPlaying)
        {
            fade = Mathf.Max(0f, fade - Time.unscaledDeltaTime / 0.5f);
            ApplyMusic();
            yield return null;
        }

        music.Stop();
        music.clip = clip;
        music.loop = loop;
        if (clip != null) music.Play();

        while (fade < 1f)
        {
            fade = Mathf.Min(1f, fade + Time.unscaledDeltaTime / 1f);
            ApplyMusic();
            yield return null;
        }
        routine = null;
    }

    public void PlaySfx(AudioClip clip, float volume = 1f, float pitchVariance = 0.06f)
    {
        if (clip == null) return;
        sfx.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        sfx.PlayOneShot(clip, volume * sfxVol);
    }

    public void PlayJingle(AudioClip clip)
    {
        StopMusic();
        if (clip == null) return;
        sfx.pitch = 1f;
        sfx.PlayOneShot(clip, Mathf.Max(sfxVol, 0.5f));
    }

    public static void Play(AudioClip clip, float volume = 1f)
    {
        if (clip != null) Instance.PlaySfx(clip, volume);
    }
}
