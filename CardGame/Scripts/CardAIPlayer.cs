using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace nv.Cards
{
    public class CardAIPlayer : CardPlayer
    {
        public override void NotifyEndTurn()
        {
            base.NotifyEndTurn();
        }

        public override void NotifyStartTurn()
        {
            base.NotifyStartTurn();
            TryPlaceCards();
        }

        public void HideCards()
        {
            for(int i = 0; i < hand.HandSize; ++i )
            {
                if( hand.currentHand[i] != null )
                {
                    hand.currentHand[ i ].transform.localPosition = ( hand as CardVirtualHand ).hidePosition;
                }
            }
        }

        public void TryPlaceCards()
        {
            //Debug.Log( "Trying to place cards." );
            List<CardPlacement> spots = gameBoard.GetOpenPlacements();

            int xmin = int.MaxValue;
            int ymin = int.MaxValue;

            int xmax = int.MinValue;
            int ymax = int.MinValue;

            for( int j = 0; j < spots.Count; ++j )
            {
                if( spots[ j ].posX > xmax )
                    xmax = spots[ j ].posX;

                if( spots[ j ].posY > ymax )
                    ymax = spots[ j ].posY;

                if( spots[ j ].posX < xmin )
                    xmin = spots[ j ].posX;

                if( spots[ j ].posY < ymin )
                    ymin = spots[ j ].posY;
            }

            bool result = false;
            result = DoTryPlace( xmin, xmax, ymin, ymax, 4 );
            if( result )
            {
                //Debug.Log( "Placed 4 cards." );
                CommitPlay();
                return;
            }
            else
            {
                CancelPlay();
            }
            result = DoTryPlace( xmin, xmax, ymin, ymax, 3 );
            if( result )
            {
                //Debug.Log( "Placed 3 cards." );
                CommitPlay();
                return;
            }
            else
            {
                CancelPlay();
            }
            result = DoTryPlace( xmin, xmax, ymin, ymax, 2 );
            if( result )
            {
                //Debug.Log( "Placed 2 cards." );
                CommitPlay();
                return;
            }
            else
            {
                CancelPlay();
            }
            result = DoTryPlace( xmin, xmax, ymin, ymax, 1 );
            if( result )
            {
                //Debug.Log( "Placed 1 card." );
                CommitPlay();
                return;
            }
            else
            {
                //no play, end turn
                gameController.CommitPlay( this );
            }
            Debug.Log( "Failed to place any cards." );
        }

        bool DoTryPlace(int xmin, int xmax, int ymin, int ymax, int size)
        {
            usedCards.Clear();
            List<CardPlacement> spots = new List<CardPlacement>();
            bool result = false;

            bool xFirst = GameRNG.Rand()%2 == 1;

            if( xFirst )
            {
                for( int y = ymin; y <= ymax; ++y )
                {
                    for( int x = xmin; x <= xmax; ++x )
                    {
                        result = TryFillX( x, y, size );
                        if( result )
                            return true;
                        usedCards.Clear();
                        CancelPlay();
                        HideCards();
                    }
                }

                for( int x = xmin; x <= xmax; ++x )
                {
                    for( int y = ymin; y <= ymax; ++y )
                    {
                        result = TryFillY( x, y, size );
                        if( result )
                            return true;
                        usedCards.Clear();
                        CancelPlay();
                        HideCards();
                    }
                }
            }
            else
            {
                //y direction first
                for( int x = xmin; x <= xmax; ++x )
                {
                    for( int y = ymin; y <= ymax; ++y )
                    {
                        result = TryFillY( x, y, size );
                        if( result )
                            return true;
                        usedCards.Clear();
                        gameController.CancelPlay( this );
                    }
                }
                for( int y = ymin; y <= ymax; ++y )
                {
                    for( int x = xmin; x <= xmax; ++x )
                    {
                        result = TryFillX( x, y, size );
                        if( result )
                            return true;
                        usedCards.Clear();
                        gameController.CancelPlay( this );
                    }
                }
            }


            return result;
        }

        List<int> usedCards = new List<int>();

        bool TryFillX(int x, int y, int size)
        {
            CardPlacement cplace = gameBoard.GetPlacement(x,y);
            if( cplace == null )
                return false;

            if( cplace.HasCard )
                return false;

            if( hand.CardsInHand < size )
                return false;

            int handIndex = CanPlaceACard( x, y );
            if( handIndex >= 0 )
            {
                if( size == 1 )
                    return true;
                bool result = TryFillX(x-1,y,size-1);
                if( !result )
                    result = TryFillX( x + 1, y, size - 1 );
                return result;
            }
            return false;
        }

        bool TryFillY( int x, int y, int size )
        {
            CardPlacement cplace = gameBoard.GetPlacement(x,y);
            if( cplace == null )
                return false;

            if( cplace.HasCard )
                return false;

            if( hand.CardsInHand < size )
                return false;

            int handIndex = CanPlaceACard( x, y );
            if( handIndex >= 0 )
            {
                if( size == 1 )
                    return true;
                bool result = TryFillY(x,y-1,size-1);
                if( !result )
                    result = TryFillY( x, y+1, size - 1 );
                return result;
            }
            return false;
        }

        int CanPlaceACard(int x, int y)
        {
            CardPlacement cplace = gameBoard.GetPlacement(x,y);
            if( cplace == null )
                return -1;

            if( cplace.HasCard )
                return -1;

            for( int i = 0; i < hand.HandSize; ++i )
            {
                if( usedCards.Contains( i ) )
                    continue;

                bool result = cplace.TryPlaceCardHere(hand.currentHand[i]);
                if( result )
                {
                    usedCards.Add( i );
                    return i;
                }
            }
            return -1;
        }
    }
}
