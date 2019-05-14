using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PathLocation
{
    public RectangleF Position { get; set; }
    public float Rotation { get; set; }
    public Vector2 Direction { get; set; }
    public double EstimatedTime {get; set;}

    public PathLocation()
    {
    }
}