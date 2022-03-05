using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace GridSystem.Geometry
{
    public class CompoundPath
    {
        public Vector2 Start;
        internal Vector2 End;

        private readonly List<IPathShape> shapes = new List<IPathShape>();

        public IReadOnlyList<IPathShape> Shapes => shapes;

        public CompoundPath ArcTo(float radius, float startAngle, float endAngle)
        {
            shapes.Add(new Arc { Radius = radius, StartAngle = startAngle, EndAngle = endAngle });
            return this;
        }


        public CompoundPath LineTo(Vector2 point)
        {
            shapes.Add(new Line { P1 = End, P2 = point });
            End = point;
            return this;
        }


        public CompoundPath CurveTo(params Vector2[] controlPoints)
        {
            var list = new List<Vector2>
            {
                End
            };
            list.AddRange(controlPoints);

            var bez = new Bezier(list);


            shapes.AddRange(bez.ToArcs());
            End = controlPoints.Last();

            return this;
        }

        public float Length
        {
            get
            {
                return shapes.Sum(shp => shp.Length);
            }
        }

        public void Offset(Vector2 offset)
        {
            Start += offset;
            End += offset;

            foreach (var shape in shapes)
            {
                shape.Move(offset);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"M {Start.x} {Start.y}");

            foreach (var shape in shapes)
            {
                if (shape is Line line)
                {
                    sb.Append(" ");
                    sb.Append($"L {line.P2.x} {line.P2.y}");
                }
                if (shape is Arc arc)
                {
                    sb.Append(" ");
                    sb.Append(arc.ToString(false));
                }
                else if (shape is Bezier bezier)
                {
                    if (bezier.order == 2)
                    {
                        sb.Append(" ");
                        sb.Append($"Q {bezier.Points[1].x} {bezier.Points[1].y} {bezier.Points[2].x} {bezier.Points[2].y}");
                    }
                    else if (bezier.order == 3)
                    {
                        sb.Append(" ");
                        sb.Append($"C {bezier.Points[1].x} {bezier.Points[1].y} {bezier.Points[2].x} {bezier.Points[2].y} {bezier.Points[3].x} {bezier.Points[3].y}");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
