using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Intruder
{
    public class Connection
    {
        public Connection SetMaster( Node master )
        {
            this.master = master;
            return this;
        }

        public Connection ShowLink( MonoBehaviour client )
        {
            _link.Operate( client , Vector3.up * 0.1f , master , master == _a ? _b : _a , Link.Operation.Show );
            return this;
        }

        public Connection HideLink( MonoBehaviour client )
        {
            _link.Operate( client , Vector3.up * 0.1f , master , master == _a ? _b : _a , Link.Operation.Hide );

            return this;
        }

        public Connection ShowConnection( MonoBehaviour client , bool immediately = false )
        {
            _connection.Operate( client , Vector3.up * 0.1f , master , master == _a ? _b : _a , Link.Operation.Show );

            if ( immediately )
                _connection.SetLerpTime( 1 );

            return this;
        }

        public Connection HideConnection( MonoBehaviour client , bool immediately = false )
        {
            _connection.Operate( client , Vector3.up * 0.1f , master , master == _a ? _b : _a , Link.Operation.Hide );

            if ( immediately )
                _connection.SetLerpTime( 1 );

            return this;
        }

        public Connection UpdatePositions()
        {
            Vector3 direction = ( _b.position - _a.position ).normalized;
            float distance = Vector3.Distance( _a.position , _b.position );
            _body.transform.position = _a.position + ( direction * distance * 0.5f );
            _link.SetPositions( master , master == _a ? _b : _a , Vector3.up * 0.1f );
            _connection.SetPositions( master , master == _a ? _b : _a , Vector3.up * 0.1f );
            return this;
        }

        public Connection Signal( MonoBehaviour client , Signal signal )
        {
            bool start = _signalQueue.Count == 0;
            _signalQueue.Enqueue( signal );

            if ( start )
                client.StartCoroutine( SignalDispatcher( master == _a ? _b : _a , client ) );

            return this;
        }

        public bool Valid( List<Node> nodes )
        {
            return nodes.Contains( _a ) && nodes.Contains( _b );
        }

        public Connection Destroy()
        {
            //TODO: Connections need to destroy links!
            //Links stick around and they damn well shouldn't

            if ( _body != null )
                GameObject.Destroy( _body );

            _a.RemoveConnection( this );
            _b.RemoveConnection( this );
            _networkA.Remove( this );
            _networkB.Remove( this );
            _body = null;
            _a = null;
            _b = null;
            return this;
        }

        IEnumerator SignalDispatcher( Node slave , MonoBehaviour client )
        {
            while ( _signalQueue.Count > 0 )
            {
                Signal signal = master.Process( _signalQueue.Peek() );
                float delay = 1f / signal.strength;

                for ( int i = 0 ; signal.strength > i ; i++ )
                    client.StartCoroutine( SignalHandler( slave , client , delay * i ) );

                if ( signal.strength > 0 )
                {
                    float wait = 1;

                    while ( wait > 0 )
                        yield return wait -= Time.deltaTime;

                    signal.Route( master , slave , client );
                }

                _signalQueue.Dequeue();
            }
        }

        IEnumerator SignalHandler( Node slave , MonoBehaviour client , float delay )
        {
            while ( delay > 0 )
                yield return delay -= Time.deltaTime;

            GameObject cube = GameObject.CreatePrimitive( PrimitiveType.Cube );
            cube.transform.localScale = Vector3.one * 0.1f;
            cube.transform.SetParent( _body.transform );
            float t = 0;

            while ( 1 > t )
            {
                t += Time.deltaTime;
                cube.transform.position = Vector3.Lerp( master.position , slave.position , t );
                yield return null;
            }

            t = 1;
            cube.transform.position = Vector3.Lerp( master.position , slave.position , t );
            GameObject.Destroy( cube );
        }

        public bool Connected ( Node a , Node b )
        {
            return ( a == _a && b == _b ) || ( a == _b && b == _a );
        }

        public Node master { get; private set; }
        public bool showingLink { get { return _link.showing; } }
        public bool linked { get { return _link.lerpTime == 1; } }
        public bool connected { get { return _connection.lerpTime == 1; } }
        public bool showingConnection { get { return _connection.showing; } }

        private Node _a { get; set; }
        private Node _b { get; set; }
        private Link _link { get; set; }
        private Link _connection { get; set; }
        private GameObject _body { get; set; }
        private Network _networkA { get; set; }
        private Network _networkB { get; set; }
        private Queue<Signal> _signalQueue { get; set; }

        public Connection( Network networkA , Network networkB , Node a , Node b )
        {
            _networkA = networkA;
            networkA.Add( this );
            _networkB = networkB;
            networkB.Add( this );
            _body = new GameObject( "Connection" );
            Vector3 direction = ( b.position - a.position ).normalized;
            float distance = Vector3.Distance( a.position , b.position );
            _body.transform.position = a.position + ( direction * distance * 0.5f );

            SetMaster( a );
            _a = a.AddConnection( this );
            _b = b.AddConnection( this );
            _signalQueue = new Queue<Signal>();
            _link = new Link( _body , Color.black , 0.25f , 0.125f );
            _connection = new Link( _body , Color.black , 0.125f , 0.125f );
        }
    }
}