using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ThrowHookEvent : EventArgs
{
    public ThrowHookEvent(bool _pressed, float _angle)
    {
        pressed = _pressed;
        angle = _angle;
    }

    public bool pressed;
    public float angle;
}
