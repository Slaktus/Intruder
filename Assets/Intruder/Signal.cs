using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Intruder
{
    public class Signal
    {
        public Signal Plus()
        {
            strength += 1;
            return this;
        }

        public Signal Minus()
        {
            strength -= 1;
            return this;
        }

        public Signal Halt()
        {
            strength = 0;
            return this;
        }

        public Signal Route ( Node from , Node to , MonoBehaviour client )
        {
            this.from = from;
            route.Add( from );
            to.RouteSignal( client , this );
            return this;
        }

        public Node from { get; private set; }
        public int strength { get; private set; }

        private List<Node> route { get; set; }

        public Signal( Node from , int strength )
        {
            this.from = from;
            this.strength = strength;
            route = new List<Node>();
        }
    }
}
