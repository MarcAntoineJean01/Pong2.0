public enum DialogType
{
    GreetingsFromPoly,
    GreetingsFromPixy,
    ChallengeFromPoly,
    StageIntro,
    StageOutro,
    Taunt,
    ExplainSpikes,
    ExplainDebuffs,
    WillToGetOut,
    AweForCreation,
    ScreenFrustration,
    Scolding,
    FirstFeelLighter,
    LastFeelLighter,
    ChatSingularity,
    CustomPoly,
    CustomPixy
}
public struct CubesToHideFromFace
{
    public int face;
    public int[] cubesToHide;
    public CubesToHideFromFace(int face, int[] cubesToHide)
    {
        this.face = face;
        this.cubesToHide = cubesToHide;
    }
}