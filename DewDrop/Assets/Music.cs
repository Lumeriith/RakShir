using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Music : MonoBehaviour
{
    public static Music instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<Music>();
            if (_instance == null)
            {
                AudioMixer mixer = Resources.Load("DewMixer") as AudioMixer;
                _instance = new GameObject("Music Manager", typeof(Music), typeof(AudioSource)).GetComponent<Music>();
                _instance.source = _instance.GetComponent<AudioSource>();
                _instance.source.outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
                _instance.source.loop = true;
            }
            return _instance;
        }
    }
    private static Music _instance;
    private AudioSource source;

    public float fadeoutDuration = 2f;
    public float fadeinDuration = 1f;

    public AudioClip pendingClip;

    private void Awake()
    {
        if(_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this);
    }

    public static void Play(AudioClip clip)
    {
        instance.pendingClip = clip;
    }

    public static void Play(string musicName)
    {
        instance.pendingClip = Resources.Load("Musics/" + musicName) as AudioClip;
    }

    private void Update()
    {
        if (pendingClip == source.clip) pendingClip = null;
        if (source.clip != null && source.clip.name != "Silence" && !source.isPlaying) source.Play();
        if(pendingClip != null)
        {
            if (source.isPlaying) source.volume -= 1f / fadeoutDuration * Time.deltaTime;
            else source.volume = 0f;
            if(source.volume <= 0)
            {
                source.Stop();
                source.clip = pendingClip;
                source.Play();
                pendingClip = null;
            }
        }
        else
        {
            source.volume = Mathf.MoveTowards(source.volume, 1f, 1f / fadeinDuration * Time.deltaTime);
        }
    }
}
