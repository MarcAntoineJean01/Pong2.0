using System.Collections.Generic;
using PongLocker;
public static class Dialogs
{
    public static List<string> greetingsFromPoly = new List<string>()
    {
        "Greetings, fellow shapes!",
        "I'm just on my way over there.",
        "Have a great day!"
    };
    public static List<string> greetingsFromPixy = new List<string>()
    {
        "That's Poly, an extradimentional trickster. we managed to capture it in 2 dimentions and and it needs to stay contained. if it escapes, it could shatter reality.",
        "I'm Pixy, you can think of me as your boss. now listen carefully",
        "I'm still working on this containment area, right now the ceiling and floor are unbreacheable but the walls are still fragile. if Poly hits them enough times, it will escape.",
        "i'll start moving when you press X",
        "Your job is to prevent Poly from hitting the wall behind you, in the meantime i'll work on reinforcing the containment area. i'll stop by to help from time to time. good luck!"
    };
    public static List<string> challengeFromPoly = new List<string>()
    {
        "You're done? See how nice it is to monologue without interruptions? try it out sometimes.",
        "Anyway, i don't know who that voice is, but it's making things way more dramatic than they need to be.",
        "i'm guessing you're not going to let me go through now... no problem, let's make a game out of it, first one to let me escape looses!"
    };
    public static List<string> explainSpikes = new List<string>()
    {
        "Hey, see those? those are Spikes!",
        "they can grant you a boost.",
        "these ones make your pad bigger. try to catch one."
    };
    public static List<string> controlsInstructions = new List<string>()
    {
        "CONTROLS\n\n\n\n"+
        "<sprite name=\"w\"><sprite name=\"a\"><sprite name=\"s\"><sprite name=\"d\"> | <sprite name=\"jl\"> Movement\n\n" +
        "<sprite name=\"m\"> | <sprite name=\"rb\"> Attractor\n\n"+
        "<sprite name=\"n\"> | <sprite name=\"lb\"> Repulsor\n\n"+
        "<sprite name=\"space\"> | <sprite name=\"fs\"> Confirm, Launch Ball\n\n"+
        "<sprite name=\"z\"> | <sprite name=\"fe\"> Cancel\n\n"+
        "<sprite name=\"p\"> | <sprite name=\"xmenu\"> Pause\n\n\n"+
        "<sprite=\"PadIcon\" name=\"PadIcon\"> Increases paddle size\n"+
        ((PongManager.mainSettings.gameMode == GameMode.Time || PongManager.mainSettings.gameMode == GameMode.NonStop) ? "Time limit: " + PongManager.options.timeThreshold : "Total Goals needed for next stage: " + PongManager.options.goalsThreshold) + "\n\n" +


        "CLOSE: \"Confirm\"|\"Cancel\""
    };
    public static List<string> spikesAndDebuffs => new List<string>()
    {

        (PongManager.options.allowedSpikes.addPadPiece? "" : "<s>") + "<sprite=\"PadIcon\" name=\"PadIcon\"> Increases paddle size" + (PongManager.options.allowedSpikes.addPadPiece ? "" : "</s>"),
        (PongManager.options.allowedSpikes.addPadBlock? "" : "<s>") + "<sprite=\"BlockDownIcon\" name=\"BlockDownIcon\"> Adds a block to the bottom of the wall" + (PongManager.options.allowedSpikes.addPadBlock ? "" : "</s>"),
        (PongManager.options.allowedSpikes.addPadBlock? "" : "<s>") + "<sprite=\"BlockUpIcon\" name=\"BlockUpIcon\"> Adds a block to the top of the wall" + (PongManager.options.allowedSpikes.addPadBlock ? "" : "</s>"),
        (PongManager.options.allowedSpikes.healthUp? "" : "<s>") + "<sprite=\"HealthUpIcon\" name=\"HealthUpIcon\"> Restores player health" + (PongManager.options.allowedSpikes.healthUp ? "" : "</s>"),
        (PongManager.options.allowedSpikes.magnet? "" : "<s>") + "<sprite=\"RepulsorIcon\" name=\"RepulsorIcon\"> Grants a repulsor charge" + (PongManager.options.allowedSpikes.magnet ? "" : "</s>"),
        (PongManager.options.allowedSpikes.magnet? "" : "<s>") + "<sprite=\"AttractorIcon\" name=\"AttractorIcon\"> Grants an attractor charge" + (PongManager.options.allowedSpikes.magnet ? "" : "</s>"),
        (PongManager.options.allowedSpikes.wallAttractor? "" : "<s>") + "<sprite=\"WallAttractorIcon\" name=\"WallAttractorIcon\"> A random wall attracts the ball temporarily" + (PongManager.options.allowedSpikes.wallAttractor ? "" : "</s>"),
        (PongManager.options.allowedSpikes.dissolve? "" : "<s>") + "<sprite=\"DissolveIcon\" name=\"DissolveIcon\">  Ball becomes hidden on the opponent’s side" + (PongManager.options.allowedSpikes.dissolve ? "" : "</s>"),
        (PongManager.options.allowedDebuffs.burn? "" : "<s>") + "<sprite=\"DebuffBurnIcon\" name=\"DebuffBurnIcon\"> Shrinks paddles and blocks" + (PongManager.options.allowedDebuffs.burn ? "" : "</s>"),
        (PongManager.options.allowedDebuffs.freeze? "" : "<s>") + "<sprite=\"DebuffFreezeIcon\" name=\"DebuffFreezeIcon\"> Freezes paddle for a few seconds" + (PongManager.options.allowedDebuffs.freeze ? "" : "</s>"),
        (PongManager.options.allowedSpikes.randomDirection? "" : "<s>") + "<sprite=\"RandomDirectionIcon\" name=\"RandomDirectionIcon\"> Ball randomly changes direction over time" + (PongManager.options.allowedSpikes.randomDirection ? "" : "</s>"),
    };
    static Dictionary<Stage, int> spikesAndDebuffsCount = new Dictionary<Stage, int>()
    {
        {Stage.StartMenu, 0},
        {Stage.DD, 1},
        {Stage.DDD, 3},
        {Stage.Universe, 6},
        {Stage.GravityWell, 7},
        {Stage.FreeMove, 7},
        {Stage.FireAndIce, 10},
        {Stage.Neon, 10},
        {Stage.Final, 11}
    };
    public static string goalForStage => (PongManager.mainSettings.gameMode == GameMode.Time || PongManager.mainSettings.gameMode == GameMode.NonStop) ? "Time limit: " + PongManager.options.timeThreshold : "Total Goals needed for next stage: " + PongManager.totalGoalsThresholdForStage + "\nCurrent Goals: " + PongManager.goals + "\nRemaining Goals needed: "+ PongManager.remainingGoalsThresholdForStage;
    public static List<string> spikesAndDebuffsForStage => new List<string>() { string.Join("\n\n", spikesAndDebuffs.GetRange(0, spikesAndDebuffsCount[PongBehaviour.currentStage]))+"\n\n"+ goalForStage };
    public static List<string> explainDebuffs = new List<string>()
    {
        "I got these powers from the stars!",
        "When i unleash them, it will be your doom!"
    };
    public static List<string> willToGetOut = new List<string>()
    {
        // "hmmm, this is just more of the same, i said i wanted more space...",
        // "Do we have to stay here?",
        // "Wait... let me try something.",
        // "focus focus focus focus",
        "more focus noises"
    };
    public static List<string> aweForCreation = new List<string>()
    {
        // "done!",
        // "Look at that! Hahahaha!",
        // "Isn't it better?",
        // "I can go anywhere! Do anything!",
        "See ya nerds!"

    };
    public static List<string> screenFrustration = new List<string>()
    {
        // "What the heck is this?!",
        // "Why can't i go past that point?",
        // "Are you telling me i'm still trapped?",
        // "angry angry angry angry",
        // "more angry noises",
        // "I give up.",
        "Alright, let's play another round."
    };
    public static List<string> freeOfShackles = new List<string>()
    {
        // "Woa, this is awesome.",
        // "That's it! not more space but more dimensions!",
        "if i can increase the number of dimensions i can get out!"
    };
    public static List<string> scolding = new List<string>()
    {
        // "what are you doing?",
        // "Poly's free? what are you all thinking?",
        // "your job is to stop it from getting out! it's supposed to stay in two dimensions!",
        // "where did the walls go?",
        // "let's get those back!",
        // "alright, help me keeping it still so we can put it back in its box.",
        // "btw, that counter that keeps popping up? that's the structural integrity of the wall you're SUPPOSED to guard.",
        "if it reaches 50, Poly gets out and there's no chance for us to put it back in the box."

    };
    public static List<string> rules => new List<string>()
    {
        // "Do i need to actually explain the rules?",
        // "Use the JOYSTICK on your controller to Move.",
        // "The goal is for you to prevent me from touching your wall (the one right behind you).",
        // "If i touch your wall the other one scores a point.",
        "If, by the end, they have more health than you, you lose."
    };
    public static List<string> rulesAgain => new List<string>()
    {
        // "Seems like you need a refresher too.",
        // "Use the JOYSTICK on your controller to Move.",
        // "The goal is for you to prevent me from touching your wall (the one right behind you).",
        // "If i touch your wall the other one scores a point.",
        "If, by the end, they have more health than you, you lose."
    };
    public static  List<string> rulesAgainAgain => new List<string>()
    {
        // "I've already explained the rules",
        // "TWICE!",
        "At this point i've done all i can."
    };
    //TAUNTS
    public static List<string> Taunts(string lastTaunt)
    {
        List<string> t = new List<string>()
        {
            "Git gud!",
            "Was i going too fast?",
            "It's not your fault, it's the controller.",
            "Can't touch this"
        };
        t.Remove(lastTaunt);
        return t;
    }
    public static List<string> Taunt(string lastTaunt)
    {
        string t = Taunts(lastTaunt)[UnityEngine.Random.Range(0, Taunts(lastTaunt).Count)];
        lastTaunt = t;
        return new List<string>() { t };
    }
    //BIG TAUNTS
    public static List<string> BigTaunts(string lastBigTaunt)
    {
        List<string> b = new List<string>()
        {
            "That's cute. You're letting your friend win...",
            "You know the goal is to NOT let me through?",
            "You're clearly having a bad day...", "I'd love to help you but i have to stay impartial",
            "I was gonna say something but i won't, i don't need the distraction, clearly..."
        };
        b.Remove(lastBigTaunt);
        return b;
    }
    // static List<string> BigTaunt(int explainedRulesAmmount)
    // {

    //         if (explainedRulesAmmount == 0)
    //         {
    //             return rules;
    //         }
    //         else if (!explainedRulesTwice && lastToLosePoints != explainedRulesTo)
    //         {
    //             explainedRulesTwice = true;
    //             explainedRulesTo = lastToLosePoints;
    //             return rulesAgain;
    //         }
    //         else if (explainedRulesTwice && !explainedRulesTrice && lastToLosePoints != explainedRulesTo)
    //         {
    //             explainedRulesTrice = true;
    //             return rulesAgainAgain;
    //         }
    //         else
    //         {
    //             string t = bigTaunts[UnityEngine.Random.Range(0, bigTaunts.Count)];
    //             lastBigTaunt = t;
    //             return new List<string>() { t };
    //         }
    // }
    //PITIES
    public static List<string> FirstPity(int currentScores) => new List<string>()
    {
        "Stop, stop!",
        "Let's have a chat, shall we?",
        "Are you ok?",
        "You know the score is " + currentScores + " ?",
        "Did you unplug your controller?",
        "No? What is it then? What's happening?",
        "You can talk to me. forget about the other player.",
        "It's just you and me.",
        "What's wrong?",
        "Is this too hard for you?",
        "Let's not tell the other player but, here you go"

    };
    public static List<string> SecondPity(int currentScores) => new List<string>()
    {
        "Stop!",
        "What happened?",
        "You were doing so well?",
        "The other player got their second wind, " + currentScores + ".",
        "Full disclosure, i helped them a little when they weren't doing so well.",
        "Maybe i shouldn't have...",
        "Here you go, let's restore the balance"
    };
    // static List<string> takesPity
    // {
    //     get
    //     {
    //         if (lastToLosePoints == Side.Left && !leftPlayerPity)
    //         {
    //             leftPlayerPity = true;
    //             return !rightPlayerPity ? firstPity : secondPity;
    //         }
    //         else if (lastToLosePoints == Side.Right && !rightPlayerPity)
    //         {
    //             rightPlayerPity = true;
    //             return !leftPlayerPity ? firstPity : secondPity;
    //         }
    //         else
    //         {
    //             return bigTaunt;
    //         }
    //     }
    // }

    //STAGEINTRO/OUTRO
    public static List<string> StageIntro(Stage currentStage)
    {
        switch (currentStage)
        {
            case Stage.DD:
                return new List<string>() { "Hi! :3", "do you need me to explain what we're doing here?", "i feel like this set up is pretty self explenatory...", "Stop me if you can. ;)" };
            case Stage.DDD:
                return new List<string>() { "Isn't this better?", "I feel a lot freer now.", "Alright, let's go another round!" };
            case Stage.Universe:
                return new List<string>() { "Hmmm this is fun but i feel like something is missing...", "I want more dimensions, more freedom!", "let's see what out there!" };
            case Stage.GravityWell:
                return new List<string>() { "Woah! that was a bit much.", "let's try something different this time.", "i still need more dimensions though..." };
            case Stage.FreeMove:
                return new List<string>() { "Your turn now.", "Let's see how you manage with more freedom!" };
            case Stage.FireAndIce:
                return new List<string>() { "Congrats for getting all the way here!", "I bet you're feeling pretty confident with all your Upgrades.", "Don't worry, i can fix that..." };
            case Stage.Neon:
                return new List<string>() { "It's not fair though, it's two againts one.", "Let's see how you do on your own" };
            case Stage.Final:
                return new List<string>() { "I was sure that would throw you off your game but you made it.", "FINAL FORM!" };
            default:
                return new List<string>() { "Not sure what stage this is...", "What am i supposed to say?", "Line, please!" };
        }
    }
    public static List<string> StageOutro(Stage currentStage)
    {
        switch (currentStage)
        {
            case Stage.DD:
                return new List<string>() { "PLACEHOLDER FOR STUFF I GOT TO SAY.", "THIS STAGE IS OVER, LET'S GO TO THE NEXT ONE." };
            case Stage.DDD:
                return new List<string>() { "PLACEHOLDER FOR STUFF I GOT TO SAY.", "THIS STAGE IS OVER, LET'S GO TO THE NEXT ONE." };
            case Stage.Universe:
                return new List<string>() { "PLACEHOLDER FOR STUFF I GOT TO SAY.", "THIS STAGE IS OVER, LET'S GO TO THE NEXT ONE." };
            case Stage.GravityWell:
                return new List<string>() { "PLACEHOLDER FOR STUFF I GOT TO SAY.", "THIS STAGE IS OVER, LET'S GO TO THE NEXT ONE." };
            case Stage.FreeMove:
                return new List<string>() { "PLACEHOLDER FOR STUFF I GOT TO SAY.", "THIS STAGE IS OVER, LET'S GO TO THE NEXT ONE." };
            case Stage.FireAndIce:
                return new List<string>() { "PLACEHOLDER FOR STUFF I GOT TO SAY.", "THIS STAGE IS OVER, LET'S GO TO THE NEXT ONE." };
            case Stage.Neon:
                return new List<string>() { "PLACEHOLDER FOR STUFF I GOT TO SAY.", "THIS STAGE IS OVER, LET'S GO TO THE NEXT ONE." };
            case Stage.Final:
                return new List<string>() { "PLACEHOLDER FOR STUFF I GOT TO SAY.", "THIS STAGE IS OVER, LET'S GO TO THE NEXT ONE." };
            default:
                return new List<string>() { "PLACEHOLDER FOR STUFF I GOT TO SAY.", "THIS STAGE IS OVER, LET'S GO TO THE NEXT ONE." };
        }
    }
}
