﻿using UnityEngine;
using System.Collections.Generic;
using System;

using Meisui.Random;

namespace nv
{
    ///rng class wrapped around the mersene twister algorithm
    public class GameRNG : GameSingleton<GameRNG>
    {
        [SerializeField]
        RNG rng;

        public bool useCurrentSeedOnAwake;

        void Awake()
        {
            if(useCurrentSeedOnAwake)
                rng.Reset(rng.Seed);
            else
                rng.Reset();
        }

        public static new GameRNG Instance {
            get {
                GameRNG gameRNG = GameSingleton<GameRNG>.Instance;

                if( gameRNG.rng == null )
                    gameRNG.rng = new RNG();

                return gameRNG;
            }
        }

        public static int Seed {
            get {
                return Instance.rng.Seed;
            }
            set {
                Instance.rng.Seed = value;
            }
        }

        public static RNG Generator {
            get {
                return Instance.rng;
            }
        }

        public static uint Rand()
        {
            return Instance.rng.Rand();
        }

        public static uint Rand( uint r )
        {
            return Instance.rng.Rand(r);
        }

        //Generates a [0-N] int
        public static int Rand( int r )
        {
            return Instance.rng.Rand( r );
        }

        public static int Randi()
        {
            return Instance.rng.Randi();
        }

        public static long Randl()
        {
            return Instance.rng.Randl();
        }

        //Generates a [0-1] float
        public static float Randf()
        {
            return Instance.rng.Randf();
        }

        //Generates a [0-N] float
        public static float Rand( float r )
        {
            return Instance.rng.Rand(r);
        }

        public double Randd()
        {
            return Instance.rng.Randd();
        }

        //Generates a [0-N] double
        public static double Rand( double a )
        {
            return Instance.rng.Rand(a);
        }

        public static uint Rand( uint a, uint b )
        {
            return Instance.rng.Rand(a,b);
        }

        public static int Rand( int a, int b )
        {
            return Instance.rng.Rand( a, b );
        }

        public static float Rand( float a, float b )
        {
            return Instance.rng.Rand( a, b );
        }

        public static double Rand( double a, double b )
        {
            return Instance.rng.Rand( a, b );
        }

        // rolling min or max will be rare, but rolling exactly between the two will be common
        public static double GaussianRandom( double min, double max )
        {
            return Instance.rng.GaussianRandom( min, max );
        }

        //random point from [ [0f,0f] , [a.x,a.y] ]
        public static Vector2 Rand( Vector2 a )
        {
            return Instance.rng.Rand( a );
        }

        //random point in an area
        public static Vector2 Rand( Vector2 a, Vector2 b )
        {
            return Instance.rng.Rand( a, b );
        }

        //random point in an area
        public static Vector2 Rand( Rect a )
        {
            return Instance.rng.Rand( a );
        }

        // random vector
        //from [ [min.x,min.y] , [max.x,max.y] ]
        public static Vector3 RandVec3( float min, float max )
        {
            return Instance.rng.RandVec3( min, max );
        }

        // random normalized vector
        //from [ [min.x,min.y] , [max.x,max.y] ]
        public static Vector3 RandVec3Normalized( float min, float max )
        {
            return Instance.rng.RandVec3Normalized( min, max );
        }

        //Returns true if generated value [0-value] is less than limit
        public static bool RandomLowerThanLimit( int limit, int value )
        {
            return Instance.rng.RandomLowerThanLimit( limit, value );
        }

        //50/50 RNG
        public static bool CoinToss()
        {
            return Instance.rng.CoinToss();
        }

        public static int WeightedRand(AnimationCurve distribution, int min, int max)
        {
            return Instance.rng.WeightedRand(distribution, min, max);
        }

        //uses a given distribution and a range to weight the outcomes
        //returns the selected random index
        public static int WeightedRand(List<float> distribution)
        {
            return Instance.rng.WeightedRand(distribution);
        }

        public static T RandomElement<T>(List<T> elements)
        {
            return Instance.rng.RandomElement(elements);
        }

        public static void RandomShuffle<T>(List<T> elements)
        {
            Instance.rng.RandomShuffle(elements);
        }

        public static void Shuffle2D<T>(ref T[][] data)
        {
            Instance.rng.Shuffle2D(ref data);
        }

        /// <summary>
        /// Gets a random angle in radians from 0 to 2PI
        /// </summary>
        public static double RandomAngled()
        {
            return Instance.rng.RandomAngled();
        }

        public static double RandomAngled(float maxTheta)
        {
            return Instance.rng.RandomAngled(maxTheta);
        }

        public static double RandomAngled(float minTheta, float maxTheta)
        {
            return Instance.rng.RandomAngled(minTheta, maxTheta);
        }

        /// <summary>
        /// Gets a random angle in radians from 0 to 2PI
        /// </summary>
        public static float RandomAngle()
        {
            return Instance.rng.RandomAngle();
        }

        public static float RandomAngle(float maxTheta)
        {
            return Instance.rng.RandomAngle(maxTheta);
        }

        public static double RandomAngle(float minTheta, float maxTheta)
        {
            return Instance.rng.RandomAngle(minTheta, maxTheta);
        }

        public static Vector2 RandomPointOnCircle(float r)
        {
            return Instance.rng.RandomPointOnCircle(r);
        }

        public static Vector2 RandomPointOnCircle(Vector2 size)
        {
            return Instance.rng.RandomPointOnCircle(size);
        }
    }
}
