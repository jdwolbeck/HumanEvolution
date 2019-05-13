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

    public override List<RectangleF> GetPath(GameData gameData)
    {
        List<RectangleF> returnPath = new List<RectangleF>();
        int closestIndex = -1;
        double closestDistance = 999999999.0;

        for (int i = 0; i < gameData.Sprites.Count; i++)
        {
            //If we ever end up needing to reference a value not on SpriteBase then this code should change to cast the sprite as Animal then check if the result is null or not
            if (gameData.Sprites[i] != ThinkingAnimal && gameData.Sprites[i].IsAlive && !(gameData.Sprites[i] is Wolf))
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
            returnPath.Add(RectangleF.PositionToRectangle(gameData.Sprites[closestIndex].Position));
        }

        return returnPath;
    }
}