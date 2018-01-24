using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Intruder
{
    public class Minus : Node
    {
        public override Signal Process( Signal signal )
        {
            return signal.Minus();
        }

        public Minus( Network network , GameObject gameObject , int resolution = 50 ) : base( network , gameObject , resolution ) { }
    }
}