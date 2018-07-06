using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CardGame
{
    public class Card : MonoBehaviour
    {
        public enum Feature
        {
            None = -1
            , TypeA = 0
            , TypeB
            , TypeC
            , Count
        };

        public enum Element
        {
            None = -1
            , TypeA = 0
            , TypeB
            , TypeC
            , TypeD
            , Count
        };

        static public int Combinations
        {
            get
            {
                return (int)Mathf.Pow((int)Card.Element.Count, (int)Card.Feature.Count);
            }
        }

        public GameObject root;

        public TextMesh debugText;

        public MeshRenderer meshRenderer;

        public Material cardMaterial;

        public Texture2D cardTexture;

        public Vector3 handPosition;

        public CardHand ownedHand;

        public CardPlacement boardPlacement;

        bool canInteract = true;

        [SerializeField]
        [HideInInspector]
        byte[] features;

        public bool InputEnabled {
            get {
                return canInteract;
            }
            set {
                canInteract = value;
            }
        }

        public bool IsHeldByHand {
            get {
                if( ownedHand == null )
                    return false;
                return ( ownedHand.heldCard == this );
            }
        }

        public void ClearHeldStatus()
        {
            if( ownedHand == null )
                return;
            ownedHand.RemoveCardFromHeld( this );
        }

        public void RemoveFromHand()
        {
            if( ownedHand == null )
                return;
            ownedHand.RemoveCardFromHand( this );
        }

        public void ReturnToHand()
        {
            if( ownedHand != null )
                transform.SetParent( ownedHand.transform );

            boardPlacement.cardInside = null;
            boardPlacement = null;
        }

        void Reset()
        {
            features = new byte[(int)Feature.Count];
        }

        void Awake()
        {
            features = new byte[(int)Feature.Count];
        }

        //public void SetFeatureElement(Feature feature, Element element)
        //{
        //    features[(int)feature] = (byte)(1 << (int)element);
        //}

        //public void SetFeatureElements(Element[] elements)
        //{
        //    for (int i = 0; i < elements.Length && i < (int)Feature.Count; ++i)
        //        features[i] = (byte)(1 << (int)elements[i]);
        //}

        //public void SetFeatureElements(int[] elements)
        //{
        //    for (int i = 0; i < elements.Length && i < (int)Feature.Count; ++i)
        //        features[i] = (byte)(1 << (int)elements[i]);
        //}

        public void SetFeatureElements(byte[] elements)
        {
            for (int i = 0; i < elements.Length && i < (int)Feature.Count; ++i)
                features[i] = (byte)(elements[i]);

            debugText.text = CardGameBoard.StrBitArray( elements );
        }

        //public bool HasMatchingFeature(Card other)
        //{
        //    if (other == null)
        //        return false;

        //    int result = 0;
        //    for (int i = 0; i < (int)Feature.Count; ++i)
        //    {
        //        result += features[i] & other.features[i];
        //    }
        //    return result > 0;
        //}

        //public bool[] GetMatchingFeatures(Card other)
        //{
        //    if (other == null)
        //        return null;

        //    //List<Feature> matchingFeatures = new List<Feature>();
        //    bool[] matchingFeatures = new bool[(int)Card.Feature.Count];
        //    for (int i = 0; i < (int)Feature.Count; ++i)
        //    {
        //        bool result = (features[i] & other.features[i]) != 0;
        //        matchingFeatures[i] = result;
        //        //if (result != 0)
        //        //    matchingFeatures.Add((Feature)i);
        //    }
        //    return matchingFeatures;
        //}

        public byte[] GetByteFeatures()
        {
            byte[] fs = new byte[(int)Card.Feature.Count];
            for( int i = 0; i < (int)Feature.Count; ++i )
                fs[i] = features[i];
            return fs;
        }

        //public Feature GetFeatureElement(Feature feature)
        //{
        //    return (Feature)features[(int)feature];
        //}

        //public int GetFeatureElement(int feature)
        //{
        //    return (int)features[(int)feature];
        //}

        public List<byte> GetFeatures()
        {
            List<byte> fs = new List<byte>();
            for (int i = 0; i < (int)Feature.Count; ++i)
            {
                fs.Add(features[i]);
            }
            return fs;
        }

        public void OnMouseDown()
        {
            if( IsMoving )
                return;

            if( !InputEnabled )
                return;

            TryPickupCard();
        }

        public void TryPickupCard()
        {
            if( ownedHand != null && ownedHand.heldCard == null )
            {
                ownedHand.heldCard = this;
                StartCoroutine( UpdateHeldPosition() );
            }
        }

        public void OnMouseUp()
        {
            if( IsMoving )
                return;

            TryPlaceCard();
        }

        public void TryPlaceCard()
        {
            var mousePos = Input.mousePosition;
            mousePos.z = GameCamera.GetGameCamera( 0 ).currentViewDistance - 1.1f; // select distance from the camera
            mousePos = GameCamera.GetCamera( 0 ).ScreenToWorldPoint( mousePos );

            Ray r = new Ray(mousePos, Vector3.right);

            RaycastHit[] hits;
            hits = Physics.RaycastAll( r, 1000f );

            for( int i = 0; i < hits.Length; ++i )
            {
                CardPlacement cplace = hits[i].collider.GetComponent<CardPlacement>();
                if( cplace != null )
                {
                    if( cplace.TryPlaceCardHere( this ) == false )
                        break;
                }
            }
            ClearHeldStatus();
        }

        IEnumerator UpdateHeldPosition()
        {
            while( ownedHand != null )
            {
                yield return new WaitForEndOfFrame();
                if( ownedHand == null )
                    break;

                if( ownedHand.heldCard == null || ownedHand.heldCard != this )
                {
                    if( boardPlacement == null )
                        transform.localPosition = handPosition;
                    continue;
                }

                var mousePos = Input.mousePosition;
                mousePos.z = GameCamera.GetGameCamera(0).currentViewDistance + 7f; // select distance from the camera
                transform.position = GameCamera.GetCamera(0).ScreenToWorldPoint(mousePos);
            }
        }

        public bool IsMoving
        {
            get
            {
                return moveRoutine != null;
            }
        }

        IEnumerator flipRoutine;

        public void CardFlip( float flipTime, float delay = 0f )
        {
            if( flipRoutine != null )
                StopCoroutine( flipRoutine );
            flipRoutine = FlipThisCard( flipTime, delay );
            StartCoroutine( flipRoutine );
        }

        IEnumerator FlipThisCard( float flipTime, float delay = 0f )
        {
            yield return new WaitForSeconds( delay );
            float slerpTime = 0f;
            float moveTime = flipTime * .5f;
            Quaternion start = transform.localRotation;
            while( slerpTime < moveTime )
            {
                slerpTime += Time.fixedDeltaTime;
                transform.localRotation = Quaternion.Slerp( start, Quaternion.AngleAxis( 180f,transform.forward ), slerpTime/moveTime );
                yield return new WaitForFixedUpdate();
            }
            start = transform.localRotation;
            slerpTime = 0f;
            while( slerpTime < moveTime )
            {
                slerpTime += Time.fixedDeltaTime;
                transform.localRotation = Quaternion.Slerp( start, Quaternion.AngleAxis( 360f, transform.forward ), slerpTime / moveTime );
                yield return new WaitForFixedUpdate();
            }
            flipRoutine = null;
        }

        IEnumerator moveRoutine;

        public void MoveToWorldPosition( Vector3 dest, float delay = 0f )
        {
            if( moveRoutine != null )
                StopCoroutine(moveRoutine);
            moveRoutine = MoveToDest(dest,false, delay );
            StartCoroutine( moveRoutine );
        }

        float transitionTime = .8f;

        public void MoveToLocalPosition( Vector3 dest, float delay = 0f )
        {
            CardFlip( transitionTime, delay);
            if( moveRoutine != null )
                StopCoroutine( moveRoutine );
            moveRoutine = MoveToDest( dest, true, delay );
            StartCoroutine( moveRoutine );
        }

        IEnumerator MoveToDest(Vector3 dest, bool local, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);
            float lerpTime = 0f;
            float moveTime = transitionTime;
            Vector3 start;
            if( local )
                start = transform.localPosition;
            else
                start = transform.position;
            while( lerpTime < moveTime )
            {
                lerpTime += Time.fixedDeltaTime;
                if( local )
                    transform.localPosition = Vector3.Lerp( start, dest, lerpTime/ moveTime );
                else
                    transform.position = Vector3.Lerp(start,dest,lerpTime/ moveTime );
                yield return new WaitForFixedUpdate();
            }
            moveRoutine = null;
        }
    }
}
