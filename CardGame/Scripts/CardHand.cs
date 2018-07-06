using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace nv.Cards
{
    public class CardHand : MonoBehaviour
    {
        public float handSpacing = 2f;

        public Card[] currentHand;

        public Card heldCard;

        public CardGameController gameController;

        public Card GetRandomCard()
        {
            int card = GameRNG.Randi()%CardsInHand;
            int c = 0;
            for( int i = 0; i < currentHand.Length; ++i )
            {
                if( currentHand[ i ] == null )
                    continue;

                if( c == card )
                    return currentHand[ c ];
                c++;
            }
            return null;
        }

        public void SetCurrentTurn()
        {
            for(int i = 0; i < currentHand.Length; ++i )
            {
                if( currentHand[ i ] == null )
                    continue;
                currentHand[ i ].InputEnabled = true;
            }
        }

        public void ClearCurrentTurn()
        {
            for( int i = 0; i < currentHand.Length; ++i )
            {
                if( currentHand[ i ] == null )
                    continue;
                currentHand[ i ].InputEnabled = false;
            }
        }

        public int HandSize
        {
            get
            {
                return (int)Card.Element.Count;
            }
        }

        public int CardsInHand
        {
            get
            {
                int c = 0;
                for( int i = 0; i < HandSize; ++i )
                {
                    if( currentHand[ i ] == null )
                        continue;
                    c++;
                }
                return c;
            }
        }

        public void RemoveCardFromHeld( Card c )
        {
            for( int i = 0; i < currentHand.Length; ++i )
            {
                if( c == heldCard )
                {
                    heldCard = null;
                    return;
                }
            }
        }

        public void RemoveCardFromHand(Card c)
        {
            for(int i = 0; i < currentHand.Length; ++i )
            {
                if( c == heldCard )
                {
                    heldCard = null;
                }

                if( c == currentHand[i] )
                {
                    c.ownedHand = null;
                    currentHand[i] = null;
                    return;
                }
            }
        }

        public Card DrawCard()
        {
            Card c = gameController.DrawNextCard();

            if( c == null )
                return null;

            c.transform.SetParent(transform);
            currentHand[ GetHandPositionForCard( c ) ] = c;
            c.ownedHand = this;
            return c;
        }

        public virtual void FillHand()
        {
            float delay = 0f;
            while( CardsInHand < HandSize )
            {
                Card c = DrawCard();
                if( c == null )
                    break;
                c.transform.localPosition = -transform.forward * 1f + transform.right * 10.0f - transform.up * 3.2f;
                c.MoveToLocalPosition(c.handPosition, delay );
                delay += .6f;
            }
        }

        void Awake()
        {
            if( currentHand == null || currentHand.Length != HandSize )
                currentHand = new Card[HandSize];
        }

        void OnValidate()
        {
            if( currentHand == null || currentHand.Length != HandSize )
                currentHand = new Card[ HandSize ];
        }

        public void ResetHand()
        {
            foreach( var c in currentHand )
            {
                if( c != null )
                    Destroy( c.gameObject );
            }

            if( heldCard != null )
                Destroy( heldCard.gameObject );

            currentHand = new Card[ HandSize ];
            heldCard = null;
        }

        int GetHandPositionForCard(Card c)
        {
            for (int i = 0; i < HandSize; ++i)
            {
                if( currentHand[ i ] != null )
                    continue;

                float pos = -.5f * HandSize * handSpacing + i * handSpacing;
                c.handPosition = new Vector3(0f, 0f, pos);

                return i;
            }
            return -1;
        }
    }
}
