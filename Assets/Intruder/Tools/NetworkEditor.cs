using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Intruder
{
    public class NetworkEditor
    {
        public void Update()
        {
            if ( _nodeEditors.Count > 0 )
                _nodeEditors[ 0 ].SetPosition( _gameObject.transform.position );

            if ( _nodeEditors.Count > 1 )
            {
                float angle = 360 / Mathf.Max( _nodeEditors.Count - 1 , 0 );

                for ( int i = 1 ; _nodeEditors.Count > i ; i++ )
                    _nodeEditors[ i ].SetPosition( _gameObject.transform.position + ( Quaternion.AngleAxis( angle * i , Vector3.up ) * ( Quaternion.Euler( 0 , _rotation , 0 ) * Vector3.forward * Mathf.Clamp( _radius , 0.5f , float.PositiveInfinity ) * 0.6f ) ) );
            }
        }

        public NetworkEditor ModifyRotation( float angle )
        {
            _rotation += angle;
            _rotation = Mathf.Repeat( _rotation , 360 );
            network.SetRotation( _rotation );
            return this;
        }

        public NetworkEditor ModifyRadius( float distance )
        {
            _radius += distance;
            _radius = Mathf.Clamp( _radius , _nodeEditors.Count > 1 ? 0.85f : 0.5f , _nodeEditors.Count > 1 ? 2 : 0.5f );

            _radiusHandle.transform.position = network.gameObject.transform.position + ( Vector3.right * _radius ) + ( Vector3.up * network.gameObject.transform.localScale.y );
            _rotationHandle.transform.position = network.gameObject.transform.position + ( Vector3.left * _radius ) + ( Vector3.up * network.gameObject.transform.localScale.y );
            _addNodeButton.transform.position = network.gameObject.transform.position + ( Vector3.forward * _radius ) + ( Vector3.up * network.gameObject.transform.localScale.y );
            _removeNodeButton.transform.position = network.gameObject.transform.position + ( Vector3.back * _radius ) + ( Vector3.up * network.gameObject.transform.localScale.y );
            network.SetRadius( _radius );
            return this;
        }

        public NetworkEditor AddNodeEditor()
        {
            if ( 9 > _nodeEditors.Count )
                _nodeEditors.Add( new NodeEditor( network , _gameObject ) );

            if ( _nodeEditors.Count == 9 )
                _addNodeButton.SetActive( false );

            if ( _nodeEditors.Count > 1 && !_radiusHandle.activeSelf )
                _radiusHandle.SetActive( true );

            if ( _nodeEditors.Count > 1 && !_rotationHandle.activeSelf )
                _rotationHandle.SetActive( true );

            if ( !_removeNodeButton.activeSelf )
                _removeNodeButton.SetActive( true );

            ModifyRadius( 0 );
            return this;
        }

        public NetworkEditor RemoveNodeEditor()
        {
            if ( _nodeEditors.Count == 0 )
            {
                RemoveNetwork();
                GameObject.Destroy( _gameObject );
            }
            else
            {
                int index = _nodeEditors.Count - 1;

                if ( index >= 0 )
                    _nodeEditors.Remove( _nodeEditors[ index ].Remove() );

                if ( _nodeEditors.Count == 0 && _radiusHandle.activeSelf )
                    _radiusHandle.SetActive( false );

                if ( _nodeEditors.Count == 0 && _rotationHandle.activeSelf )
                    _rotationHandle.SetActive( false );

                if ( 9 > _nodeEditors.Count && !_addNodeButton.activeSelf )
                    _addNodeButton.SetActive( true );

                ModifyRadius( 0 );
            }
            
            return this;
        }

        public NetworkEditor AddNetwork()
        {
            network = new Network( _gameObject );
            _nodeEditors = new List<NodeEditor>();
            _addNodeButton.SetActive( true );
            _radius = network.gameObject.transform.localScale.x * 0.5f;
            _radiusHandle.transform.position = network.gameObject.transform.position + ( Vector3.right * _radius ) + ( Vector3.up * network.gameObject.transform.localScale.y );
            _rotationHandle.transform.position = network.gameObject.transform.position + ( Vector3.left * _radius ) + ( Vector3.up * network.gameObject.transform.localScale.y );
            _addNodeButton.transform.position = network.gameObject.transform.position + ( Vector3.forward * _radius ) + ( Vector3.up * network.gameObject.transform.localScale.y );
            _removeNodeButton.transform.position = network.gameObject.transform.position + ( Vector3.back * _radius ) + ( Vector3.up * network.gameObject.transform.localScale.y );
            return this;
        }

        public NetworkEditor RemoveNetwork()
        {
            if ( network != null )
                network.Destroy();

            _removeNodeButton.SetActive( false );
            _addNodeButton.SetActive( false );
            _rotationHandle.SetActive( false );
            _radiusHandle.SetActive( false );
            network = null;
            return this;
        }

        public Network network { get; private set; }

        private List<NodeEditor> _nodeEditors { get; set; }
        private GameObject _addNodeButton { get; set; }
        private GameObject _removeNodeButton { get; set; }
        private GameObject _rotationHandle { get; set; }
        private GameObject _radiusHandle { get; set; }
        private GameObject _gameObject { get; set; }
        private float _rotation { get; set; }
        private float _radius { get; set; }

        private void RadiusHandle( QuickButton button )
        {
            button.StartCoroutine( RadiusHandler( button ) );
        }

        private IEnumerator RadiusHandler( QuickButton button )
        {
            Vector2 prevMouse = Input.mousePosition;

            while ( Input.GetMouseButton( 0 ) )
            {
                Vector2 mouse = Input.mousePosition;
                ModifyRadius( ( mouse.x - prevMouse.x ) * Time.deltaTime );
                prevMouse = mouse;
                yield return null;
            }
        }

        private void RotationHandle( QuickButton button )
        {
            button.StartCoroutine( RotationHandler( button ) );
        }

        private IEnumerator RotationHandler( QuickButton button )
        {
            Vector2 prevMouse = Input.mousePosition;

            while ( Input.GetMouseButton( 0 ) )
            {
                Vector2 mouse = Input.mousePosition;
                ModifyRotation( ( mouse.x - prevMouse.x ) * Time.deltaTime * 4 );
                prevMouse = mouse;
                yield return null;
            }
        }

        private void AddNodeEditor( QuickButton button )
        {
            AddNodeEditor();
        }

        private void RemoveNodeEditor ( QuickButton button )
        {
            RemoveNodeEditor();
        }

        public NetworkEditor()
        {
            _gameObject = new GameObject( "NetworkBase" );

            _radiusHandle = GameObject.CreatePrimitive( PrimitiveType.Cylinder );
            _radiusHandle.GetComponent<MeshRenderer>().material.color = Color.cyan;
            _radiusHandle.AddComponent<QuickButton>().SetMouseDown( ( QuickButton button ) => RadiusHandle( button ) );
            _radiusHandle.transform.localScale = Vector3.one * 0.2f;
            _radiusHandle.transform.SetParent( _gameObject.transform );
            _radiusHandle.SetActive( false );

            _rotationHandle = GameObject.CreatePrimitive( PrimitiveType.Cylinder );
            _rotationHandle.GetComponent<MeshRenderer>().material.color = Color.yellow;
            _rotationHandle .AddComponent<QuickButton>().SetMouseDown( ( QuickButton button ) => RotationHandle( button ) );
            _rotationHandle.transform.localScale = Vector3.one * 0.2f;
            _rotationHandle.transform.SetParent( _gameObject.transform );
            _rotationHandle.SetActive( false );

            _addNodeButton = GameObject.CreatePrimitive( PrimitiveType.Cylinder );
            _addNodeButton.GetComponent<MeshRenderer>().material.color = Color.green;
            _addNodeButton.AddComponent<QuickButton>().SetMouseDown( ( QuickButton button ) => AddNodeEditor( button ) );
            _addNodeButton.transform.localScale = Vector3.one * 0.2f;
            _addNodeButton.transform.SetParent( _gameObject.transform );
            _addNodeButton.SetActive( true );

            _removeNodeButton = GameObject.CreatePrimitive( PrimitiveType.Cylinder );
            _removeNodeButton.GetComponent<MeshRenderer>().material.color = Color.red;
            _removeNodeButton.AddComponent<QuickButton>().SetMouseDown( ( QuickButton button ) => RemoveNodeEditor( button ) );
            _removeNodeButton.transform.localScale = Vector3.one * 0.2f;
            _removeNodeButton.transform.SetParent( _gameObject.transform );
            _removeNodeButton.SetActive( true );
        }
    }
}