using UnityEngine;
using System.Collections;

namespace CardGame
{
    public class CardPlacement : MonoBehaviour
    {
        public int posX;
        public int posY;

        public CardGameBoard board;

        public Card cardInside;

        public MeshRenderer placeRenderer;

        public Material goodPlace;
        public Material badPlace;

        public void SetPlaceType(bool good)
        {
            placeRenderer.material = ( good ? goodPlace : badPlace );
        }

        public bool HasCard
        {
            get
            {
                return cardInside != null;
            }
        }

        public bool TryPlaceCardHere( Card c )
        {
            if( c == null )
                return false;

            if( HasCard )
                return false;

            if( board.CanPlaceHere( c, posX, posY ) )
            {
                BindCardHere( c );

                PositionCardHere( c );

                board.NotifyPlacement( this );
                return true;
            }
            return false;
        }

        public void NotifyCommitPlacement()
        {
            cardInside.RemoveFromHand();
        }

        public void NotifyCancelPlacement()
        {
            ClearCardBinding();
        }

        public void OnDestroy()
        {
            if( GameCamera.GetGameCamera( 0 ) != null )
                GameCamera.GetGameCamera( 0 ).RemoveObjectFromTracking( gameObject );
        }

        public void BindCardHere( Card c )
        {
            if( c == null )
                return;

            cardInside = c;
            cardInside.boardPlacement = this;

            cardInside.ClearHeldStatus();
        }

        public void ClearCardBinding()
        {
            if( cardInside == null )
                return;

            cardInside.ReturnToHand();
        }

        public void PositionCardHere( Card c )
        {
            cardInside.transform.SetParent( transform );
            cardInside.transform.localPosition = Vector3.zero;
        }
    }
}
