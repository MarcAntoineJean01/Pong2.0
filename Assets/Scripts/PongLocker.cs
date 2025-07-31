using UnityEngine;
public enum GameMode
{
    Time,
    Goals
}
public enum GamePhase
{
    Startup,
    Playing,
    CutScene,
    Pause
}
public enum Stage
{
    StartMenu,
    DD,
    DDD,
    Universe,
    GravityWell,
    FreeMove,
    FireAndIce,
    Neon,
    Final
}
public enum TransitionStep
{
    MoveCamera,
    SwitchCameras,
    MoveEntities,
    UpdateField,
    ResetXY
}
public enum Entity
{
    Ball,
    Spike,
    LeftPad,
    RightPad
}
public enum Side
{
    None,
    Top,
    Bottom,
    Left,
    Right,
}
public struct Player
{
    public PlayerController controller { get; set; }
    public int health { get; private set; }
    public ProgressBar healthBarFull;
    public ProgressBar healthBarSplit;
    public ProgressBar healthBar => PongBehaviour.currentStage == Stage.Neon ? healthBarSplit : healthBarFull;
    public float wallNoiseBase;

    public Player(PlayerController controller, int health, ProgressBar healthBar, ProgressBar healthBarSplit)
    {
        this.controller = controller;
        this.healthBarFull = healthBar;
        this.healthBarSplit = healthBarSplit;
        this.health = this.healthBarFull.current = this.healthBarFull.maximum = health;
        this.wallNoiseBase = 0;
        this.healthBar.minimum = 0;
    }
    public void UpdateHealth(int amount, bool remove = false)
    {
        switch (remove)
        {
            case false:
                this.health += amount;
                break;
            case true:
                this.health -= amount;
                break;
        }
        this.healthBar.current = this.health;
    }

}
[System.Serializable]
public struct PongPosRot
{
    public Vector3 postion;
    public Quaternion rotation;
    public PongPosRot(Vector3 postion, Quaternion rotation)
    {
        this.postion = postion;
        this.rotation = rotation;
    }

}
[System.Serializable]
public struct MainGameSettings
{
    public GameMode gameMode;
    public bool cutScenesOn;
    public bool inGameDialogsOn;
    public bool tutorialsOn;
    public PlayerController leftPlayerController;
    public PlayerController rightPlayerController;

}
[System.Serializable]
public struct GameOptions
{
    [Range(0, 1)]
    public float soundVolume;
    public float musicVolume => (m_MusicVolume + soundVolume) / 2;
    public float effectsVolume => (m_EffectsVolume + soundVolume) / 2;
    [Range(0, 1)]
    public float m_MusicVolume;
    [Range(0, 1)]
    public float m_EffectsVolume;
    public AllowedSpikes allowedSpikes;
    public AllowedDebuffs allowedDebuffs;

    [Range(10, 60)]
    public int timeThreshold;
    [Range(3, 20)]
    public int goalsThreshold;
    [Range(1, 20)]
    public int padMaxMagnetCharges;
    [Range(5, 200)]
    public int startingHealth;
}
public enum Setting
{
    GameMode,
    CutScenesOn,
    InGameDialogsOn,
    TutorialsOn,
    LeftPlayerController,
    RightPlayerController,
    SoundVolume,
    MusicVolume,
    EffectsVolume,
    AllowedSpikes,
    AllowedDebuffs,
    TimeThreshold,
    GoalsThreshold,
    PadMaxMagnetCharges,
    StartingHealth,
    Stage
}
