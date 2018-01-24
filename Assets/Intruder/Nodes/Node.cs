﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Intruder
{
    public abstract class Node
    {
        public abstract Signal Process( Signal signal );

        public Node ShowMarker( MonoBehaviour client )
        {
            _marker.Operate( client , Marker.Operation.Show );
            return this;
        }

        public Node HideMarker( MonoBehaviour client )
        {
            _marker.Operate( client , Marker.Operation.Hide );
            return this;
        }

        public Node AddConnection( Connection connection )
        {
            connections.Add( connection );
            return this;
        }

        public Node RemoveConnection ( Connection connection )
        {
            connections.Remove( connection );
            return this;
        }

        private List<Connection> connections { get; set; }

        public Node Destroy()
        {
            while ( connections.Count > 0 )
                connections[ 0 ].Destroy();

            if ( _body != null )
                GameObject.Destroy( _body );

            _body = null;
            return this;
        }

        public virtual Node RouteSignal( MonoBehaviour client , Signal signal )
        {
            List<int> indices = new List<int>();

            for ( int i = 0 ; network.connections.Count > i ; i++ )
                if ( network.connections[ i ].connected && network.connections[ i ].master == this )
                    indices.Add( i );

            if ( indices.Count > 0 )
            {
                int index = indices.IndexOf( _index );
                index = 0 > index || index + 1 >= indices.Count ? 0 : index + 1;
                _index = indices[ index ];

                network.connections[ _index ].Signal( client , signal );
            }
            else
                _index = -1;

            return this;
        }

        public Vector3 position
        {
            get
            {
                return _body.transform.position;
            }
            set
            {
                _body.transform.position = value;

                 for ( int i = 0 ; network.connections.Count > i ; i++ )
                    network.connections[ i ].UpdatePositions();
            }
        }

        private void ConnectNodes( QuickButton button , Node node )
        {
            if ( node != null )
                button.StartCoroutine( ConnectNodesHandler( button , node ) );
        }

        private IEnumerator ConnectNodesHandler( QuickButton button , Node start )
        {
            bool holding = true;

            while ( holding )
            {
                holding = Input.GetMouseButton( 0 );

                if ( !holding )
                {
                    Ray ray = Camera.main.ScreenPointToRay( new Vector3( Input.mousePosition.x , Input.mousePosition.y , Camera.main.transform.position.y ) );
                    RaycastHit[] hits = Physics.RaycastAll( ray );
                    Node end = null;

                    for ( int i = 0 ; hits.Length > i && end == null ; i++ )
                    {
                        end = end != this ? network.GetNode( hits[ i ].transform.gameObject ) : null;

                        if ( end != null )
                        {
                            bool connected = false;

                            for ( int j = 0 ; connections.Count > j ; j++ )
                                if ( connections[ j ].Connected( start , end ) )
                                    connected = true;

                            if ( !connected )
                                new Connection( network , start , end ).ShowConnection( button );
                        }
                    }
                }

                yield return null;
            }
        }

        public bool IsNode ( GameObject gameObject )
        {
            return _cylinder == gameObject;
        }

        protected Network network { get; private set; }

        private int _index { get; set; }
        private Marker _marker { get; set; }
        private GameObject _body { get; set; }
        private GameObject _cylinder { get; set; }

        public Node( Network network , GameObject parent , int resolution = 50 )
        {
            this.network = network;

            _index = -1;
            _body = new GameObject( GetType().Name );

            _cylinder = GameObject.CreatePrimitive( PrimitiveType.Cylinder );
            _cylinder.AddComponent<QuickButton>().SetMouseDown( ( QuickButton button ) => ConnectNodes( button , this ) );
            _cylinder.transform.SetParent( _body.transform );

            if ( parent != null )
                _body.transform.SetParent( parent.transform );

            _body.transform.localScale = Vector3.one;
            _marker = new Marker( resolution , _body );
            connections = new List<Connection>();
        }

        public enum Operation
        {
            Show,
            Hide
        }
    }
}
