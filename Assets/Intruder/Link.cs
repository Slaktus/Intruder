using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Intruder
{
    public class Link
    {
        public void Operate( MonoBehaviour client , Vector3 offset , Node from , Node to , Operation operation )
        {
            client.StartCoroutine( OperationHandler( offset , from , to , operation ) );
        }

        public Link SetPositions ( Node master , Node slave , Vector3 offset )
        {
            _lineRenderer.SetPositions( new Vector3[] { master.position + offset , slave.position + offset } );
            return this;
        }

        public Link SetLerpTime ( float lerpTime )
        {
            this.lerpTime = lerpTime;
            return this;
        }

        private Link SetShowing ( bool showing )
        {
            this.showing = showing;
            return this;
        }

        private Link SetPositions ( Vector3[] positions )
        {
            _lineRenderer.SetPositions( positions );
            return this;
        }

        private IEnumerator OperationHandler( Vector3 offset , Node from , Node to , Operation operation )
        {
            if ( PerformOperation( operation ) )
            {
                SetShowing( operation == Operation.Show );
                Func<bool> Condition = GetCondition( operation );

                if ( _lineRenderer.positionCount != 2 )
                    _lineRenderer.positionCount = 2;

                while ( Condition() && lerpTime >= 0 && 1 >= lerpTime )
                    yield return SetPositions( new Vector3[] { from.position + offset , Vector3.Lerp( from.position + offset , to.position + offset , UpdateLerpTime( operation ) ) } );

                if ( Condition() )
                    _lineRenderer.SetPositions( new Vector3[] { from.position + offset , Vector3.Lerp( from.position + offset , to.position + offset , FinalizeLerpTime( operation ) ) } );
            }
        }

        private bool PerformOperation( Operation operation )
        {
            switch ( operation )
            {
                case Operation.Hide:
                    return showing;

                case Operation.Show:
                    return !showing;

                default:
                    return false;
            }
        }

        private Func<bool> GetCondition( Operation operation )
        {
            switch ( operation )
            {
                case Operation.Show:
                    return () => showing;

                case Operation.Hide:
                    return () => !showing;

                default:
                    return () => false;
            }
        }

        private float UpdateLerpTime( Operation operation )
        {
            switch ( operation )
            {
                case Operation.Show:
                    return lerpTime += Time.deltaTime;

                case Operation.Hide:
                    return lerpTime -= Time.deltaTime;

                default:
                    return lerpTime;
            }
        }

        private float FinalizeLerpTime( Operation operation )
        {
            switch ( operation )
            {
                case Operation.Show:
                    return lerpTime = 1;

                case Operation.Hide:
                    _lineRenderer.positionCount = 0;
                    return lerpTime = 0;

                default:
                    return 0;
            }
        }

        public bool showing { get; private set; }
        public float lerpTime { get; private set; }

        private LineRenderer _lineRenderer { get; set; }

        public Link( GameObject parent , Color color , float startWidth , float endWidth , string name = "Link" , int numCapVertices = 3 )
        {
            _lineRenderer = new GameObject( name ).AddComponent<LineRenderer>();
            _lineRenderer.transform.SetParent( parent.transform );
            _lineRenderer.transform.localRotation = Quaternion.LookRotation( Vector3.down );
            _lineRenderer.alignment = LineAlignment.Local;
            _lineRenderer.numCapVertices = numCapVertices;
            _lineRenderer.startWidth = startWidth;
            _lineRenderer.endWidth = endWidth;
            _lineRenderer.positionCount = 0;
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
        }

        public enum Operation
        {
            Show,
            Hide
        }
    }
}