using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Intruder
{
    public class Plus : Node
    {
        public override Signal Process ( Signal signal )
        {
            return signal.Plus();
        }

        public Plus( Network network , GameObject gameObject , int resolution = 50 ) : base( network , gameObject , resolution ) { }
    }
}