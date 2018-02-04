using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Intruder;

public class Bootstrap : MonoBehaviour
{
    Grid grid;

	// Use this for initialization
	void Awake ()
    {
        grid = new Grid( 8 , 4 , 1.25f );

        /*home = new Home( Vector3.zero );
        plusA = new Plus( Vector3.right * 3 );
        plusB = new Plus( ( Vector3.right * 3 ) + ( Vector3.forward * 3 ) );
        minusA = new Minus( ( Vector3.left * 3 ) + ( Vector3.forward * 3 ) );
        plusC = new Plus( Vector3.left * 3 );
        connectionA = new Connection( home , plusA ).SetMaster( home );
        connectionB = new Connection( plusA , plusB ).SetMaster( plusA );
        connectionC = new Connection( plusB , minusA ).SetMaster( plusB );
        connectionD = new Connection( minusA , plusC ).SetMaster( minusA );
        connectionE = new Connection( plusC , home ).SetMaster( plusC );

        connectionA.ShowConnection( this ).ShowLink( this );
        connectionB.ShowConnection( this ).ShowLink( this );
        connectionC.ShowConnection( this ).ShowLink( this );
        connectionD.ShowConnection( this ).ShowLink( this );
        connectionE.ShowConnection( this ).ShowLink( this );*/
    }

    private void Update()
    {
        grid.Update();
    }

    NetworkEditor networkBase { get; set; }
    /*Home home;
    Plus plusA;
    Plus plusB;
    Plus plusC;
    Minus minusA;
    Connection connectionA;
    Connection connectionB;
    Connection connectionC;
    Connection connectionD;
    Connection connectionE;*/
}
