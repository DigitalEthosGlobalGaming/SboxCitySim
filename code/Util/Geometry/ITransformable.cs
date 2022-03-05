using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GridSystem.Geometry
{
    internal interface ITransformable
    {
        void Move(Vector2 offset);
        void Rotate(float angleInDegrees, Vector2 rotationOrigin);

    }
}
