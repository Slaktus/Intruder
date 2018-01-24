using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Intruder;

public class Bootstrap : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        networkBase = new NetworkEditor();
        networkBase.AddNetwork();

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
        networkBase.Update();

        if ( Input.GetKeyDown( KeyCode.Space ) )
            networkBase.AddNodeEditor();

        if ( Input.GetKeyDown( KeyCode.KeypadPlus ) )
            networkBase.ModifyRadius( 0.1f );

        if ( Input.GetKeyDown( KeyCode.KeypadMinus ) )
            networkBase.ModifyRadius( -0.1f );

        //home.Signal( this );
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
