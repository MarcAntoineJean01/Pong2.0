using System.Collections.Generic;
using UnityEngine;
using UiLocker;
public class DialogManager : PongManager
{
    //DIALOG ON IS BROKEN FIX IT, CAN'T CHECK ON ONE BOOL FOR ALL BUBBLES
    //BOOL SHOULD BE CHECKING IF ANY BUBBLE IS STILL ALIVE
    // public static bool dialogOn => activeSpeechBubbles.Count > 0;
    // bool explainedRules = false;
    // bool explainedRulesTwice = false;
    // bool explainedRulesTrice = false;
    // Side explainedRulesTo;
    public string currentScores => leftPlayer.health + " : " + rightPlayer.health;
    // bool leftPlayerPity = false;
    // bool rightPlayerPity = false;
    // string lastTaunt;
    // string lastBigTaunt;
    // string lastSpeedUp;
    public PixySpeechBubble pixySpeechBubble;
    public PolySpeechBubble polySpeechBubble;
    public GameObject pixyBubbleGhost;
    public GameObject polyBubbleGhost;
    public bool activePolyBubble => polySpeechBubble.gameObject.activeSelf;
    public bool activePixyBubble => pixySpeechBubble.gameObject.activeSelf;
    public void MakePixySpeechBubble(List<string> speech, bool orientation, bool permanent = false)
    {
        pixySpeechBubble.speech = speech;
        pixySpeechBubble.permanent = permanent;
        pixySpeechBubble.orientation = orientation;
        pixySpeechBubble.gameObject.SetActive(true);
    }
    public void MakeSPeechBubble(DialogType dialogType, string customString = "", bool makeGhost = false)
    {
        NewSpeechBubble newBubble = null;
        // int scoreDifference;
        switch (dialogType)
        {
            case DialogType.GreetingsFromPoly:
                newBubble = polySpeechBubble;
                newBubble.speech = Dialogs.greetingsFromPoly;
                break;
            case DialogType.GreetingsFromPixy:
                newBubble = pixySpeechBubble;
                newBubble.speech = Dialogs.greetingsFromPixy;
                break;
            case DialogType.ChallengeFromPoly:
                newBubble = polySpeechBubble;
                newBubble.speech = Dialogs.challengeFromPoly;
                break;
            case DialogType.StageIntro:
                newBubble = polySpeechBubble;
                newBubble.speech = Dialogs.StageIntro(currentStage);
                break;
            case DialogType.StageOutro:
                newBubble = polySpeechBubble;
                newBubble.speech = Dialogs.StageOutro(currentStage);
                break;
            // case DialogType.Taunt:
            //     newBubble.speech = taunt;
            //     activeSpeechBubbles.Add(newBubble);
            //     break;
            case DialogType.ExplainSpikes:
                newBubble = pixySpeechBubble;
                newBubble.speech = Dialogs.explainSpikes;
                break;
            case DialogType.ExplainDebuffs:
                newBubble = pixySpeechBubble;
                newBubble.speech = Dialogs.explainDebuffs;
                break;
            case DialogType.WillToGetOut:
                newBubble = polySpeechBubble;
                newBubble.speech = Dialogs.willToGetOut;
                break;
            case DialogType.AweForCreation:
                newBubble = polySpeechBubble;
                newBubble.speech = Dialogs.aweForCreation;
                break;
            case DialogType.ScreenFrustration:
                newBubble = polySpeechBubble;
                newBubble.speech = Dialogs.screenFrustration;
                break;
            case DialogType.FirstFeelLighter:
                newBubble = polySpeechBubble;
                newBubble.speech = new List<string>() { "AHHHHHH, it's taken a piece out of me!", "hmm, actually this feels fine", "i feel a lot lighter!", "carry on." };
                break;
            case DialogType.LastFeelLighter:
                newBubble = polySpeechBubble;
                newBubble.speech = new List<string>() { "Look at all these new faces!", "i feel great", "being a cube was very limiting", "i feel i can do so much more now" };
                break;
            case DialogType.Scolding:
                newBubble = pixySpeechBubble;
                newBubble.speech = Dialogs.scolding;
                break;
            case DialogType.ChatSingularity:
                newBubble = polySpeechBubble;
                newBubble.speech = new List<string>() { "LOREM IPSUM" };
                break;
            case DialogType.CustomPoly:
                newBubble = polySpeechBubble;
                newBubble.speech = new List<string>() { customString };
                break;
            case DialogType.CustomPixy:
                newBubble = pixySpeechBubble;
                newBubble.speech = new List<string>() { customString };
                break;
        }
        if (newBubble != null)
        {
            newBubble.makeGhost = makeGhost;
            newBubble.gameObject.SetActive(true);
        }

    }
}