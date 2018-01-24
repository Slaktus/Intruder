using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Intruder
{
    public class Home : Node
    {
        public void Signal ( MonoBehaviour client )
        {
            if ( Time.time > _time + _interval )
            {
                _time = Time.time;
                RouteSignal( client , new Signal( this , 1 ) );
            }
        }

        public override Signal Process( Signal signal )
        {
            if ( signal.from != this )
                signal.Halt();

            return signal;
        }

        private float _time { get; set; }
        private float _interval { get; set; }

        public Home ( Network network , GameObject parent , int resolution = 50 ) : base ( network , parent , resolution )
        {
            _interval = 1;
        }
    }
}