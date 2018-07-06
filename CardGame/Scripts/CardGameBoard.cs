using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace nv.Cards
{
    public class CardGameBoard : MonoBehaviour
    {
        public GameObject cardPlacementPrefab;

        public CardGameController gameController;

        //current board state

        Dictionary< int, Dictionary< int, CardPlacement > > board = new Dictionary<int, Dictionary<int, CardPlacement>>();

        List<CardPlacement> placements = new List<CardPlacement>();

        List<Card> cardsInBoard = new List<Card>();

        //pending board state

        List<CardPlacement> pendingPlacements = new List<CardPlacement>();

        List<Card> pendingCards = new List<Card>();

        public List<CardPlacement> GetOpenPlacements()
        {
            List<CardPlacement> open = new List<CardPlacement>();
            for( int i = 0; i < placements.Count; ++i )
            {
                if( placements[ i ].HasCard == false )
                    open.Add( placements[ i ] );
            }
            return open;
        }

        public void ResetGameBoard()
        {
            pendingPlacements.Clear();

            foreach( var row in board )
            {
                row.Value.Clear();
            }
            board.Clear();

            foreach( var c in placements )
                Destroy( c.gameObject );
            placements.Clear();

            foreach( var c in cardsInBoard )
                Destroy( c.gameObject );
            cardsInBoard.Clear();

            foreach( var c in pendingCards )
                Destroy( c.gameObject );
            pendingCards.Clear();

            CardPlacement start = AddPlacement( 0, 0 );
            //if( GameCamera.GetGameCamera( 0 ) != null )
            //    GameCamera.GetGameCamera( 0 ).AddObjectToTracking( start.gameObject );
        }

        public void UpdatePlacements()
        {
            for(int i = 0; i < placements.Count; ++i )
            {
                if( !placements[i].HasCard )
                {
                    bool result = CheckSizeRule(placements[i].posX,placements[i].posY);
                    if(!result)
                    {
                        placements[ i ].SetPlaceType(false);
                    }
                    else
                    {
                        //TODO: check against remaining deck to see if this placement is still valid
                    }
                }
            }
        }

        void AddPendingPlacement( CardPlacement newPlacement )
        {
            if( newPlacement != null && pendingPlacements.Contains(newPlacement) == false )
            { 
                pendingPlacements.Add( newPlacement );
            }
        }

        public void NotifyCancelPlacements()
        {
            for( int i = 0; i < pendingCards.Count; ++i )
            {
                pendingCards[ i ].boardPlacement.NotifyCancelPlacement();
            }

            for( int i = 0; i < pendingPlacements.Count; ++i )
            {
                RemovePlacement( pendingPlacements[ i ] );
            }

            pendingCards.Clear();
            pendingPlacements.Clear();

            UpdatePlacements();
        }

        public void NotifyPlacement( CardPlacement wherePlaced )
        {
            int x = wherePlaced.posX;
            int y = wherePlaced.posY;

            pendingCards.Add( wherePlaced.cardInside );

            AddPendingPlacement( AddPlacement( x - 1, y ) );
            AddPendingPlacement( AddPlacement( x, y - 1 ) );
            AddPendingPlacement( AddPlacement( x, y + 1 ) );
            AddPendingPlacement( AddPlacement( x + 1, y ) );

            UpdatePlacements();
        }

        public void NotifyCommitPlacements()
        {
            for( int i = 0; i < pendingCards.Count; ++i )
            {
                cardsInBoard.Add( pendingCards[i] );
                pendingCards[ i ].boardPlacement.NotifyCommitPlacement();
            }

            pendingCards.Clear();
            pendingPlacements.Clear();

            UpdatePlacements();
        }

        public bool RemovePlacement( CardPlacement c )
        {
            return RemovePlacement( c.posX, c.posY );
        }

        public bool RemovePlacement( int x, int y )
        {
            if( board.ContainsKey( x ) == true )
            {
                if( board[ x ].ContainsKey( y ) == true )
                {
                    GameObject placementObject = board[ x ][ y ].gameObject;
                    placements.Remove( board[ x ][ y ] );
                    bool result = board[ x ].Remove( y );
                    Destroy( placementObject );
                    return result;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public CardPlacement GetPlacement( int x, int y )
        {
            if( board.ContainsKey( x ) == true )
            {
                if( board[ x ].ContainsKey( y ) == true )
                {
                    return board[ x ][y];
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public bool HasPlacement(int x, int y)
        {
            if( board.ContainsKey( x ) == true )
            {
                if( board[ x ].ContainsKey( y ) == true )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public CardPlacement AddPlacement( int x, int y )
        {
            if( HasPlacement( x, y ) )
                return null;

            if( board.ContainsKey( x ) == false )
                board.Add( x, new Dictionary<int, CardPlacement>() );

            GameObject placement = (GameObject)Instantiate(cardPlacementPrefab,transform,false);
            CardPlacement cplace = placement.GetComponent<CardPlacement>();
            cplace.posX = x;
            cplace.posY = y;
            cplace.transform.localPosition = new Vector3(0f,y,-x);

            //add placement to board
            board[ x ][ y ] = cplace;

            //notify the game object it's now active in the board
            placement.SetActive( true );

            //keep track of it
            placements.Add(cplace);

            //notify the camera to keep track of this
            //GameCamera.GetGameCamera(0).AddObjectToTracking(placement);

            return cplace;
        }

        public bool CanPlaceHere(Card c, int x, int y)
        {
            if( board.ContainsKey( x ) == true )
            {
                if( board[x].ContainsKey( y ) == true && board[ x ][ y ].HasCard )
                {
                    return false;
                }
                else
                {
                    return CheckPlacementRule(c,x,y);
                }
            }
            return CheckPlacementRule( c, x, y );
        }

        bool CheckPlacementRule( Card c, int x, int y )
        {
            bool canPlace = CheckSizeRule(x,y);

            if(!canPlace)
                return false;

            canPlace = CheckCardGroupingRule(x,y);

            if( !canPlace )
                return false;

            List<Card> cardsInX = GetXCards(x,y);
            List<Card> cardsInY = GetYCards(x,y);

            //bool canPlaceX = CheckUniqueRule(c, cardsInX);
            //bool canPlaceY = CheckUniqueRule(c, cardsInY);

            //bool isUnique = canPlaceX && canPlaceY;

            bool canPlaceX = CheckGameRules( c, cardsInX );
            bool canPlaceY = CheckGameRules( c, cardsInY );

            bool passesAlikeRule = canPlaceX && canPlaceY;

            canPlace = passesAlikeRule;

            return canPlace;
        }

        bool CheckCardGroupingRule( int x, int y )
        {
            if( pendingCards.Count <= 0 )
                return true;

            bool resultX = true;
            bool resultY = true;

            for(int i = 0; i < pendingCards.Count; ++i )
            {
                if( pendingCards[ i ].boardPlacement.posX != x )
                    resultX = false;
                if( pendingCards[ i ].boardPlacement.posY != y )
                    resultY = false;
            }

            return resultX || resultY;
        }

        List<Card> GetXCards(int x, int y)
        {
            List<Card> cardsInX = new List<Card>();
            for( int i = 0; i < (int)Card.Element.Count; ++i )
            {
                int ix = x - (i+1);
                CardPlacement cplace = GetPlacement(ix,y);
                if( cplace == null || !cplace.HasCard )
                    break;
                cardsInX.Add( cplace.cardInside );
            }
            for( int i = 0; i < (int)Card.Element.Count; ++i )
            {
                int ix = x + (i+1);
                CardPlacement cplace = GetPlacement(ix,y);
                if( cplace == null || !cplace.HasCard )
                    break;
                cardsInX.Add( cplace.cardInside );
            }
            return cardsInX;
        }

        List<Card> GetYCards( int x, int y )
        {
            List<Card> cardsInY = new List<Card>();

            for( int i = 0; i < (int)Card.Element.Count; ++i )
            {
                int iy = y - (i+1);
                CardPlacement cplace = GetPlacement(x,iy);
                if( cplace == null || !cplace.HasCard )
                    break;
                cardsInY.Add( cplace.cardInside );
            }
            for( int i = 0; i < (int)Card.Element.Count; ++i )
            {
                int iy = y + (i+1);
                CardPlacement cplace = GetPlacement(x,iy);
                if( cplace == null || !cplace.HasCard )
                    break;
                cardsInY.Add( cplace.cardInside );
            }

            return cardsInY;
        }

        bool CheckSizeRule( int x, int y )
        {
            List<Card> cardsInX = new List<Card>();
            List<Card> cardsInY = new List<Card>();
            for(int i = 0; i < (int)Card.Element.Count; ++i)
            {
                int ix = x - (i+1);
                CardPlacement cplace = GetPlacement(ix,y);
                if( cplace == null || !cplace.HasCard )
                    break;
                cardsInX.Add(cplace.cardInside);
            }
            for( int i = 0; i < (int)Card.Element.Count; ++i )
            {
                int ix = x + (i+1);
                CardPlacement cplace = GetPlacement(ix,y);
                if( cplace == null || !cplace.HasCard )
                    break;
                cardsInX.Add( cplace.cardInside );
            }
            if(cardsInX.Count >= (int)Card.Element.Count)
                return false;

            for( int i = 0; i < (int)Card.Element.Count; ++i )
            {
                int iy = y - (i+1);
                CardPlacement cplace = GetPlacement(x,iy);
                if( cplace == null || !cplace.HasCard )
                    break;
                cardsInY.Add( cplace.cardInside );
            }
            for( int i = 0; i < (int)Card.Element.Count; ++i )
            {
                int iy = y + (i+1);
                CardPlacement cplace = GetPlacement(x,iy);
                if( cplace == null || !cplace.HasCard )
                    break;
                cardsInY.Add( cplace.cardInside );
            }
            if( cardsInY.Count >= (int)Card.Element.Count )
                return false;
            return true;
        }

        bool CheckGameRules(Card c, List<Card> cards)
        {
            //2 cards are always "alike"
            if( cards.Count <= 1 )
                return true;

            bool[] features = new bool[(int)Card.Feature.Count];
            byte[] cfeats = c.GetByteFeatures();            

            for( int i = 0; i < features.Length; ++i )
                features[ i ] = true;

            byte[] feats = new byte[(int)Card.Feature.Count];
            byte[] tfeats = cards[0].GetByteFeatures();
            for( int j = 0; j < (int)Card.Feature.Count; ++j )
            {
                feats[ j ] |= tfeats[ j ];
            }

            for(int i = 1; i < cards.Count; ++i )
            {
                byte[] cardsfeats = cards[i].GetByteFeatures();
                for( int j = 0; j < (int)Card.Feature.Count; ++j )
                {
                    feats[j] &= cardsfeats[j];
                }
            }

            //Debug.Log( "---row: " + StrBitArray( feats ) );
            //Debug.Log( "card: " + StrBitArray( cfeats ) );

            for( int j = 0; j < feats.Length; ++j )
            {
                //Debug.Log( j + "]] alike?" );
                if(feats[j] != 0)
                {
                    if( (feats[j] & cfeats[j]) == 0 )
                    {
                        //Debug.Log( "fails alike rule" );
                        return false;
                    }
                } 
                else
                {
                    //Debug.Log( j + "]]---cfeats unique?: " + StrBitArray( cfeats ) );
                    for( int i = 0; i < cards.Count; ++i )
                    {
                        byte[] cardsfeats = cards[i].GetByteFeatures();
                        //Debug.Log( j + "]]---row unique?: " + StrBitArray( cardsfeats ) );
                        if( ( cardsfeats[ j ] & cfeats[ j ] ) != 0 )
                        {
                            //Debug.Log( "fails alike rule" );
                            return false;
                        }
                    }
                }
            }
            //Debug.Log( "passes alike rule" );

            return true;
        }

        static public string StrBitArray(byte[] bytes)
        {
            string str = "";
            for( int j = 0; j < bytes.Length; ++j )
                str += System.Convert.ToString( bytes[j], 2 ) + " ";
            return str;
        }

        //int GetNumberOfStates( bool[] check, bool state )
        //{
        //    int c = 0;
        //    for( int i = 0; i < check.Length; ++i )
        //    {
        //        if( check[ i ] == state )
        //            c++;
        //    }
        //    return c;
        //}
    }
}
