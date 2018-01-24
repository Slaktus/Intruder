using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Intruder
{
    public class Marker
    {
        public void Operate ( MonoBehaviour client , Operation operation )
        {
            if ( ( !showing && operation == Operation.Show ) || ( showing && operation == Operation.Hide ) )
                client.StartCoroutine( HandleMarker() );
        }

        private IEnumerator HandleMarker()
        {
            showing = !showing;
            bool current = showing;
            _marker.loop = false;

            while ( current == showing && _lerpTime >= 0 && 1 >= _lerpTime )
            {
                _lerpTime = current ? _lerpTime + Time.deltaTime : _lerpTime - Time.deltaTime;
                int count = Mathf.Clamp( Mathf.RoundToInt( _resolution * _lerpTime ) , 0 , _resolution );

                if ( count != _marker.positionCount )
                {
                    _marker.positionCount = count;
                    _marker.SetPositions( _positions.ToArray() );
                }

                yield return null;
            }

            if ( current == showing && current )
                _marker.loop = true;

            _lerpTime = Mathf.Clamp01( _lerpTime );
        }

        public bool showing { get; private set; }

        private float _lerpTime { get; set; }
        private int _resolution { get; set; }
        private LineRenderer _marker { get; set; }
        private List<Vector3> _positions { get; set; }

        public Marker( int resolution , GameObject parent )
        {
            _marker = new GameObject( "Marker" ).AddComponent<LineRenderer>();
            _marker.transform.localRotation = Quaternion.LookRotation( Vector3.down );
            _marker.transform.SetParent( parent.transform );
            _marker.positionCount = 0;

            _resolution = resolution;
            float increment = 360 / resolution;
            _positions = new List<Vector3>( resolution );

            for ( int i = 0 ; resolution > i ; i++ )
                _positions.Add( ( Quaternion.AngleAxis( i * increment , Vector3.back ) * Vector3.up ) * 0.75f );

            _marker.endWidth = 0.5f;
            _marker.startWidth = 0.5f;
            _marker.positionCount = 0;
            _marker.useWorldSpace = false;
            _marker.numCornerVertices = 1;
            _marker.alignment = LineAlignment.Local;
            _marker.SetPositions( _positions.ToArray() );
        }

        public enum Operation
        {
            Show,
            Hide
        }
    }
}