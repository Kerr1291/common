using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System;
using Object = UnityEngine.Object;
using System.Reflection;
using System.IO;

namespace nv.Tests
{
#if UNITY_EDITOR
    public class AStarTests
    {
        Color WriteColor(int type)
        {
            if(type == 1)
                return Color.red;
            if(type == 2)
                return Color.white;
            if(type == 3)
                return Color.green;
            return Color.clear;
        }

        int ReadColor(Color type)
        {
            if(type == Color.red)
                return 1;
            if(type == Color.white)
                return 2;
            if(type == Color.green)
                return 3;
            return 0;
        }

        string testOutputPath = "Assets/common/PathFinding/";

        IEnumerator WriteToFile(Texture2D tex, string filepath)
        {
            yield return new WaitForEndOfFrame();

            // Encode texture into PNG
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(filepath, bytes);

            yield break;
        }


        [UnityTest]
        public IEnumerator Create100by100andPathFrom0_0to99_99()
        {
            float size = 100f;
            var mapRect = new Rect(-size / 2f, -size / 2f, size, size);
            var map = new ArrayGrid<int>(mapRect.size.ToInt());
            var start = new Vector2Int(0, 0);
            var end = map.MaxValidPosition;
            bool searchDiagonal = true;
            bool debug = false;

            AStar pathFinder = new AStar();
            yield return pathFinder.FindPath(map, start, end, null, searchDiagonal, debug);
            
            Assert.That(pathFinder.result != null, Is.True, "Path failed to generate.");
            yield break;
        }

        [UnityTest]
        public IEnumerator Create1000by1000andPathFrom0_0to999_999()
        {
            float size = 1000f;
            var mapRect = new Rect(-size / 2f, -size / 2f, size, size);
            var map = new ArrayGrid<int>(mapRect.size.ToInt());
            var start = new Vector2Int(0, 0);
            var end = map.MaxValidPosition;
            bool searchDiagonal = true;
            bool debug = false;

            AStar pathFinder = new AStar();
            yield return pathFinder.FindPath(map, start, end, null, searchDiagonal, debug);

            Assert.That(pathFinder.result != null, Is.True, "Path failed to generate.");
            yield break;
        }

        [UnityTest]
        public IEnumerator Create1000by1000andPathFrom0_0to999_999AndWriteToPNGFile()
        {
            float size = 1000f;
            var mapRect = new Rect(-size / 2f, -size / 2f, size, size);
            var map = new ArrayGrid<int>(mapRect.size.ToInt());
            var start = new Vector2Int(0, 0);
            var end = map.MaxValidPosition;
            bool searchDiagonal = true;
            bool debug = false;

            AStar pathFinder = new AStar();
            yield return pathFinder.FindPath(map, start, end, null, searchDiagonal, debug);

            Assert.That(pathFinder.result != null, Is.True, "Path failed to generate.");

            var path = pathFinder.result;
            map.SetElements(path, 3);
            Texture2D mapTex;
            mapTex = map.ToTexture(WriteColor);
            yield return WriteToFile(mapTex, testOutputPath + "1000by1000BasicTestOutputPath.png");

            Assert.That(File.Exists(testOutputPath + "1000by1000BasicTestOutputPath.png"), Is.True, "Failed to write test output to file.");

            yield break;
        }

        //TODO: move into rng test set
        [Test]
        [TestCase(100)] //expect fail
        [TestCase(1000)]//expect fail
        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(1000000)]
        [TestCase(10000000)]
        public void QuickCoinTossTest(int trials)
        {
            RNG rng = new RNG();

            int[] counts = new int[] { 0, 0 };

            for(int i = 0; i < trials; ++i)
            {
                counts[GameRNG.CoinToss() ? 1 : 0]++;
            }

            double oddsZero = (double)counts[0] / (double)trials;
            double oddsOne = (double)counts[1] / (double)trials;

            double errorZero = Math.Abs(.5 - oddsZero);
            double errorOne = Math.Abs(.5 - oddsOne);

            double allowedError = 0.01;

            Assert.That(errorZero, Is.AtMost(allowedError), "Error of RNG CoinFlip for zero was too high. "+counts[0]+ " zeros and " + counts[1] + " ones ");
            Assert.That(errorOne, Is.AtMost(allowedError), "Error of RNG CoinFlip for one was too high");
        }
    }
#endif
}