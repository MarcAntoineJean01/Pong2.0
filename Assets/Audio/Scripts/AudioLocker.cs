using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MusicType
{
    GameOverMusic,
    BackgroundMusic,
    DissolveMusic
}
public enum AudioType
{
    PadBounce,
    BallBounce,
    BallShieldBounce,
    BalScreenBounce,
    Attractor,
    Repulsor,
    BallSpeech,
    SingularityImplosion,
    UiSwitchFaces,
    UiSwitchCubes,
    UiConfirm,
    StartupSound,
    LaunchBall,
    SpikeProc,
    GameOverVoice,
    WarningLowHealth,
    LostHealth,
    GainedHealth
}
[System.Serializable]
public class AudioList
{
    public BallAudioClips ballAudioClips;
    public SpikeAudioClips spikeAudioClips;
    public SFXAudioClips sfxAudioClips;
    public UiAudioClips uiAudioClips;
    public MusicAudioClips musicAudioClips;

}
[System.Serializable]
public class MusicAudioClips
{
    public MusicClip introMusic;
    public MusicClip universeMusic;
    public MusicClip NeonMusic;
    public MusicClip finalMusic;
    public MusicClip gameOverMusic;
}
[System.Serializable]
public class BallAudioClips
{
    public AudioClip bounce;
    public AudioClip padBounce;
    public AudioClip shieldBounce;
    public AudioClip screenBounce;
    public AudioClip launchBall;
    public AudioClip go;
    public AudioClip laugh;
    public AudioClip ghostly;
    public List<AudioClip> fillers = new List<AudioClip>();
}
[System.Serializable]
public class SFXAudioClips
{
    public AudioClip attractor;
    public AudioClip repulsor;
    public AudioClip singularityImplosion;
    public AudioClip singularityNoise;
    public AudioClip gameOverVoice;
    public AudioClip lostHealth;
    public AudioClip gainedHealth;
    public AudioClip warningLowHealth;
}
[System.Serializable]
public class SpikeAudioClips
{
    public AudioClip spawn;
    public AudioClip death;
    public AudioClip bounce;
    public AudioClip padPieceProc;
    public AudioClip blockPieceProc;
    public AudioClip magnetProc;
    public AudioClip wallAttractorProc;
    public AudioClip dissolveProc;
    public AudioClip randomDirectionProc;
    public AudioClip healthUpProc;
    public MusicClip dissolveMusic;

    public AudioClip SpikeProc(SpikeType spikeType)
    {
        switch (spikeType)
        {
            default:
                return null;
            case SpikeType.SpikePadPiece:
                return padPieceProc;
            case SpikeType.SpikePadBlock:
                return blockPieceProc;
            case SpikeType.SpikeMagnet:
                return magnetProc;
            case SpikeType.SpikeWallAttractor:
                return wallAttractorProc;
            case SpikeType.SpikeDissolve:
                return dissolveProc;
            case SpikeType.SpikeRandomDirection:
                return randomDirectionProc;
            case SpikeType.HealthUp:
                return healthUpProc;
        }
    }
}
[System.Serializable]
public class UiAudioClips
{
    public AudioClip switchCubes;
    public AudioClip switchSides;
    public AudioClip select;
    public AudioClip startupSound;
}
[System.Serializable]
public class BeatIntervals
{
    public float steps;
    public UnityEvent beatTrigger;
    int lastBeat;
    public float GetBeatLength(float bpm)
    {
        return 60f / (bpm * steps);
    }
    public void CheckForNewBeat(float beat)
    {
        if (Mathf.FloorToInt(beat) != lastBeat)
        {
            lastBeat = Mathf.FloorToInt(beat);
            beatTrigger.Invoke();
        }
    }
}
[System.Serializable]
public class MusicClip
{
    public AudioClip clip;
    public float duration => clip.length;
    public int bpm;
    public List<BeatSection> beatSections = new List<BeatSection>();
}
[System.Serializable]
public class BeatSection
{
    public float start;
    public float end;
}