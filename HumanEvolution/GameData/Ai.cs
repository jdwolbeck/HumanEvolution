using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Ai
{
    public SpriteBase ThinkingAnimal { get; set; }

    public Ai(SpriteBase animalIn)
    {
        ThinkingAnimal = animalIn;
    }

    public List<RectangleF> GetPath(GameData gameData)
    {
        List<RectangleF> returnPath = new List<RectangleF>();
        int closestIndex = -1;
        double closestDistance = 9999999.0;

        for (int i = 0; i < gameData.Sprites.Count; i++)
        {
            if (gameData.Sprites[i] != ThinkingAnimal && gameData.Sprites[i].IsAlive)
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
            returnPath.Add(Animal.PositionToRectangle(gameData.Sprites[closestIndex].Position));
        }

        return returnPath;
    }
}
