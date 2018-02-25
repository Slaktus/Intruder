using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BreakIO
{
    public class BreakIO : MonoBehaviour
    {
        public Material lineMaterial;

        private void Awake()
        {
            Draw.SetMaterial( lineMaterial );
        }

        private void Start()
        {
            new Game( 6 , 6 , 0.3f , this );
        }
    }

    public static class Draw
    {
        public static void SetMaterial ( Material material )
        {
            _material = material;
        }

        public static bool Arrow( Vector3 position , Vector3 direction , float size )
        {
            Vector3 perpendicularA = new Vector3( -direction.y , direction.x );
            Vector3 perpendicularB = -perpendicularA;

            Vector3 top = position + ( direction * size * 0.5f );
            Vector3 bottomA = position + ( ( -direction + perpendicularA ).normalized * size * 0.5f );
            Vector3 bottomB = position + ( ( -direction + perpendicularB ).normalized * size * 0.5f );

            Line( top , bottomA , Color.green );
            Line( bottomB , top , Color.green );
            Line( bottomA , bottomB , Color.green );
            return true;
        }

        public static bool Circle( Vector3 position , float radius , Color color = default( Color ) , float duration = 0 , int resolution = 20 )
        {
            float increment = 360 / resolution;
            Vector3[] points = new Vector3[ resolution ];

            for ( int i = 0 ; resolution > i ; i++ )
                points[ i ] = position + ( Quaternion.AngleAxis( increment * i , Vector3.forward ) * Vector3.up ) * radius;

            for ( int i = 0 ; resolution - 1 > i ; i++ )
                Line( points[ i ] , points[ i + 1 ] , color , duration );

            Line( points[ resolution - 1 ] , points[ 0 ] , color , duration );
            return true;
        }

        public static void Line( Vector3 from , Vector3 to , Color color = default( Color ) , float duration = 0 )
        {
            Debug.DrawLine( from , to , color , duration );
        }

        public static void Terminal( Terminal terminal )
        {
            Vector3 position = terminal.node.position;

            switch ( terminal.mode )
            {
                case global::BreakIO.Terminal.Mode.Multiply:
                    Line( position + ( Vector3.left * 0.025f ) + ( Vector3.up * 0.025f ) , position + ( Vector3.right * 0.025f ) + ( Vector3.down * 0.025f ) , Color.green );
                    Line( position + ( Vector3.right * 0.025f ) + ( Vector3.up * 0.025f ) , position + ( Vector3.left * 0.025f ) + ( Vector3.down * 0.025f ) , Color.green );
                    break;

                case global::BreakIO.Terminal.Mode.Minus:
                    Line( position + ( Vector3.left * 0.025f ) , position + ( Vector3.right * 0.025f ) , Color.green );
                    break;

                case global::BreakIO.Terminal.Mode.Home:
                    Line( position + ( Vector3.left * 0.025f ) , position + ( Vector3.right * 0.025f ) , Color.green );
                    Line( position + ( Vector3.left * 0.025f ) + ( Vector3.up * 0.04f ) , position + ( Vector3.left * 0.025f ) + ( Vector3.down * 0.04f ) , Color.green );
                    Line( position + ( Vector3.right * 0.025f ) + ( Vector3.up * 0.04f ) , position + ( Vector3.right * 0.025f ) + ( Vector3.down * 0.04f ) , Color.green );
                    break;

                case global::BreakIO.Terminal.Mode.Plus:
                    Line( position + ( Vector3.left * 0.025f ) , position + ( Vector3.right * 0.025f ) , Color.green );
                    Line( position + ( Vector3.up * 0.025f ) , position + ( Vector3.down * 0.025f ) , Color.green );
                    break;
            }
        }

        public static bool Circle<T> ( T item , Color color = default( Color ) , int resolution = 20 ) where T : Common
        {
            bool registered = _items.Contains( item );

            if ( registered )
                Circle( _lineRenderers[ _items.IndexOf( item ) ] , item.position , item.radius , color , resolution );

            return registered;
        }

        public static bool LineRendererColor<T>( T item , Color color ) where T : Common
        {
            bool registered = _items.Contains( item );

            if ( registered )
                LineRendererColor( _lineRenderers[ _items.IndexOf( item ) ] , color );

            return registered;
        }

        public static bool LineRendererColor( LineRenderer lineRenderer , Color color )
        {
            lineRenderer.material.color = color;
            return true;
        }

        public static bool Circle ( LineRenderer lineRenderer , Vector3 position , float radius , Color color = default( Color ) , int resolution = 20 )
        {
            float increment = 360 / resolution;
            Vector3[] points = new Vector3[ resolution ];

            for ( int i = 0 ; resolution > i ; i++ )
                points[ i ] = position + ( Quaternion.AngleAxis( increment * i , Vector3.forward ) * Vector3.up ) * radius;

            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions( points );
            lineRenderer.material.color = color;
            return true;
        }

        public static void Register<T>( T item , LineRendererSettings settings ) where T : Common
        {
            _lineRenderers.Add( GetLineRenderer( ( item is Node ? "Node" : "Network" ) + ( "LineRenderer" + _items.Count ) , settings ) );
            _items.Add( item );
        }

        public static void Deregister<T>( T item ) where T : Common
        {
            LineRenderer lineRenderer = _lineRenderers[ _items.IndexOf( item ) ];

            _items.Remove( item );
            _lineRenderers.Remove( lineRenderer );
            GameObject.Destroy( lineRenderer.gameObject );
        }

        public static LineRenderer GetLineRenderer( string name , LineRendererSettings settings )
        {
            LineRenderer lineRenderer = settings.Apply( new GameObject( name ).AddComponent<LineRenderer>() );
            lineRenderer.transform.SetParent( _parent.transform );
            lineRenderer.material = new Material( _material );
            return lineRenderer;
        }

        private static Material _material { get; set; }
        private static GameObject _parent { get; set; }
        private static List<Common> _items { get; set; }
        private static List<LineRenderer> _lineRenderers { get; set; }

        static Draw()
        {
            _items = new List<Common>();
            _parent = new GameObject( "Draw" );
            _lineRenderers = new List<LineRenderer>();
        }
    }

    public class Game
    {
        private IEnumerator Handler()
        {
            while ( true )
                yield return level.EditorUpdate( _client );
        }

        public Level level { get; private set; }

        private MonoBehaviour _client { get; set; }

        public Game ( int width , int height , float spacing , MonoBehaviour client )
        {
            _client = client;
            level = new Level( width , height , spacing , client );
            client.StartCoroutine( Handler() );
        }
    }

    public class Level
    {
        public void GameUpdate() { }

        private float signalTime = 0;
        private float signalInterval = 1;

        public bool EditorUpdate( MonoBehaviour client )
        {
            if ( Time.time > signalTime + signalInterval )
            {
                for ( int i = 0 ; terminals.Count > i ; i++ )
                    if ( terminals[ i ].mode == Terminal.Mode.Home )
                        terminals[ i ].RouteSignal( new Signal( terminals[ i ] , 1 ) , client );

                signalTime = Time.time;
            }

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

            return true;
        }

        public Network AddNetwork( int index )
        {
            Network network = new Network( this , index );
            networks.Add( network );
            return network;
        }

        public Node AddNode( Network network )
        {
            Node node = new Node( network );
            nodes.Add( node );
            return node;
        }

        public Connection AddConnection( Node a , Node b , Node master )
        {
            Connection connection = new Connection( a , b , this );
            connections.Add( connection );
            return connection;
        }

        public Link AddLink( Link link )
        {
            links.Add( link );
            return link;
        }

        public Terminal AddTerminal( Terminal terminal )
        {
            terminals.Add( terminal );
            return terminal;
        }

        public Network RemoveNetwork( int index )
        {
            Network network = NetworkAtIndex( index );

            if ( network != null )
                networks.Remove( network.Destroy() );

            return network;
        }

        public Node RemoveNode( Node node )
        {
            nodes.Remove( node );
            return node;
        }

        public Connection RemoveConnection( Connection connection )
        {
            connections.Remove( connection );
            connection.a.RemoveConnection( connection );
            connection.b.RemoveConnection( connection );
            return connection;
        }

        public Link RemoveLink( Link link )
        {
            links.Remove( link );
            return link;
        }

        public Terminal RemoveTerminal( Terminal terminal )
        {
            terminals.Remove( terminal );
            return terminal;
        }

        private void EditorDraw()
        {
            for ( int i = 0 ; grid.width * grid.height > i ; i++ )
            {
                Draw.LineRendererColor( lineRenderers[ i ] , Overlap( currentWorldMousePosition , 0.01f , this[ i ] , 0.125f ) ? Color.red : Color.blue );
                lineRenderers[ i ].enabled = !Overlap( this[ i ] , 0.125f ) && NetworkAtIndex( i ) == null;
            }

            Draw.Circle( currentValidMousePosition , 0.01f , Color.red );

            for ( int i = 0 ; networks.Count > i ; i++ )
                Draw.Circle( networks[ i ] , hoverNetwork == networks[ i ] ? Color.red : Color.yellow );

            for ( int i = 0 ; nodes.Count > i ; i++ )
                Draw.Circle( nodes[ i ] , hoverNode == nodes[ i ] ? Color.red : nodes[ i ].terminal != null ? Color.green : Color.yellow );

            /*
            for ( int i = 0 ; terminals.Count > i ; i++ )
                Draw.Terminal( terminals[ i ] );

            for ( int i = 0 ; connections.Count > i ; i++ )
                Draw.Line( connections[ i ].a.position , connections[ i ].b.position , Color.yellow );

            for ( int i = 0 ; links.Count > i ; i++ )
                Draw.Line( links[ i ].master.node.position , links[ i ].slave.node.position , Color.green );
            */
        }

        private static bool Overlap ( Vector3 positionA , float radiusA , Vector3 positionB , float radiusB )
        {
            return Mathf.Abs( radiusA + radiusB ) > Vector3.Distance( positionA , positionB );
        }

        private static Vector2 IntersectionPoint( Vector2 a1 , Vector2 a2 , Vector2 b1 , Vector2 b2 )
        {
            float d = ( b2.y - b1.y ) * ( a2.x - a1.x ) - ( b2.x - b1.x ) * ( a2.y - a1.y );
            float a = ( b2.x - b1.x ) * ( a1.y - b1.y ) - ( b2.y - b1.y ) * ( a1.x - b1.x );
            float b = ( a2.x - a1.x ) * ( a1.y - b1.y ) - ( a2.y - a1.y ) * ( a1.x - b1.x );

            if ( d == 0 )
                return Vector3.zero;

            float ua = a / d;
            float ub = b / d;

            return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1
                ? new Vector2( a1.x + ( ua * ( a2.x - a1.x ) ) , a1.y + ( ua * ( a2.y - a1.y ) ) )
                : Vector2.zero;
        }

        private bool Overlap<T> ( T item ) where T : Common
        {
            return Overlap( item.position , item.radius );
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
            if ( Input.GetMouseButtonUp( 1 ) && hoverNode != null && hoverNode.terminal == null )
                hoverNetwork.RemoveNode( hoverNode );
        }

        private void TryAddNetwork()
        {
            if ( Input.GetMouseButtonDown( 0 ) && NetworkAtIndex( nearestIndex ) == null && !Overlap( this[ nearestIndex ] , 0.125f ) )
                AddNetwork( nearestIndex );
        }

        private void TryRemoveNetwork()
        {
            if ( Input.GetMouseButtonUp( 1 ) && NetworkAtIndex( nearestIndex ) != null && NetworkAtIndex( nearestIndex ).nodes.Count == 0 )
                RemoveNetwork( nearestIndex );
        }

        private void TryAddTerminal()
        {
            if ( Input.GetMouseButtonDown( 0 ) && hoverNode != null && hoverNode.terminal == null )
                hoverNode.AddTerminal();
        }

        private void TrySwapTerminal()
        {
            if ( !addingConnection && Input.GetMouseButtonDown( 0 ) && hoverNode != null && hoverNode.terminal != null )
                hoverNode.terminal.SetMode( hoverNode.terminal.mode + 1 > Terminal.Mode.Count - 1 ? Terminal.Mode.None : hoverNode.terminal.mode + 1 );
        }

        private void TryRemoveTerminal()
        {
            if ( !removingConnection && Input.GetMouseButtonUp( 1 ) && hoverNode != null && hoverNode.terminal != null )
                hoverNode.RemoveTerminal();
        }

        private void TryAddConnection()
        {
            if ( Input.GetMouseButtonDown( 0 ) && hoverNode != null && hoverNode.terminal != null )
                client.StartCoroutine( AddConnectionHandler( hoverNode ) );
        }

        private void TryRemoveConnection()
        {
            if ( Input.GetMouseButtonDown( 1 ) && hoverNode == null )
                client.StartCoroutine( RemoveConnectionHandler() );
        }

        private IEnumerator AddConnectionHandler( Node from )
        {
            Node to = null;
            addingConnection = true;

            while ( Input.GetMouseButton( 0 ) )
            {
                Draw.Line( from.position , currentWorldMousePosition , Color.red );
                to = Overlap( currentWorldMousePosition , 0 , nearestNode.position , nearestNode.radius ) ? nearestNode : to;
                yield return null;
            }

            if ( to != from && to != null )
            {
                if ( HasConnection( from , to ) )
                {
                    Link link = new Link( GetConnection( from , to ) , from , to );
                    from.terminal.AddLink( link );
                    to.terminal.AddLink( link );
                    AddLink( link );
                }
                else
                {
                    if ( to.terminal == null )
                        to.AddTerminal();

                    Connection connection = AddConnection( from , to , from );
                    to.AddConnection( connection );
                    from.AddConnection( connection );
                }
            }

            addingConnection = false;
        }

        private IEnumerator RemoveConnectionHandler()
        {
            removingConnection = true;
            Vector3 start = currentWorldMousePosition;

            while ( Input.GetMouseButton( 1 ) )
            {
                Draw.Line( start , currentWorldMousePosition , Color.red );
                yield return null;
            }

            Vector3 end = currentWorldMousePosition;
            int index = -1;

            for ( int i = 0 ; connections.Count > i && 0 > index ; i++ )
                if ( IntersectionPoint( start , end , connections[ i ].a.position , connections[ i ].b.position ) != Vector2.zero )
                    index = i;

            if ( index >= 0 )
            {
                if ( HasLink( connections[ index ].a , connections[ index ].b ) )
                    GetLink( connections[ index ].a , connections[ index ].b ).Destroy();

                connections[ index ].Destroy();
            }

            removingConnection = false;
        }

        private bool HasConnection ( Node a , Node b )
        {
            return GetConnection( a , b ) != null;
        }

        private Connection GetConnection ( Node a , Node b )
        {
            for ( int i = 0 ; connections.Count > i ; i++ )
                if ( connections[ i ].Connects( a , b ) )
                    return connections[ i ];

            return null;
        }

        private bool HasLink ( Node a , Node b )
        {
            return GetLink( a , b ) != null;
        }

        private Link GetLink( Node a , Node b )
        {
            for ( int i = 0 ; links.Count > i ; i++ )
                if ( links[ i ].Has( a ) && links[ i ].Has( b ) )
                    return links[ i ];

            return null;
        }

        private int NearestIndex ( Vector3 position )
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

        private Network NetworkAtIndex ( int index )
        {
            for ( int i = 0 ; networks.Count > i ; i++ )
                if ( networks[ i ].index == index )
                    return networks[ i ];

            return null;
        }

        private T Nearest<T> ( Vector3 position , List<T> candidates ) where T : Common
        {
            int index = -1;
            float shortest = float.PositiveInfinity;

            for ( int i = 0 ; candidates.Count > i ; i++ )
            {
                float distance = Vector3.Distance( position , candidates[ i ].position );

                if ( shortest > distance )
                {
                    index = i;
                    shortest = distance;
                }
            }

            return index >= 0 ? candidates[ index ] : null;
        }

        public Node nearestNode
        {
            get
            {
                List<Node> nodes = new List<Node>( this.nodes.Count );

                for ( int i = 0 ; networks.Count > i ; i++ )
                {
                    Node nearest = Nearest( currentWorldMousePosition , networks[ i ].nodes );

                    if ( nearest != null )
                        nodes.Add( nearest );
                }

                return Nearest( currentWorldMousePosition , nodes );
            }
        }

        public Network nearestNetwork { get { return Nearest( currentWorldMousePosition , networks ); } }

        public Vector3 currentGridMousePosition { get; private set; }
        public Vector3 currentWorldMousePosition { get; private set; }
        public Vector3 lastValidGridMousePosition { get; private set; }
        public Vector3 nearestGridPosition { get { return this[ nearestIndex ]; } }
        public Vector3 this[ int index ] { get { return grid.GetPosition( index ); } }
        public Vector3 this[ int x , int y ] { get { return grid.GetPosition( x , y ); } }
        public Vector3 currentValidMousePosition { get { return currentGridMousePosition != Vector3.zero ? currentGridMousePosition : currentWorldMousePosition; } }
        public Network hoverNetwork { get { return nearestNetwork != null && Overlap( currentValidMousePosition , 0 , nearestNetwork.position , nearestNetwork.radius ) ? nearestNetwork : null; } }
        public Node hoverNode { get { return nearestNode != null && Overlap( currentValidMousePosition , 0 , nearestNode.position , nearestNode.radius ) ? nearestNode : null; } }
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
        private List<LineRenderer> lineRenderers { get; set; }


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

            lineRenderers = new List<LineRenderer>( width * height );

            for ( int i = 0 ; width * height > i ; i++ )
                lineRenderers.Add( Draw.GetLineRenderer( "Grid" + i , new LineRendererSettings( 1 , 1 , 0.01f , Color.white ) ) );

            for ( int i = 0 ; width * height > i ; i++ )
                Draw.Circle( lineRenderers[ i ] , this[ i ] , 0.125f , Color.white );
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
        public bool Links ( Node a , Node b )
        {
            return Has( a ) && Has( b );
        }

        public bool Has ( Node node )
        {
            return master.node == node || slave.node == node;
        }

        public Link SetSlave( Terminal slave )
        {
            this.slave = slave;
            return this;
        }

        public Link SetMaster( Terminal master )
        {
            this.master = master;
            return this;
        }

        public Link Destroy()
        {
            connection.level.RemoveLink( this );
            master.RemoveLink( this );
            slave.RemoveLink( this );
            connection = null;
            master = null;
            slave = null;
            return this;
        }

        public Link Signal( Signal signal , MonoBehaviour client )
        {
            bool start = _signalQueue.Count == 0;
            _signalQueue.Enqueue( signal );

            if ( start )
                client.StartCoroutine( SignalDispatcher( client ) );

            return this;
        }

        IEnumerator SignalDispatcher( MonoBehaviour client )
        {
            while ( _signalQueue.Count > 0 && master != null && slave != null )
            {
                Signal signal = master.Process( _signalQueue.Peek() );
                float delay = 1f / signal.strength;

                for ( int i = 0 ; signal.strength > i ; i++ )
                    client.StartCoroutine( SignalHandler( delay * i , client ) );

                if ( signal.strength > 0 )
                {
                    float wait = 1;

                    while ( wait > 0 && master != null && slave != null )
                        yield return wait -= Time.deltaTime;

                    if ( master != null && slave != null )
                        signal.Route( master , slave , client );
                }

                _signalQueue.Dequeue();
            }
        }

        IEnumerator SignalHandler( float delay , MonoBehaviour client )
        {
            while ( delay > 0 && connection != null && master != null && slave != null && master.node != null && slave.node != null )
                yield return delay -= Time.deltaTime;

            float t = 0;

            while ( 1 > t && connection != null && master != null && slave != null && master.node != null && slave.node != null )
                yield return Draw.Arrow( Vector3.Lerp( master.node.position , slave.node.position , t += Time.deltaTime ) , ( slave.node.position - master.node.position ).normalized , 0.05f );
        }

        public Terminal slave { get; private set; }
        public Terminal master { get; private set; }
        public Connection connection { get; private set; }

        private Queue<Signal> _signalQueue { get; set; }

        public Link ( Connection connection , Node master , Node slave )
        {
            this.connection = connection;
            this.master = master.terminal;
            this.slave = slave.terminal;
            _signalQueue = new Queue<Signal>( 10 );
        }
    }

    public class Signal
    {
        public Signal Route( Terminal from , Terminal to , MonoBehaviour client )
        {
            this.from = from;
            route.Add( from );
            to.RouteSignal( this , client );
            return this;
        }

        public  Signal Plus()
        {
            strength += 1;
            return this;
        }

        public Signal Minus()
        {
            strength -= 1;
            return this;
        }

        public Terminal from { get; private set; }
        public int strength { get; private set; }

        private List<Terminal> route { get; set; }

        public Signal( Terminal from , int strength )
        {
            this.from = from;
            this.strength = strength;
            route = new List<Terminal>();
        }

        public Signal( Signal signal )
        {
            from = signal.from;
            strength = signal.strength;
            route = new List<Terminal>( signal.route );
        }
    }

    public class Terminal
    {
        public Terminal RouteSignal( Signal signal , MonoBehaviour client )
        {
            List<int> indices = new List<int>();

            for ( int i = 0 ; links.Count > i ; i++ )
                if ( links[ i ].master == this )
                    indices.Add( i );

            if ( indices.Count > 0 )
            {

                if ( _broadcast )
                    for ( int i = 0 ; indices.Count > i ; i++ )
                        links[ indices[ i ] ].Signal( new Signal( signal ) , client );
                else
                {
                    int index = indices.IndexOf( _currentLink );
                    index = 0 > index || index + 1 >= indices.Count ? 0 : index + 1;
                    links[ _currentLink = indices[ index ] ].Signal( new Signal( signal ) , client );
                }
            }
            else
                _currentLink = -1;

            return this;
        }

        public Signal Process( Signal signal )
        {
            switch ( mode )
            {
                case Mode.Plus:
                    return signal.Plus();

                case Mode.Minus:
                    return signal.Minus();

                default:
                    return signal;
            }
        }

        public Terminal SetMode ( Mode mode )
        {
            this.mode = mode;
            return this;
        }

        public Terminal AddLink ( Link link )
        {
            links.Add( link );
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
            node.network.level.RemoveLink( link );
            links.Remove( link );
            return this;
        }

        public Terminal Destroy()
        {
            while ( links.Count > 0 )
                RemoveLink( links[ links.Count - 1 ] );

            node.network.level.RemoveTerminal( this );
            links = null;
            node = null;
            return this;
        }

        public List<Link> links { get; private set; }
        public Node node { get; private set; }
        public Mode mode { get; private set; }

        private int _currentLink { get; set; }
        private bool _broadcast { get { return mode == Mode.Multiply; } }

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

    public class Node : Common
    {
        public Terminal AddTerminal()
        {
            if ( terminal == null )
                terminal = network.level.AddTerminal( new Terminal( this ) );

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
            return network.RemoveConnection( connection );
        }

        public Node Destroy()
        {
            while ( connections.Count > 0 )
                connections[ connections.Count - 1 ].Destroy();

            if ( terminal != null )
                terminal.Destroy();

            network.level.RemoveNode( this );
            connections = null;
            terminal = null;
            network = null;

            Draw.Deregister( this );
            return this;
        }

        public override Vector3 position
        {
            get
            {
                float increment = 360 / Mathf.Max( 1 , ( network.nodes.Count - 1 ) );
                float angle = increment * ( network.nodes.Count == 2 && index == 0 ? 1.5f : index );
                return network.position + ( angle > 0 ? ( Quaternion.AngleAxis( angle , Vector3.forward ) * Vector3.up ) * ( network.radius - ( radius * 1.5f ) ) : Vector3.zero );
            }
        }

        public override float radius { get { return 0.0625f; } }

        public Network network { get; private set; }
        public Terminal terminal { get; private set; }
        public List<Connection> connections { get; private set; }
        public int index { get { return network.nodes.IndexOf( this ); } }

        public Node ( Network network )
        {
            this.network = network;
            connections = new List<Connection>();
            Draw.Register( this , new LineRendererSettings( 1 , 0 , 0.01f , Color.white ) );
            Draw.Circle( this , Color.white );
        }
    }

    public class Network : Common
    {
        public void AddNode()
        {
            if ( 9 > nodes.Count )
                nodes.Add( level.AddNode( this ) );
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

            Draw.Deregister( this );
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

        public override float radius { get { return Radius( nodes.Count ); } }
        public override Vector3 position { get { return level[ index ]; } }

        public List<Connection> connections { get; private set; }
        public List<Node> nodes { get; private set; }
        public Level level { get; private set; }
        public int index { get; private set; }

        public Network ( Level level , int index )
        {
            this.index = index;
            this.level = level;
            nodes = new List<Node>( 9 );
            connections = new List<Connection>();
            nodes.Add( level.AddNode( this ) );
            Draw.Register( this , new LineRendererSettings( 1 , 0 , 0.01f , Color.white ) );
            Draw.Circle( this , Color.white );
        }
    }

    public class Connection
    {
        public bool Connects ( Node a , Node b )
        {
            return Has( a ) && Has( b );
        }

        public Node Other ( Node node )
        {
            return node == a ? b : a;
        }

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

    public abstract class Common
    {
        public abstract float radius { get; }
        public abstract Vector3 position { get; }
    }

    public struct LineRendererSettings
    {
        public LineRenderer Apply ( LineRenderer lineRenderer )
        {
            lineRenderer.shadowCastingMode = shadowCastingMode;
            lineRenderer.numCornerVertices = numCornerVertices;
            lineRenderer.receiveShadows = receiveShadows;
            lineRenderer.numCapVertices = numCapVertices;
            lineRenderer.startWidth = startWidth;
            lineRenderer.startColor = startColor;
            lineRenderer.alignment = alignment;
            lineRenderer.endWidth = endWidth;
            lineRenderer.endColor = endColor;
            lineRenderer.loop = loop;
            return lineRenderer;
        }

        public UnityEngine.Rendering.ShadowCastingMode shadowCastingMode { get; private set; }
        public LineAlignment alignment { get; private set; }
        public int numCornerVertices { get; private set; }
        public bool receiveShadows { get; private set; }
        public int numCapVertices { get; private set; }
        public float startWidth { get; private set; }
        public Color startColor { get; private set; }
        public float endWidth { get; private set; }
        public Color endColor { get; private set; }
        public bool loop { get; private set; }

        public LineRendererSettings( int numCornerVertices , int numCapVertices , float width , Color color , bool loop = true )
        {
            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            alignment = LineAlignment.Local;
            receiveShadows = false;

            this.numCornerVertices = numCornerVertices;
            this.numCapVertices = numCapVertices;
            this.loop = loop;

            startWidth = width;
            startColor = color;
            endWidth = width;
            endColor = color;
        }
    }
}