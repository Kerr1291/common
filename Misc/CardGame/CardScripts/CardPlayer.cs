using UnityEngine;
using System.Collections;

namespace CardGame
{
    public class CardPlayer : MonoBehaviour
    {
        public CardHand hand;

        public CardGameController gameController;

        public CardGameBoard gameBoard;

        public bool currentTurn = false;

        public void Awake()
        {
            gameController.AddPlayer( this );
        }

        public void CancelPlay()
        {
            gameController.CancelPlay( this );
        }

        public void CommitPlay()
        {
            gameController.CommitPlay( this );
        }

        public void Quit()
        {
            gameController.NotifyPlayerQuit( this );
            Application.Quit();
        }

        public void ResetPlayer()
        {
            hand.ResetHand();
        }

        public void FillHand()
        {
            hand.FillHand();
        }

        public virtual void NotifyEndTurn()
        {
            hand.ClearCurrentTurn();
            currentTurn = false;
        }

        public virtual void NotifyStartTurn()
        {
            hand.SetCurrentTurn();
            currentTurn = true;
        }
    }
}