using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Car : SpriteBase, ILiving
{
    public override bool IsMovable { get; set; } = true;

    public Car()
    {
    }
}
