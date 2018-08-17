using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace nv.Cards
{
    public class CardGameController : MonoBehaviour
    {
        byte[][] cardDeck;
        
        [SerializeField]
        int nextCard = 0;

        [Header("Set to 0 to randomize; Use to control game card draw order")]
        [Tooltip("Set to 0 to randomize; Use to control game card draw order")]
        public int gameSeed;

        [Header("If true, RNG seeds will progressively shuffle the deck each game")]
        [Tooltip("If true, RNG seeds will progressively shuffle the deck each game")]
        public bool sequenceGenerateRNG = false;

        public GameObject baseCard;

        public List<CardPlayer> players = new List<CardPlayer>();

        public CardGameBoard gameBoard;

        public int TotalCards
        {
            get
            {
                return cardDeck.Length;
            }
        }

        public int RemainingCards
        {
            get
            {
                return cardDeck.Length - nextCard;
            }
        }

        static public CardGameController Instance
        {
            get; private set;
        }

        void Reset()
        {
            Instance = this;
        }

        void Awake()
        {
            GenerateDeck();
            Instance = this;
        }

        public void GenerateDeck()
        {
            int combinations = Card.Combinations;

            cardDeck = new byte[ combinations ][];

            int rankA = (int)Mathf.Pow((int)Card.Element.Count, 0f);
            int rankB = (int)Mathf.Pow((int)Card.Element.Count, 1f);
            int rankC = (int)Mathf.Pow((int)Card.Element.Count, 2f);

            for( int i = 0; i < combinations; ++i )
            {
                cardDeck[ i ] = new byte[ (byte)Card.Feature.Count ];
                cardDeck[ i ][ 0 ] = (byte)( 1 << ( ( i / rankA ) % (int)Card.Element.Count ) );
                cardDeck[ i ][ 1 ] = (byte)( 1 << ( ( i / rankB ) % (int)Card.Element.Count ) );
                cardDeck[ i ][ 2 ] = (byte)( 1 << ( ( i / rankC ) % (int)Card.Element.Count ) );

                //Debug.Log( CardGameBoard.StrBitArray( cardDeck[ i ] ) );
            }
        }

        public void ResetDraw()
        {
            nextCard = 0;
        }

        public void ShuffleDeck()
        {
            if(gameSeed != 0)
                GameRNG.Seed = gameSeed;
            else
                GameRNG.Generator.Reset();

            GameRNG.Shuffle2D( ref cardDeck );
        }

        public Card DrawNextCard()
        {
            if( nextCard >= cardDeck.Length )
                return null;

            return DrawCard( nextCard++ );
        }

        protected Card DrawCard( int index )
        {
            GameObject card = (GameObject)Instantiate(baseCard,transform,false);
            card.gameObject.SetActive( true );
            Card c = card.GetComponentInChildren<Card>();

            c.SetFeatureElements( cardDeck[ index ] );
            CardRenderer.Instance.GenerateCard( c );

            return c;
        }

        public void AddPlayer( CardPlayer player )
        {
            if( players.Contains( player ) == true )
                return;

            if( player == null )
                return;

            //don't add players during a game
            if( nextCard > 0 )
                return;

            players.Add( player );
            player.NotifyEndTurn();
        }

        public void StartGame()
        {
            ResetDraw();

            if( !sequenceGenerateRNG )
                GenerateDeck();

            ShuffleDeck();

            foreach( var player in players )
                player.ResetPlayer();

            gameBoard.ResetGameBoard();

            foreach( var player in players )
            {
                player.FillHand();
                player.NotifyEndTurn();
            }

            //TODO: select a first player at random
            players[ GameRNG.Randi()%players.Count ].NotifyStartTurn();
        }

        public void CancelPlay( CardPlayer actor )
        {
            gameBoard.NotifyCancelPlacements();
        }

        public void CommitPlay( CardPlayer actor )
        {
            gameBoard.NotifyCommitPlacements();
            actor.FillHand();
            actor.NotifyEndTurn();

            if( RemainingCards <= 0 )
            {
                EndGame();
            }
            else
            {
                NextGameTurn( actor );
            }
        }

        public void NotifyPlayerQuit( CardPlayer actor )
        {
        }

        void NextGameTurn( CardPlayer actor )
        {
            CardPlayer next = GetNextPlayer( actor );
            next.NotifyStartTurn();
        }

        CardPlayer GetNextPlayer( CardPlayer actor )
        {
            int i = 0;
            for( ; i < players.Count; ++i )
            {
                if( players[i] == actor )
                    break;
            }

            if( i == players.Count )
                return null;

            return players[ ( i + 1 ) % players.Count ];
        }

        void EndGame()
        {
            StartCoroutine( RunEndGame() );
        }

        IEnumerator RunEndGame()
        {
            yield return new WaitForSeconds( 2f );
            StartGame();
        }
    }
}
