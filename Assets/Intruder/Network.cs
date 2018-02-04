using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Intruder
{
    public class Network
    {
        public Network Add ( Connection connection )
        {
            connections.Add( connection );
            return this;
        }

        public Network Remove( Connection connection )
        {
            connections.Remove( connection );
            return this;
        }

        public Network Add ( Node node )
        {
            nodes.Add( node );
            return this;
        }

        public Network Remove ( Node node )
        {
            nodes.Remove( node );
            return this;
        }

        public Network Destroy()
        {
            while ( nodes.Count > 0 )
            {
                nodes[ nodes.Count - 1 ].Destroy();
                Remove( nodes[ nodes.Count - 1 ] );
            }

            GameObject.Destroy( cylinder );
            GameObject.Destroy( gameObject );
            return this;
        }

        public Network SetRadius( float radius )
        {
            cylinder.transform.localScale = new Vector3( radius * 2 , cylinder.transform.localScale.y , radius * 2 );
            this.radius = radius;
            return this;
        }

        public Network SetRotation( float rotation )
        {
            gameObject.transform.rotation = Quaternion.Euler( 0 , rotation , 0 );
            this.rotation = rotation;

            Debug.Log( connections.Count );

            for ( int i = 0 ; connections.Count > i ; i++ )
                connections[ i ].UpdatePositions();

            return this;
        }

        public Node GetNode( GameObject gameObject )
        {
            for ( int i = 0 ; nodes.Count > i ; i++ )
                if ( nodes[ i ].IsNode( gameObject ) )
                    return nodes[ i ];

            return null;
        }

        public float radius { get; private set; }
        public float rotation { get; private set; }
        public List<Node> nodes { get; private set; }
        public GameObject cylinder { get; private set; }
        public GameObject gameObject { get; private set; }
        public List<Connection> connections{ get; private set; }

        public Network( GameObject parent )
        {
            nodes = new List<Node>();
            connections = new List<Connection>();
            gameObject = new GameObject( "Network" );
            gameObject.transform.SetParent( parent.transform );
            cylinder = GameObject.CreatePrimitive( PrimitiveType.Cylinder );
            cylinder.transform.SetParent( gameObject.transform );
            gameObject.transform.localPosition = Vector3.zero;
            SetRadius( 0.5f );
        }
    }
}
