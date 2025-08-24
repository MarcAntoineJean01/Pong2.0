using UnityEngine;
using System.Collections.Generic;
using PongGame.PongLocker;
using System;

namespace PongGame.UiLocker
{
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
    public struct CubeFaceIndexAndDirection
    {
        public Side dir;
        public int faceIndex;
        public CubeFaceIndexAndDirection(Side dir, int faceIndex)
        {
            this.dir = dir;
            this.faceIndex = faceIndex;

        }
    }
    public struct CubeNextFaceIndexAndTextRotation
    {
        public int nextFaceIndex { get; private set; }
        public Quaternion cubeStartingRotation { get; private set; }
        public Vector3 textRotation { get; private set; }
        public CubeNextFaceIndexAndTextRotation(Quaternion cubeStartingRotation, int nextFaceIndex, Vector3 textRotation)
        {
            this.cubeStartingRotation = cubeStartingRotation;
            this.nextFaceIndex = nextFaceIndex;
            this.textRotation = textRotation;
        }
    }
    public static class CubeDirectory
    {
        static int threshold = 1;
        public static Dictionary<CubeFaceIndexAndDirection, List<CubeNextFaceIndexAndTextRotation>> nextFaceIndexAndRotationDict = new Dictionary<CubeFaceIndexAndDirection, List<CubeNextFaceIndexAndTextRotation>>()
        {
            {
                new CubeFaceIndexAndDirection(Side.Top, 5), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 0, -1), 3, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 1, 0), 1, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, -0.70711f, 0.70711f), 0, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 0.70711f, 0.70711f), 2, Vector3.zero),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Top, 4), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -1, 0, 0), 3, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-1, 0, 0, 0), 1, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, -0.70711f, 0, 0), 2, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0.70711f, 0, 0), 0, Vector3.forward * 180),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Top, 3), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, 0, -0.70711f), 4, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, -0.70711f, 0), 5, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.5f, -0.5f, -0.5f, 0.5f), 0, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, -0.5f, -0.5f), 2, Vector3.forward * 270),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Top, 2), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, 0.5f, -0.5f, -0.5f), 4, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, -0.5f, 0.5f), 5, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0.70711f, 0, -0.70711f), 3, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, -0.70711f, 0), 1, Vector3.forward * 270),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Top, 1), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, 0.70711f, 0), 4, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.70711f, 0, 0, -0.70711f), 5, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, 0.5f, 0.5f), 2, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.5f, -0.5f, 0.5f, -0.5f), 0, Vector3.forward * 270),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Top, 0), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, 0.70711f, 0), 1, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, 0, -0.70711f), 3, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, 0.5f, 0.5f, 0.5f), 5, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, 0.5f, -0.5f), 4, Vector3.forward * 180),
                }
            },

            {
                new CubeFaceIndexAndDirection(Side.Bottom, 5), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 0, -1), 1, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 1, 0), 3, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 0.70711f, -0.70711f), 2, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 0.70711f, 0.70711f), 0, Vector3.forward * 180),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Bottom, 4), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -1, 0, 0), 1, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-1, 0, 0, 0), 3, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.70711f, -0.70711f, 0, 0), 2, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, -0.70711f, 0, 0), 0, Vector3.zero),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Bottom, 3), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, -0.70711f, 0), 4, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, 0, -0.70711f), 5, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, 0.5f, 0.5f, -0.5f), 2, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, -0.5f, -0.5f), 0, Vector3.forward * 270),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Bottom, 2), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, 0, 0.70711f), 1, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, -0.70711f, 0), 3, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, -0.5f, 0.5f), 4, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.5f, -0.5f, 0.5f, 0.5f), 5, Vector3.forward * 270),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Bottom, 1), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.70711f, 0, 0, -0.70711f), 4, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, 0.70711f, 0), 5, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, 0.5f, 0.5f), 0, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.5f, -0.5f, 0.5f, -0.5f), 2, Vector3.forward * 270),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Bottom, 0), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, 0, -0.70711f), 1, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, 0.70711f, 0), 3, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, 0.5f, 0.5f, 0.5f), 4, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, 0.5f, -0.5f), 5, Vector3.forward * 90),
                }
            },

            {
                new CubeFaceIndexAndDirection(Side.Left, 5), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 0, -1), 2, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, -1, 0), 0, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 0.70711f, 0.70711f), 1, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, -0.70711f, 0.70711f), 3, Vector3.forward * 90),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Left, 4), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -1, 0, 0), 0, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-1, 0, 0, 0), 2, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, -0.70711f, 0, 0), 3, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0.70711f, 0, 0), 1, Vector3.forward * 270),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Left, 3), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, 0, -0.70711f), 2, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0.70711f, 0.70711f, 0), 0, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, -0.5f, -0.5f), 5, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.5f, -0.5f, -0.5f, 0.5f), 4, Vector3.zero),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Left, 2), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0.70711f, 0, -0.70711f), 4, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, 0.5f, -0.5f, -0.5f), 1, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, -0.5f, 0.5f), 3, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, -0.70711f, 0), 5, Vector3.forward * 180),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Left, 1), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.70711f, 0, 0, -0.70711f), 2, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, 0.70711f, 0), 0, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, 0.5f, 0.5f), 4, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.5f, -0.5f, 0.5f, -0.5f), 5, Vector3.forward * 90),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Left, 0), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, 0, -0.70711f), 5, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, 0.5f, 0.5f, 0.5f), 1, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, 0.5f, -0.5f), 3, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, 0.70711f, 0), 4, Vector3.forward * 270),
                }
            },

            {
                new CubeFaceIndexAndDirection(Side.Right, 5), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 0, -1), 0, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 0.70711f, -0.70711f), 1, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 1, 0), 2, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0, 0.70711f, 0.70711f), 3, Vector3.forward * 270),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Right, 4), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -1, 0, 0), 2, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-1, 0, 0, 0), 0, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.70711f, -0.70711f, 0, 0), 3, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, -0.70711f, 0, 0), 1, Vector3.forward * 90),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Right, 3), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, 0, -0.70711f), 0, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, -0.70711f, 0), 2, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, -0.5f, -0.5f), 4, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.5f, -0.5f, -0.5f, 0.5f), 5, Vector3.forward * 90),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Right, 2), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, 0.70711f, 0, -0.70711f), 5, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, -0.5f, 0.5f), 1, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.5f, -0.5f, 0.5f, 0.5f), 3, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, -0.70711f, 0), 4, Vector3.forward * 270),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Right, 1), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.70711f, 0, 0, -0.70711f), 0, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, 0.70711f, 0), 2, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, 0.5f, 0.5f), 5, Vector3.forward * 270),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0.5f, -0.5f, 0.5f, -0.5f), 4, Vector3.zero),
                }
            },
            {
                new CubeFaceIndexAndDirection(Side.Right, 0), new List<CubeNextFaceIndexAndTextRotation>()
                {
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(0, -0.70711f, 0, -0.70711f), 4, Vector3.forward * 90),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, 0.5f, 0.5f, 0.5f), 3, Vector3.forward * 180),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.5f, -0.5f, 0.5f, -0.5f), 1, Vector3.zero),
                    new CubeNextFaceIndexAndTextRotation(new Quaternion(-0.70711f, 0, 0.70711f, 0), 5, Vector3.forward * 180),
                }
            }

        };
        public static CubeNextFaceIndexAndTextRotation NextFaceIndexAndRotation(CubeFaceIndexAndDirection currentIndexAndDirection, Quaternion rotation)
        {
            return nextFaceIndexAndRotationDict[currentIndexAndDirection].Find(cubeFaceRotation => Quaternion.Angle(rotation, cubeFaceRotation.cubeStartingRotation) < threshold);
        }
        public static Dictionary<Tuple<int, int>, int[]> NextPreviousFaceDict = new Dictionary<Tuple<int, int>, int[]>()
        {
            { Tuple.Create(0, 0), new int[] {1, 5, 3, 4} },
            { Tuple.Create(0, 1), new int[] {2, 5, 4, 0} },
            { Tuple.Create(0, 2), new int[] {3, 5, 4, 1} },
            { Tuple.Create(0, 3), new int[] {0, 5, 4, 2} },
            { Tuple.Create(0, 4), new int[] {3, 2, 0, 1} },
            { Tuple.Create(0, 5), new int[] {0, 1, 3, 2} },
            { Tuple.Create(90, 0), new int[] {5, 3, 1, 4} },
            { Tuple.Create(90, 1), new int[] {0, 5, 2, 4} },
            { Tuple.Create(90, 2), new int[] {1, 5, 3, 4} },
            { Tuple.Create(90, 3), new int[] {2, 5, 0, 4} },
            { Tuple.Create(90, 4), new int[] {2, 1, 0, 3} },
            { Tuple.Create(90, 5), new int[] {2, 1, 0, 3} },
            { Tuple.Create(180, 0), new int[] {4, 3, 5, 1} },
            { Tuple.Create(180, 1), new int[] {4, 0, 2, 5} },
            { Tuple.Create(180, 2), new int[] {4, 1, 3, 5} },
            { Tuple.Create(180, 3), new int[] {4, 2, 0, 5} },
            { Tuple.Create(180, 4), new int[] {0, 1, 3, 2} },
            { Tuple.Create(180, 5), new int[] {3, 2, 0, 1} },
            { Tuple.Create(270, 0), new int[] {1, 4, 5, 3} },
            { Tuple.Create(270, 1), new int[] {4, 2, 5, 0} },
            { Tuple.Create(270, 2), new int[] {4, 3, 5, 1} },
            { Tuple.Create(270, 3), new int[] {4, 0, 5, 2} },
            { Tuple.Create(270, 4), new int[] {3, 0, 1, 2} },
            { Tuple.Create(270, 5), new int[] {3, 0, 1, 2} },
        };

        public static int[] NextPreviousFaces(Tuple<int, int> key)
        {
            return NextPreviousFaceDict[key];
        }
        public static int[] PreviousFaces(Tuple<int, int> key)
        {
            return new int[] { NextPreviousFaceDict[key][2], NextPreviousFaceDict[key][3] };
        }
        public static int[] NextFaces(Tuple<int, int> key)
        {
            return new int[] { NextPreviousFaceDict[key][0], NextPreviousFaceDict[key][1] };
        }

        public static Dictionary<int, CubesToHideFromFace[]> cubesToHideForMenuDict = new Dictionary<int, CubesToHideFromFace[]>()
        {
            {0, new CubesToHideFromFace[]{
                    new CubesToHideFromFace(2, new int[4] {0,1,2,3}),
                    new CubesToHideFromFace(3, new int[4] {12,13,14,15}),
                    new CubesToHideFromFace(4, new int[4] {3,7,11,15}) ,
                    new CubesToHideFromFace(5, new int[4] {0,4,8,12})
                }
            },
            {1, new CubesToHideFromFace[]{
                    new CubesToHideFromFace(2, new int[4] {12,13,14,15}),
                    new CubesToHideFromFace(3, new int[4] {0,1,2,3}),
                    new CubesToHideFromFace(4, new int[4] {0,4,8,12}) ,
                    new CubesToHideFromFace(5, new int[4] {3,7,11,15})
                }
            },
            {2, new CubesToHideFromFace[]{
                    new CubesToHideFromFace(0, new int[4] {12,13,14,15}),
                    new CubesToHideFromFace(1, new int[4] {0,1,2,3}),
                    new CubesToHideFromFace(4, new int[4] {12,13,14,15}) ,
                    new CubesToHideFromFace(5, new int[4] {12,13,14,15})
                }
            },
            {3, new CubesToHideFromFace[]{
                    new CubesToHideFromFace(0, new int[4] {0,1,2,3}),
                    new CubesToHideFromFace(1, new int[4] {12,13,14,15}),
                    new CubesToHideFromFace(4, new int[4] {0,1,2,3}),
                    new CubesToHideFromFace(5, new int[4] {0,1,2,3})
                }
            },
            {4, new CubesToHideFromFace[]{
                    new CubesToHideFromFace(0, new int[4] {0,4,8,12}),
                    new CubesToHideFromFace(1, new int[4] {0,4,8,12}),
                    new CubesToHideFromFace(2, new int[4] {0,4,8,12}),
                    new CubesToHideFromFace(3, new int[4] {0,4,8,12})
                }
            },
            {5, new CubesToHideFromFace[]{
                    new CubesToHideFromFace(0, new int[4] {3,7,11,15}),
                    new CubesToHideFromFace(1, new int[4] {3,7,11,15}),
                    new CubesToHideFromFace(2, new int[4] {3,7,11,15}),
                    new CubesToHideFromFace(3, new int[4] {3,7,11,15})
                }
            }
        };
    }
}

