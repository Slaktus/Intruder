using UnityEngine;
using System;

public class QuickButton : MonoBehaviour
{
    public QuickButton SetMouseEnter (Action<QuickButton> MouseEnter )
    {
        this.MouseEnter = MouseEnter;
        return this;
    }

    public QuickButton SetMouseExit( Action<QuickButton> MouseExit )
    {
        this.MouseExit = MouseExit;
        return this;
    }

    public QuickButton SetMouseDown( Action<QuickButton> MouseDown )
    {
        this.MouseDown = MouseDown;
        return this;
    }
    private void OnMouseEnter()
    {
        MouseEnter( this );
    }

    private void OnMouseExit()
    {
        MouseExit( this );
    }

    private void OnMouseDown()
    {
        MouseDown( this );
    }

    private Action<QuickButton> MouseEnter = ( QuickButton quickButton ) => { };
    private Action<QuickButton> MouseExit = ( QuickButton quickButton ) => { };
    private Action<QuickButton> MouseDown = ( QuickButton quickButton ) => { };
}
