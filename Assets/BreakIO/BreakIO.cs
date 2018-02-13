using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

namespace BreakIO
{
    public class BreakIO : MonoBehaviour
    {
        private void Awake()
        {
            new Game( 6 , 6 , 0.3f , this );
        }
    }

    public static class Draw
    {
        public static void Circle( Vector3 position , float radius , Color color = default( Color ) , float duration = 0 , int resolution = 20 )
        {
            float increment = 360 / resolution;
            Vector3[] points = new Vector3[ resolution ];

            for ( int i = 0 ; resolution > i ; i++ )
                points[ i ] = position + ( Quaternion.AngleAxis( increment * i , Vector3.forward ) * Vector3.up ) * radius;

            for ( int i = 0 ; resolution - 1 > i ; i++ )
                LineFromTo( points[ i ] , points[ i + 1 ] , color , duration );

            LineFromTo( points[ resolution - 1 ] , points[ 0 ] , color , duration );
        }

        public static void LineFromTo( Vector3 from , Vector3 to , Color color = default( Color ) , float duration = 0 )
        {
            Debug.DrawLine( from , to , color , duration );
        }

        public static void LineOffset( Vector3 from , Vector3 offset , Color color = default( Color ) , float duration = 0 )
        {
            Debug.DrawLine( from , from + offset , color , duration );
        }
    }

    public class Game
    {
        private IEnumerator Handler()
        {
            Func<IEnumerator> game = GameHandler;
            Func<IEnumerator> editor = EditorHandler;
            IEnumerator handler = default( IEnumerator );

            while ( true )
            {
                handler = this.editor ? editor() : game();

                while ( handler.MoveNext() )
                    yield return handler.Current;
            }
        }

        public IEnumerator GameHandler()
        {
            Mode mode = this.mode;

            while ( mode == this.mode )
            {
                level.GameUpdate();
                yield return null;
            }
        }

        public IEnumerator EditorHandler()
        {
            Mode mode = this.mode;

            while ( mode == this.mode )
            {
                level.EditorUpdate();
                yield return null;
            }
        }

        public Game SetMode ( Mode mode )
        {
            this.mode = mode;
            return this;
        }

        public Mode mode { get; private set; }
        public Level level { get; private set; }
        public bool editor { get { return mode == Mode.Editor; } }

        private MonoBehaviour client { get; set; }

        public Game ( int width , int height , float spacing , MonoBehaviour client )
        {
            mode = Mode.Editor;
            this.client = client;
            level = new Level( width , height , spacing , client );
            client.StartCoroutine( Handler() );
        }

        public enum Mode
        {
            Game,
            Editor
        }
    }

    public class Level
    {
        public void GameUpdate()
        {
        }

        public void EditorUpdate()
        {
            UpdateMousePosition();
            TrySwapTerminal();
            TryAddTerminal();
            TryAddConnection();
            TryRemoveNode();
            TryRemoveTerminal();
            TryRemoveConnection();
            TryAddNode();
            TryRemoveNetwork();
            TryAddNetwork();
            EditorDraw();
        }

        private void EditorDraw()
        {
            for ( int i = 0 ; grid.width * grid.height > i ; i++ )
                if ( NetworkAtIndex( i ) == null && !Overlap( this[ i ] , 0.125f ) )
                    Draw.Circle( this[ i ] , 0.125f , Color.blue );

            Draw.Circle( currentValidMousePosition , 0.01f , Color.red );

            if ( NetworkAtIndex( nearestIndex ) == null && 0.125f > Vector3.Distance( this[ nearestIndex ] , currentValidMousePosition ) && !Overlap( this[ nearestIndex ] , 0.125f ) )
                Draw.Circle( nearestGridPosition , 0.125f , Color.red );

            for ( int i = 0 ; networks.Count > i ; i++ )
                Draw.Circle( networks[ i ].position , networks[ i ].radius , networks[ i ].Contains( currentValidMousePosition ) ? Color.red : Color.yellow );

            for ( int i = 0 ; nodes.Count > i ; i++ )
                Draw.Circle( nodes[ i ].position , nodes[ i ].radius , nodes[ i ].Contains( currentValidMousePosition ) ? Color.red : nodes[ i ].terminal != null ? Color.green : Color.yellow );

            for ( int i = 0 ; connections.Count > i ; i++ )
                Draw.LineFromTo( connections[ i ].a.position , connections[ i ].b.position , Color.yellow );
        }

        private bool Overlap ( Vector3 positionA , float radiusA , Vector3 positionB , float radiusB )
        {
            return Mathf.Abs( radiusA + radiusB ) > Vector3.Distance( positionA , positionB );
        }

        private bool Overlap ( Vector3 position , float radius )
        {
            for ( int i = 0 ; networks.Count > i ; i++ )
                if ( networks[ i ].position != position && Overlap( position , radius , networks[ i ].position , networks[ i ].radius ) )
                    return true;

            return false;
        }

        private void UpdateMousePosition()
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector3 projectedMouse = new Vector3( mousePosition.x , mousePosition.y , Camera.main.transform.position.z );
            currentWorldMousePosition = Camera.main.ScreenToWorldPoint( new Vector3( projectedMouse.x , projectedMouse.y , -projectedMouse.z ) );
            RaycastHit[] hits = Physics.RaycastAll( Camera.main.ScreenPointToRay( projectedMouse ) );
            currentGridMousePosition = Vector3.zero;

            for ( int i = 0 ; hits.Length > i ; i++ )
                lastValidGridMousePosition = currentGridMousePosition = hits[ i ].point;
        }

        private void TryAddNode()
        {
            if ( Input.GetMouseButtonDown( 0 ) && hoverNetwork != null && hoverNode == null && !Overlap( hoverNetwork.position , hoverNetwork.Radius( hoverNetwork.nodes.Count + 1 ) ) )
                hoverNetwork.AddNode();
        }

        private void TryRemoveNode()
        {
            if ( Input.GetMouseButtonDown( 1 ) && hoverNode != null && hoverNode.terminal == null )
                hoverNetwork.RemoveNode( hoverNode );
        }

        private void TryAddNetwork()
        {
            if ( Input.GetMouseButtonDown( 0 ) && NetworkAtIndex( nearestIndex ) == null && !Overlap( this[ nearestIndex ] , 0.125f ) )
                AddNetwork( nearestIndex );
        }

        private void TryRemoveNetwork()
        {
            if ( Input.GetMouseButtonDown( 1 ) && NetworkAtIndex( nearestIndex ) != null && NetworkAtIndex( nearestIndex ).nodes.Count == 0 )
                RemoveNetwork( nearestIndex );
        }

        public Network AddNetwork( int index )
        {
            Network network = new Network( this , index );
            networks.Add( network );
            return network;
        }

        public Network RemoveNetwork ( int index )
        {
            Network network = NetworkAtIndex( index );

            if ( network != null )
                networks.Remove( network.Destroy() );

            return network;
        }

        public Node AddNode ( Network network , int index )
        {
            Node node = new Node( network );
            nodes.Add( node );
            return node;
        }

        public Node RemoveNode ( Node node )
        {
            nodes.Remove( node );
            return node;
        }

        public void TryAddTerminal()
        {
            if ( Input.GetMouseButtonDown( 0 ) && hoverNode != null && hoverNode.terminal == null )
                hoverNode.AddTerminal();
        }

        public void TrySwapTerminal()
        {
            if ( !addingConnection && Input.GetMouseButtonDown( 0 ) && hoverNode != null && hoverNode.terminal != null )
                hoverNode.terminal.SetMode( hoverNode.terminal.mode + 1 > Terminal.Mode.Count - 1 ? Terminal.Mode.Home : hoverNode.terminal.mode + 1 );
        }

        public void TryRemoveTerminal()
        {
            if ( !removingConnection && Input.GetMouseButtonDown( 1 ) && hoverNode != null && hoverNode.terminal != null )
            {
                hoverNode.RemoveTerminal();

                while ( hoverNode.connections.Count > 0 )
                    RemoveConnection( hoverNode.connections[ hoverNode.connections.Count - 1 ] );
            }
        }

        public void TryAddConnection()
        {
            if ( Input.GetMouseButtonDown( 0 ) && hoverNode != null && hoverNode.terminal != null )
                client.StartCoroutine( AddConnectionHandler( hoverNode ) );
        }

        public void TryRemoveConnection()
        {
            if ( Input.GetMouseButtonDown( 1 ) && hoverNode == null )
                client.StartCoroutine( RemoveConnectionHandler() );
        }

        public Connection AddConnection( Node a , Node b , Node master )
        {
            Connection connection = new Connection( a , b , this );
            connections.Add( connection );
            return connection;
        }

        public Connection RemoveConnection ( Connection connection )
        {
            connections.Remove( connection );
            connection.a.RemoveConnection( connection );
            connection.b.RemoveConnection( connection );
            return connection;
        }

        public Link AddLink ( Link link )
        {
            links.Add( link );
            return link;
        }

        public Link RemoveLink( Link link )
        {
            links.Remove( link );
            return link;
        }

        public Terminal AddTerminal ( Terminal terminal )
        {
            terminals.Add( terminal );
            return terminal;
        }

        public Terminal RemoveTerminal ( Terminal terminal )
        {
            terminals.Remove( terminal );
            return terminal;
        }

        private IEnumerator AddConnectionHandler( Node a )
        {
            Node b = null;
            addingConnection = true;

            while ( Input.GetMouseButton( 0 ) )
            {
                Draw.LineFromTo( a.position , currentWorldMousePosition , Color.red );
                b = nearestNode.Contains( currentWorldMousePosition ) ? nearestNode : b;
                yield return null;
            }

            if ( b != a && b != null && !HasConnection( a , b ) )
            {
                if ( b.terminal == null )
                    b.AddTerminal();

                Connection connection = AddConnection( a , b , a );
                b.AddConnection( connection );
                a.AddConnection( connection );
            }

            addingConnection = false;
        }

        private IEnumerator RemoveConnectionHandler()
        {
            removingConnection = true;
            Vector3 start = currentWorldMousePosition;

            while ( Input.GetMouseButton( 1 ) )
            {
                Draw.LineFromTo( start , currentWorldMousePosition , Color.red );
                yield return null;
            }

            Vector3 end = currentWorldMousePosition;
            int index = -1;

            for ( int i = 0 ; connections.Count > i && 0 > index ; i++ )
                if ( IntersectionPoint( start , end , connections[ i ].a.position , connections[ i ].b.position ) != Vector2.zero )
                    index = i;

            if ( index >= 0 )
                connections[ index ].Destroy();

            removingConnection = false;
        }

        public static Vector2 IntersectionPoint( Vector2 a1 , Vector2 a2 , Vector2 b1 , Vector2 b2 )
        {
            float d =
                ( b2.y - b1.y ) * ( a2.x - a1.x )
                -
                ( b2.x - b1.x ) * ( a2.y - a1.y );

            float a =
                ( b2.x - b1.x ) * ( a1.y - b1.y )
                -
                ( b2.y - b1.y ) * ( a1.x - b1.x );

            float b =
                ( a2.x - a1.x ) * ( a1.y - b1.y )
                -
                ( a2.y - a1.y ) * ( a1.x - b1.x );

            if ( d == 0 )
                return Vector3.zero;

            float ua = a / d;
            float ub = b / d;

            return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1
                ? new Vector2( a1.x + ( ua * ( a2.x - a1.x ) ) , a1.y + ( ua * ( a2.y - a1.y ) ) )
                : Vector2.zero;
        }

        public bool HasConnection ( Node a , Node b )
        {
            for ( int i = 0 ; connections.Count > i ; i++ )
                if ( connections[ i ].Has( a ) && connections[ i ].Has( b ) )
                    return true;

            return false;
        }

        public int NearestIndex ( Vector3 position )
        {
            int index = -1;
            float shortest = float.PositiveInfinity;

            for ( int i = 0 ; grid.width * grid.height > i ; i++ )
            {
                float distance = Vector3.Distance( position , this[ i ] );

                if ( shortest > distance )
                {
                    shortest = distance;
                    index = i;
                }
            }

            return index;
        }

        public Network NetworkAtIndex ( int index )
        {
            for ( int i = 0 ; networks.Count > i ; i++ )
                if ( networks[ i ].index == index )
                    return networks[ i ];

            return null;
        }

        public Node nearestNode
        {
            get
            {
                List<Node> nodes = new List<Node>( this.nodes.Count );

                for ( int i = 0 ; networks.Count > i ; i++ )
                {
                    Node nearest = networks[ i ].NearestNode( currentWorldMousePosition );

                    if ( nearest != null )
                        nodes.Add( nearest );
                }

                int index = -1;
                float shortest = float.PositiveInfinity;

                for ( int i = 0 ; nodes.Count > i ; i++ )
                {
                    float distance = Vector3.Distance( currentWorldMousePosition , nodes[ i ].position );

                    if ( shortest > distance )
                    {
                        index = i;
                        shortest = distance;
                    }
                }

                return index >= 0 ? nodes[ index ] : null;
            }
        }

        public Network nearestNetwork
        {
            get
            {
                int index = -1;
                float shortest = float.PositiveInfinity;

                for ( int i = 0 ; networks.Count > i ; i++ )
                {
                    float distance = Vector3.Distance( currentWorldMousePosition , networks[ i ].position );

                    if ( shortest > distance )
                    {
                        index = i;
                        shortest = distance;
                    }
                }

                return index >= 0 ? networks[ index ] : null;
            }
        }

        public Vector3 currentGridMousePosition { get; private set; }
        public Vector3 currentWorldMousePosition { get; private set; }
        public Vector3 lastValidGridMousePosition { get; private set; }
        public Vector3 nearestGridPosition { get { return this[ nearestIndex ]; } }
        public Vector3 this[ int index ] { get { return grid.GetPosition( index ); } }
        public Vector3 this[ int x , int y ] { get { return grid.GetPosition( x , y ); } }
        public Vector3 currentValidMousePosition { get { return currentGridMousePosition != Vector3.zero ? currentGridMousePosition : currentWorldMousePosition; } }
        public Network hoverNetwork { get { return nearestNetwork != null && nearestNetwork.Contains( currentValidMousePosition ) ? nearestNetwork : null; } }
        public Node hoverNode { get { return nearestNode != null && nearestNode.Contains( currentValidMousePosition ) ? nearestNode : null; } }
        public int nearestIndex { get { return NearestIndex( lastValidGridMousePosition ); } }

        public Grid grid { get; private set; }
        public List<Node> nodes { get; private set; }
        public List<Link> links { get; private set; }
        public List<Network> networks { get; private set; }
        public List<Terminal> terminals { get; private set; }
        public List<Connection> connections { get; private set; }

        
        private Mesh mesh { get; set; }
        private MonoBehaviour client { get; set; }
        private bool addingConnection { get; set; }
        private bool removingConnection { get; set; }
        private MeshCollider meshCollider { get; set; }

        public Level ( int width , int height , float spacing , MonoBehaviour client = null , float padding = -1 )
        {
            grid = new Grid( width , height , spacing );
            connections = new List<Connection>();
            terminals = new List<Terminal>();
            networks = new List<Network>();
            links = new List<Link>();
            nodes = new List<Node>();

            if ( 0 > padding )
                padding = spacing;

            mesh = new Mesh();
            Vector3 bottomLeft = this[ 0 ];
            Vector3 topRight = this[ ( width * height ) - 1 ];
            Vector3 topLeft = bottomLeft + ( Vector3.up * ( ( height - 1 ) * spacing ) );
            Vector3 bottomRight = topRight + ( Vector3.down * ( ( height - 1 ) * spacing ) );

            mesh.SetVertices( new List<Vector3>()
            {
                bottomLeft + ( ( Vector3.left + Vector3.down ) * spacing ) ,
                topLeft + ( ( Vector3.left + Vector3.up ) * spacing) ,
                topRight + ( ( Vector3.right + Vector3.up ) * spacing) ,
                bottomRight + ( ( Vector3.right + Vector3.down ) * spacing ) 
            } );

            mesh.SetNormals( new List<Vector3>() { Vector3.forward , Vector3.forward , Vector3.forward , Vector3.forward } );
            mesh.SetIndices( new int[] { 0 , 1 , 2 , 2 , 3 , 0 } , MeshTopology.Triangles , 0 );
            meshCollider = new GameObject( "MeshCollider" ).AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            if ( client != null )
            {
                this.client = client;
                meshCollider.transform.SetParent( client.transform );
            }
        }
    }

    public class Grid
    {
        public Vector3 GetPosition ( int x , int y )
        {
            return new Vector3( ( x * spacing ) - ( spacing * 0.5f ) , ( y * spacing ) - ( spacing * 0.5f ) );
        }

        public Vector3 GetPosition( int index )
        {
            int x = index % height;
            int y = ( index - x ) / width;
            return GetPosition( x , y );
        }

        public int width { get; private set; }
        public int height { get; private set; }
        public float spacing { get; private set; }

        public Grid ( int width , int height , float spacing )
        {
            this.width = width;
            this.height = height;
            this.spacing = spacing;
        }
    }

    public class Link
    {
        public Link SetSlave ( Node slave )
        {
            this.slave = slave;
            return this;
        }

        public Link SetMaster ( Node master )
        {
            this.master = master;
            return this;
        }

        public Link Destroy()
        {
            connection.level.RemoveLink( this );
            connection = null;
            master = null;
            slave = null;
            return this;
        }

        public Node slave { get; private set; }
        public Node master { get; private set; }
        public Connection connection { get; private set; }

        public Link ( Connection connection , Node master , Node slave )
        {
            this.connection = connection;
            this.master = master;
            this.slave = slave;

        }
    }

    public class Terminal
    {
        public Terminal SetMode ( Mode mode )
        {
            this.mode = mode;
            return this;
        }

        public Terminal AddLink ( Connection connection , Node master , Node slave )
        {
            links.Add( connection.level.AddLink( new Link( connection , master , slave ) ) );
            return this;
        }

        public Terminal RemoveLink ( Connection connection )
        {
            int index = -1;

            for ( int i = 0 ; links.Count > i && 0 > index ; i++ )
                if ( links[ i ].connection == connection )
                    index = i;

            if ( index >= 0 )
                links.Remove( links[ index ].Destroy() );

            return this;
        }

        public Terminal RemoveLink( Link link )
        {
            links.Remove( link.Destroy() );
            return this;
        }

        public Terminal Destroy()
        {
            while ( links.Count > 0 )
                RemoveLink( links[ links.Count - 1 ] );

            node.level.RemoveTerminal( this );
            links = null;
            node = null;
            return this;
        }

        public List<Link> links { get; private set; }
        public Node node { get; private set; }
        public Mode mode { get; private set; }

        public Terminal( Node node )
        {
            this.node = node;
            links = new List<Link>( node.connections.Count );
        }

        public enum Mode
        {
            None,
            Home,
            Plus,
            Minus,
            Multiply,
            Count
        }
    }

    public class Node
    {
        public Terminal AddTerminal()
        {
            if ( terminal == null )
                terminal = level.AddTerminal( new Terminal( this ) );

            return terminal;
        }

        public void RemoveTerminal()
        {
            terminal.Destroy();
            terminal = null;
        }

        public Connection AddConnection ( Connection connection )
        {
            network.AddConnection( connection );
            connections.Add( connection );
            return connection;
        }

        public Connection RemoveConnection ( Connection connection )
        {
            connections.Remove( connection );
            network.RemoveConnection( connection );

            if ( connections.Count == 0 && terminal != null )
                RemoveTerminal();

            return connection;
        }

        public bool Contains( Vector3 position )
        {
            return radius > Vector3.Distance( this.position , position );
        }

        public Node Destroy()
        {
            while ( connections.Count > 0 )
                connections[ connections.Count - 1 ].Destroy();

            level.RemoveNode( this );
            connections = null;
            network = null;
            return this;
        }

        public Vector3 position
        {
            get
            {
                float increment = 360 / Mathf.Max( 1 , ( network.nodes.Count - 1 ) );
                float angle = increment * ( network.nodes.Count == 2 && index == 0 ? 1.5f : index );

                if ( angle > 0 )
                    return network.position + ( Quaternion.AngleAxis( angle , Vector3.forward ) * Vector3.up ) * ( network.radius - ( radius * 1.5f ) );
                else
                    return network.position;
            }
        }

        public Network network { get; private set; }
        public Terminal terminal { get; private set; }
        public float radius { get { return 0.0625f; } }
        public Level level { get { return network.level; } }
        public List<Connection> connections { get; private set; }
        public int index { get { return network.nodes.IndexOf( this ); } }

        public Node ( Network network )
        {
            connections = new List<Connection>();
            this.network = network;
        }
    }

    public class Network
    {
        public bool Contains( Vector3 position )
        {
            return radius > Vector3.Distance( this.position , position );
        }

        public void AddNode()
        {
            if ( 9 > nodes.Count )
                nodes.Add( level.AddNode( this , nodes.Count + 1 ) );
        }

        public void RemoveNode()
        {
            if ( nodes.Count > 0 )
                nodes.Remove( nodes[ nodes.Count - 1 ].Destroy() );
        }

        public void RemoveNode( Node node )
        {
            if ( nodes.Count > 0 )
                nodes.Remove( node.Destroy() );
        }

        public Connection AddConnection ( Connection connection )
        {
            if ( !connections.Contains( connection ) )
                connections.Add( connection );

            return connection;
        }

        public Connection RemoveConnection ( Connection connection )
        {
            if ( connections.Contains( connection ) )
                connections.Remove( connection );

            return connection;
        }

        public Node NearestNode ( Vector3 position )
        {
            int index = -1;
            float shortest = float.PositiveInfinity;

            for ( int i = 0 ; nodes.Count > i ; i++ )
            {
                float distance = Vector3.Distance( position , nodes[ i ].position );

                if ( shortest > distance )
                {
                    shortest = distance;
                    index = i;
                }
            }

            return index >= 0 ? nodes[ index ] : null;
        }

        public Network Destroy()
        {
            while ( connections.Count > 0 )
                connections.Remove( connections[ connections.Count - 1 ].Destroy() );

            while ( nodes.Count > 0 )
                nodes.Remove( nodes[ nodes.Count - 1 ].Destroy() );

            connections = null;
            nodes = null;
            level = null;
            return this;
        }

        public float Radius( int count )
        {
            switch ( count )
            {
                default:
                    return 0.125f;

                case 2:
                    return 0.225f;

                case 3:
                case 4:
                case 5:
                    return 0.35f;

                case 6:
                case 7:
                    return 0.425f;

                case 8:
                case 9:
                    return 0.5f;
            }

        }

        public float radius { get { return Radius( nodes.Count ); } }

        public int index { get; private set; }
        public Vector3 position { get { return level[ index ]; } }
        public List<Connection> connections { get; private set; }
        public List<Node> nodes { get; private set; }
        public Level level { get; private set; }

        public Network ( Level level , int index )
        {
            this.index = index;
            this.level = level;
            nodes = new List<Node>( 9 );
            connections = new List<Connection>();
            nodes.Add( level.AddNode( this , 0 ) );
        }
    }

    public class Connection
    {
        public bool Has ( Node node )
        {
            return node == a || node == b;
        }

        public Connection Destroy()
        {
            level.RemoveConnection( this );
            a = null;
            b = null;
            return this;
        }

        public Node a { get; private set; }
        public Node b { get; private set; }
        public Level level { get; private set; }

        public Connection ( Node a , Node b , Level level )
        {
            this.a = a;
            this.b = b;
            this.level = level;
        }
    }
}