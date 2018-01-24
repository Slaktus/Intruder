using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Intruder
{
    public class NodeEditor
    {
        public NodeEditor SetPosition ( Vector3 position )
        {
            if ( node != null )
                node.position = position + Vector3.up;

            _gameObject.transform.position = position + Vector3.up;
            return this;
        }

        public Node Add<T>() where T : Node
        {
            Type type = typeof( T );
            RemoveNode();

            if ( _addNode.activeSelf )
                _addNode.SetActive( false );

            if ( !_removeNode.activeSelf )
                _removeNode.SetActive( true );

            if ( !_nextNode.activeSelf )
                _nextNode.SetActive( true );

            if ( !_prevNode.activeSelf )
                _prevNode.SetActive( true );

            Node node =
                type == typeof( Home ) ? AddHome() :
                type == typeof( Plus ) ? AddPlus() :
                type == typeof( Minus ) ? AddMinus() : 
                null;

            if ( node != null )
                _network.Add( node );

            return node;
        }

        public NodeEditor RemoveNode()
        {
            if ( !_addNode.activeSelf )
                _addNode.SetActive( true );

            if ( _removeNode.activeSelf )
                _removeNode.SetActive( false );

            if ( _nextNode.activeSelf )
                _nextNode.SetActive( false );

            if ( _prevNode.activeSelf )
                _prevNode.SetActive( false );

            if ( node != null )
            {
                node.Destroy();
                _network.Remove( node );
            }

            node = null;
            return this;
        }

        public NodeEditor Remove()
        {
            RemoveNode();
            GameObject.Destroy( _gameObject );
            return this;
        }

        private Node SwitchNode ( int step )
        {
            int next = ( int ) _current + step;
            next = next > ( int ) Nodes.Count - 1 ? 1 : next;
            next = 1 > next ? ( int ) Nodes.Count - 1 : next;

            switch ( ( Nodes ) next )
            {
                case Nodes.Home:
                    return Add<Home>();

                case Nodes.Plus:
                    return Add<Plus>();

                case Nodes.Minus:
                    return Add<Minus>();

                default:
                    return null;
            }
        }

        private Node AddHome()
        {
            _current = Nodes.Home;
            return node = new Home( _network , _gameObject );
        }

        private Node AddPlus()
        {
            _current = Nodes.Plus;
            return node = new Plus( _network , _gameObject );
        }

        private Node AddMinus()
        {
            _current = Nodes.Minus;
            return node = new Minus( _network , _gameObject );
        }

        public Node node { get; private set; }

        private GameObject _gameObject { get; set; }
        private GameObject _removeNode { get; set; }
        private GameObject _addNode { get; set; }
        private GameObject _prevNode { get; set; }
        private GameObject _nextNode { get; set; }
        private Network _network { get; set; }
        private Nodes _current { get; set; }

        public NodeEditor( Network network , GameObject parent )
        {
            _network = network;

            _gameObject = GameObject.CreatePrimitive( PrimitiveType.Cube );
            _gameObject.GetComponent<MeshRenderer>().material.color = Color.black;
            _gameObject.transform.localScale = Vector3.one * 0.25f;
            _gameObject.transform.position += Vector3.up;
            _gameObject.transform.SetParent( parent.transform );

            _removeNode = GameObject.CreatePrimitive( PrimitiveType.Cylinder );
            _removeNode.GetComponent<MeshRenderer>().material.color = Color.red;
            _removeNode.AddComponent<QuickButton>().SetMouseDown( ( QuickButton button ) => RemoveNode() );
            _removeNode.transform.SetParent( _gameObject.transform );
            _removeNode.transform.localPosition = ( Vector3.back + Vector3.up ) * 0.5f;
            _removeNode.transform.localScale = Vector3.one * 0.5f;
            _removeNode.SetActive( false );

            _addNode = GameObject.CreatePrimitive( PrimitiveType.Cylinder );
            _addNode.GetComponent<MeshRenderer>().material.color = Color.green;
            _addNode.AddComponent<QuickButton>().SetMouseDown( ( QuickButton button ) => Add<Home>() );
            _addNode.transform.SetParent( _gameObject.transform );
            _addNode.transform.localPosition = ( Vector3.forward + Vector3.up ) * 0.5f;
            _addNode.transform.localScale = Vector3.one * 0.5f;
            _addNode.SetActive( true );

            _prevNode = GameObject.CreatePrimitive( PrimitiveType.Cylinder );
            _prevNode.GetComponent<MeshRenderer>().material.color = Color.green;
            _prevNode.AddComponent<QuickButton>().SetMouseDown( ( QuickButton button ) => SwitchNode( -1 ) );
            _prevNode.transform.SetParent( _gameObject.transform );
            _prevNode.transform.localPosition = ( Vector3.left + Vector3.up ) * 0.5f;
            _prevNode.transform.localScale = Vector3.one * 0.5f;
            _prevNode.SetActive( false );

            _nextNode = GameObject.CreatePrimitive( PrimitiveType.Cylinder );
            _nextNode.GetComponent<MeshRenderer>().material.color = Color.green;
            _nextNode.AddComponent<QuickButton>().SetMouseDown( ( QuickButton button ) => SwitchNode( 1 ) );
            _nextNode.transform.SetParent( _gameObject.transform );
            _nextNode.transform.localPosition = ( Vector3.right + Vector3.up ) * 0.5f;
            _nextNode.transform.localScale = Vector3.one * 0.5f;
            _nextNode.SetActive( false );
        }

        private enum Nodes
        {
            None,
            Home,
            Plus,
            Minus,
            Count
        }
    }
}