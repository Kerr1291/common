using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace nv
{
    public class ConstantLengthTransition : TimedTransition
    {
        [SerializeField]
        protected float transitionLength;

        public override float TotalTime
        {
            get
            {
                return transitionLength;
            }

            set
            {
                transitionLength = value;
            }
        }
    }
}