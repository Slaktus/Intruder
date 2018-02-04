using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Intruder
{
    public class Grid
    {
        public void Update()
        {
            for ( int i = 0 ; positions.Length > i ; i++ )
            {
                if ( instances[ i ] != null )
                {
                    for ( int j = 0 ; positions.Length > j ; j++ )
                    {
                        if ( instances[ i ].network != null && i != j )
                        {
                            float distance = Vector3.Distance( positions[ i ] , positions[ j ] ) - 0.5f;

                            if ( instances[ j ] != null && instances[ i ].network.radius > distance )
                            {
                                instances[ j ].Destroy();
                                instances[ j ] = null;
                            }
                        }
                    }
                }
            }

            for ( int i = 0 ; instances.Length > i ; i++ )
            {
                if ( instances[ i ] == null )
                {
                    bool occupied = false;

                    for ( int j = 0 ; positions.Length > j && !occupied ; j++ )
                    {
                        if ( i != j )
                        {
                            float distance = Vector3.Distance( positions[ i ] , positions[ j ] ) - 0.5f;

                            if ( instances[ j ] != null && instances[ j ].network != null && instances[ j ].network.radius > distance )
                                occupied = true;
                        }
                    }

                    if ( !occupied )
                        instances[ i ] = new NetworkEditor( positions[ i ] , this );
                }
            }

            for ( int i = 0 ; instances.Length > i ; i++ )
                if ( instances[ i ] != null )
                    instances[ i ].Update();
        }

        public bool ValidRadius ( NetworkEditor instance , float radius )
        {
            if ( instance == null )
                return false;

            int index = -1;

            for ( int i = 0 ; instances.Length > i && 0 > index ; i++ )
                if ( instances[ i ] == instance )
                    index = i;

            if ( 0 > index )
                return false;

            for ( int i = 0 ; positions.Length > i ; i++ )
                if ( i != index )
                {
                    float distance = Vector3.Distance( positions[ index ] , positions[ i ] ) - 0.25f;

                    if ( instances[ i ] != null && instances[ i ].network != null && radius + instances[ i ].network.radius > distance )
                        return false;
                }

            return true;
        }

        int width;
        int height;
        float spacing;
        Vector3[] positions;
        NetworkEditor[] instances;

        public Grid ( int width , int height , float spacing )
        {
            this.width = width;
            this.height = height;
            this.spacing = spacing;

            int x = 0;
            int y = 0;
            int count = width * height;
            positions = new Vector3[ count ];
            instances = new NetworkEditor[ count ];

            for ( int i = 0 ; count > i ; i++ )
            {
                if ( x == width )
                {
                    y++;
                    x = 0;
                }

                positions[ i ] = new Vector3( ( x * spacing ) - ( width * spacing * 0.5f ) + ( spacing * 0.5f ) , 0 , ( y * spacing ) - ( height * spacing * 0.5f ) + ( spacing * 0.5f ) );
                instances[ i ] = new NetworkEditor( positions[ i ] , this );
                x++;
            }
        }
    }
}

