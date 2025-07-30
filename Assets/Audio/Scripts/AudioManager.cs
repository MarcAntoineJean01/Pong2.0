using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AudioManager : PongManager
{
    public AudioList audioList;
    public GameObject audioSourcePrefab;
    public float bpm;
    public AudioSource musicAudioSource;
    public MusicClip currentMusicClip;
    public AudioSource secondaryMusicAudioSource;
    public BeatIntervals[] beatIntervals;
    public bool beatsOn
    {
        get
        {
            bool inBeatSection = false;
            foreach (BeatSection beatSection in currentMusicClip.beatSections)
            {
                if (musicAudioSource.time >= beatSection.start && musicAudioSource.time <= beatSection.end) { inBeatSection = true; break; }
            }
            return inBeatSection && secondaryMusicAudioSource == null;
        }
    }
    List<AudioSource> liveEffectsAudios = new List<AudioSource>();
    void Update()
    {
        if (musicAudioSource)
        {
            if (beatsOn)
            {
                foreach (BeatIntervals beat in beatIntervals)
                {
                    float sampleTime = musicAudioSource.timeSamples / (musicAudioSource.clip.frequency * beat.GetBeatLength(bpm));
                    beat.CheckForNewBeat(sampleTime);
                }
            }
        }

    }
    public void PlayMusic(MusicType musicType)
    {
        if (musicType == MusicType.DissolveMusic)
        {
            AudioSource audioSource = Instantiate(audioSourcePrefab, field.ball.transform).GetComponent<AudioSource>();
            audioSource.transform.localPosition = Vector3.zero;
            audioSource.clip = PickMusicClip(musicType).clip;
            audioSource.volume = options.musicVolume;
            if (secondaryMusicAudioSource != null)
            {
                GameObject.Destroy(secondaryMusicAudioSource.gameObject);
            }
            secondaryMusicAudioSource = audioSource;
            secondaryMusicAudioSource.loop = true;
            secondaryMusicAudioSource.Play();
        }
        else
        {
            currentMusicClip = PickMusicClip(musicType);
            musicAudioSource.clip = currentMusicClip.clip;
            musicAudioSource.volume = options.musicVolume;
            musicAudioSource.pitch = 1;
            musicAudioSource.loop = true;
            bpm = currentMusicClip.bpm;
            musicAudioSource.Play();
        }
    }
    public void PlayAudio(AudioType audioType, Vector3 position, float? duration = null)
    {
        AudioSource audioSource = Instantiate(audioSourcePrefab, position, Quaternion.identity).GetComponent<AudioSource>();
        liveEffectsAudios.Add(audioSource);
        audioSource.clip = PickAudioClip(audioType);
        audioSource.volume = options.effectsVolume;
        audioSource.Play();
        if (duration != null)
        {
            StartCoroutine(KillAudioAfterTime(audioSource, duration.Value));
        }
        else
        {
            StartCoroutine(KillAudioAfterTime(audioSource, audioSource.clip.length));
        }
    }

    AudioClip PickAudioClip(AudioType audioType)
    {
        switch (audioType)
        {
            default:
                return null;
            case AudioType.PadBounce:
                return audioList.ballAudioClips.padBounce;
            case AudioType.BallBounce:
                return audioList.ballAudioClips.bounce;
            case AudioType.BallShieldBounce:
                return audioList.ballAudioClips.shieldBounce;
            case AudioType.BalScreenBounce:
                return audioList.ballAudioClips.screenBounce;
            case AudioType.Attractor:
                return audioList.sfxAudioClips.attractor;
            case AudioType.Repulsor:
                return audioList.sfxAudioClips.repulsor;
            case AudioType.BallSpeech:
                return audioList.ballAudioClips.fillers[UnityEngine.Random.Range(0, audioList.ballAudioClips.fillers.Count)];
            case AudioType.SingularityImplosion:
                return audioList.sfxAudioClips.singularityImplosion;
            case AudioType.UiConfirm:
                return audioList.uiAudioClips.select;
            case AudioType.UiSwitchCubes:
                return audioList.uiAudioClips.switchCubes;
            case AudioType.UiSwitchFaces:
                return audioList.uiAudioClips.switchSides;
            case AudioType.GameOverVoice:
                return audioList.sfxAudioClips.gameOverVoice;
            case AudioType.StartupSound:
                return audioList.uiAudioClips.startupSound;
            case AudioType.LaunchBall:
                return audioList.ballAudioClips.launchBall;
            case AudioType.SpikeProc:
                SpikeEntity activeSpikeType = field.spikeStore.activeSpikes.spikeLeft;
                if (activeSpikeType is SpikePadBlock)
                {
                    return audioList.spikeAudioClips.blockPieceProc;
                }
                else if (activeSpikeType is SpikeMagnet)
                {
                    return audioList.spikeAudioClips.magnetProc;
                }
                else if (activeSpikeType is SpikeWallAttractor)
                {
                    return audioList.spikeAudioClips.wallAttractorProc;
                }
                else if (activeSpikeType is SpikeDissolve)
                {
                    return audioList.spikeAudioClips.dissolveProc;
                }
                else if (activeSpikeType is SpikeRandomDirection)
                {
                    return audioList.spikeAudioClips.randomDirectionProc;
                }
                else // (activeSpikeType is SpikePadPiece)
                {
                    return audioList.spikeAudioClips.padPieceProc;
                }
            case AudioType.GainedHealth:
                return audioList.sfxAudioClips.gainedHealth;
            case AudioType.LostHealth:
                return audioList.sfxAudioClips.lostHealth;
            case AudioType.WarningLowHealth:
                return audioList.sfxAudioClips.warningLowHealth;
        }
    }
    MusicClip PickMusicClip(MusicType musicType)
    {
        switch (musicType)
        {
            default:
                return null;
            case MusicType.BackgroundMusic:
                return PickBackgroundMusicClip();
            case MusicType.GameOverMusic:
                return audioList.musicAudioClips.gameOverMusic;
            case MusicType.DissolveMusic:
                return audioList.spikeAudioClips.dissolveMusic;
        }
    }
    MusicClip PickBackgroundMusicClip()
    {
        switch (nextStage)
        {
            default:
                return audioList.musicAudioClips.introMusic;
            case Stage.Neon:
                return audioList.musicAudioClips.NeonMusic;
            case Stage.Final:
                return audioList.musicAudioClips.finalMusic;
        }
    }

    public void KillSecondaryMusic()
    {
        if (secondaryMusicAudioSource != null)
        {
            GameObject.Destroy(secondaryMusicAudioSource.gameObject);
            secondaryMusicAudioSource = null;
        }
    }
    IEnumerator KillAudioAfterTime(AudioSource audioSource, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        liveEffectsAudios.Remove(audioSource);
        GameObject.Destroy(audioSource.gameObject);
    }
    public void UpdateVolume()
    {
        musicAudioSource.volume = options.musicVolume;
        if (secondaryMusicAudioSource != null)
        {
            secondaryMusicAudioSource.volume = options.musicVolume;
        }
        foreach (AudioSource audioSource in liveEffectsAudios)
        {
            audioSource.volume = options.effectsVolume;
        }
    }

    IEnumerator CycleSmoothSwitchTracks(AudioClip newTrack)
    {
        float t = 0;
        while (t < 2)
        {
            yield return null;
        }
    }
}