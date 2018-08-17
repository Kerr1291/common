using UnityEngine;
using System.Collections.Generic;

namespace nv.Cards
{
    [System.Serializable]
    public abstract class Feature
    {
        public abstract void ApplyFeature( Texture2D dest );
    }

    [System.Serializable]
    public class ColorFeature : Feature
    {
        public float alphaThreshold = .1f;
        public Color color;

        public override void ApplyFeature( Texture2D dest )
        {
            for(int j = 0; j < dest.height; ++j)
            {
                for( int i = 0; i < dest.width; ++i )
                {
                    Color c = dest.GetPixel(i, j);
                    if( c.a < alphaThreshold )
                        continue;
                    dest.SetPixel(i,j, color );
                }
            }
            dest.Apply();
        }
    }

    [System.Serializable]
    public class NumberFeature : Feature
    {
        public float alphaThreshold = .1f;
        public Texture2D number;

        public override void ApplyFeature( Texture2D dest )
        {
            for( int j = 0; j < dest.height; ++j )
            {
                for( int i = 0; i < dest.width; ++i )
                {
                    Color c = number.GetPixel(i, j);
                    if( c.a < alphaThreshold )
                        continue;
                    dest.SetPixel( i, j, c );
                }
            }
            dest.Apply();
        }
    }

    [System.Serializable]
    public class ShapeFeature : Feature
    {
        public float alphaThreshold = .1f;
        public Texture2D dotPattern;

        public override void ApplyFeature( Texture2D dest )
        {
            for( int j = 0; j < dest.height; ++j )
            {
                for( int i = 0; i < dest.width; ++i )
                {
                    Color c = dotPattern.GetPixel(i, j);
                    if( c.a < alphaThreshold )
                        continue;
                    dest.SetPixel( i, j, c );
                }
            }
            dest.Apply();
        }
    }

    public class CardRenderer : MonoBehaviour
    {
        static public CardRenderer Instance
        {
            get; private set;
        }

        public int textureSize = 128;

        public ColorFeature[] featuresA;
        public NumberFeature[] featuresB;
        public ShapeFeature[] featuresC;

        static void ValidateFeature<T>( ref T[] features )
        {
            if( features == null || features.Length != (int)Card.Element.Count )
            {
                T[] f = new T[ (int)Card.Element.Count ];
                if( features != null && f.Length >= features.Length )
                    features.CopyTo( f, 0 );
                features = f;
            }
        }

        void Validate()
        {
            ValidateFeature<ColorFeature>( ref featuresA );
            ValidateFeature<NumberFeature>( ref featuresB );
            ValidateFeature<ShapeFeature>( ref featuresC );

            textureSize = Mathf.ClosestPowerOfTwo( textureSize );
        }

        void Reset()
        {
            Instance = this;
        }

        void Awake()
        {
            Instance = this;
        }

        void OnValidate()
        {
            Validate();
        }

        public Texture2D baseTexture;

        public void SetFeatureBase(Texture2D dest)
        {
            for (int j = 0; j < dest.height; ++j)
            {
                for (int i = 0; i < dest.width; ++i)
                {
                    dest.SetPixel(i, j, baseTexture.GetPixel(i, j));
                }
            }
            dest.Apply();
        }

        public void SetFillColor( Texture2D dest, Color color )
        {
            float alphaThreshold = .1f;
            for( int j = 0; j < dest.height; ++j )
            {
                for( int i = 0; i < dest.width; ++i )
                {
                    Color c = dest.GetPixel(i, j);
                    if( c.a > alphaThreshold )
                        continue;
                    dest.SetPixel( i, j, color );
                }
            }
            dest.Apply();
        }

        public int GetFirstFlippedBitIndex( byte b )
        {
            for(int i = 0; i < 8; ++i )
            {
                bool r = ((1 << i) & b) == 0;
                if(!r)
                    return i;
            }
            return -1;
        }

        public void GenerateCard( Card dest )
        {
            dest.cardTexture = new Texture2D( textureSize, textureSize, TextureFormat.ARGB32, false, false );
            dest.cardMaterial = dest.meshRenderer.material;

            byte[] features = dest.GetByteFeatures();

            //Debug.Log( "genrating card: "+CardGameBoard.StrBitArray(features) );
            //Debug.Log( "f2 : " + GetFirstFlippedBitIndex( features[ 2 ] ) );
            //Debug.Log( "f0 : " + GetFirstFlippedBitIndex( features[ 0 ] ) );
            //Debug.Log( "f1 : " + GetFirstFlippedBitIndex( features[ 1 ] ) );

            int[] findex = new int[ features.Length ];

            findex[ 0 ] = GetFirstFlippedBitIndex( features[ 0 ] );
            findex[ 1 ] = GetFirstFlippedBitIndex( features[ 1 ] );
            findex[ 2 ] = GetFirstFlippedBitIndex( features[ 2 ] );

            SetFeatureBase( dest.cardTexture );

            featuresC[ findex[ 2 ] ].ApplyFeature( dest.cardTexture );
            featuresA[ findex[ 0 ] ].ApplyFeature( dest.cardTexture );
            featuresB[ findex[ 1 ] ].ApplyFeature( dest.cardTexture );

            SetFillColor( dest.cardTexture, Color.white );

            dest.cardMaterial.mainTexture = dest.cardTexture;
            dest.meshRenderer.material = dest.cardMaterial;
        }
    }
}
