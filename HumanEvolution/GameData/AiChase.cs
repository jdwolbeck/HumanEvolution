using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AiChase : Ai, IAi
{
    public override Animal ThinkingAnimal { get; set; }

    public AiChase(Animal animalIn)
    {
        ThinkingAnimal = animalIn;
    }

    public override List<PathLocation> GetPath(GameData gameData)
    {
        List<PathLocation> returnPath = new List<PathLocation>();
        int closestIndex = -1;
        double closestDistance = 999999999.0;

        for (int i = 0; i < gameData.Sprites.Count; i++)
        {
            //If we ever end up needing to reference a value not on SpriteBase then this code should change to cast the sprite as Animal then check if the result is null or not
            if (gameData.Sprites[i] != ThinkingAnimal && gameData.Sprites[i].IsAlive && gameData.Sprites[i] is ILiving && !(gameData.Sprites[i] is Wolf))
            {
                double curDistance = Math.Sqrt(
                (gameData.Sprites[i].Position.X - ThinkingAnimal.Position.X) * (gameData.Sprites[i].Position.X - ThinkingAnimal.Position.X) +
                (gameData.Sprites[i].Position.Y - ThinkingAnimal.Position.Y) * (gameData.Sprites[i].Position.Y - ThinkingAnimal.Position.Y)) / ThinkingAnimal.Speed;

                if (curDistance < closestDistance)
                {
                    closestDistance = curDistance;
                    closestIndex = i;
                }
            }
        }

        if (closestIndex >= 0)
        {
            PathLocation path = new PathLocation();

            //Determine the position, sprite rotation, estimated time and direction. This prevents the main thread from having to do all these calculations
            path.Position = RectangleF.PositionToRectangle(gameData.Sprites[closestIndex].Position);
            path.Rotation = (float)Math.Atan2((path.Position.Y - ThinkingAnimal.Position.Y), (path.Position.X - ThinkingAnimal.Position.X)) + MathHelper.ToRadians(90);
            path.EstimatedTime = (Math.Sqrt((path.Position.X - ThinkingAnimal.Position.X) * (path.Position.X - ThinkingAnimal.Position.X) + (path.Position.Y - ThinkingAnimal.Position.Y) * (path.Position.Y - ThinkingAnimal.Position.Y))) / ThinkingAnimal.Speed;
            path.Direction = new Vector2((float)Math.Cos(path.Rotation - MathHelper.ToRadians(90)), (float)Math.Sin(path.Rotation - MathHelper.ToRadians(90)));
            path.Direction.Normalize();

            returnPath.Add(path);
        }

        return returnPath;
    }
}