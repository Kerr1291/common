using UnityEngine;
using System.Collections;

namespace nv.Cards
{
    public class CardVirtualHand : CardHand
    {
        [SerializeField]
        public Vector3 hidePosition;

        void Awake()
        {
            if( currentHand == null || currentHand.Length != HandSize )
                currentHand = new Card[ HandSize ];
        }

        void OnValidate()
        {
            if( currentHand == null || currentHand.Length != HandSize )
                currentHand = new Card[ HandSize ];
        }

        public override void FillHand()
        {
            while( CardsInHand < HandSize )
            {
                Card c = DrawCard();
                if( c == null )
                    break;
                c.transform.localPosition = hidePosition;
            }
        }
    }
}
