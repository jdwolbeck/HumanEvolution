using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Animal : SpriteBase, ISmartAnimal, ILiving
{
    public override bool IsMovable { get; set; } = true;
    public Ai AnimalAi { get; set; }
    public List<RectangleF> Path { get; set; }
    public RectangleF FinalDestination { get; set; }
    public RectangleF CurrentDesination { get; set; }
    public double EstimatedTimeToDestination { get; set; }
    public double ElapsedTravelTime { get; set; }
    public string CurrentPathCompare { get; set; }

    public Animal()
    {
        CurrentDesination = RectangleF.Empty;
        FinalDestination = RectangleF.Empty;
        Path = new List<RectangleF>();
        ElapsedTravelTime = 0;
    }

    public override void UpdateTick(GameTime gameTime, ref GameData gameData)
    {
        List<RectangleF> newPaths = AnimalAi.GetPath(gameData);

        if (CurrentPathCompare != PathsToString(newPaths))
        {
            Path = AnimalAi.GetPath(gameData);

            if (Path.Count() > 0) //Only set the animal moving if we came back with a new path
            {
                NewPathCalc();
            }
        }
    }
    public override void UpdateMovement(GameTime gameTime, ref GameData gameData)
    {
        if (Path.Count > 0)
        {
            if (CurrentDesination == RectangleF.Empty)
            {
                NewPathCalc();
            }

            Position += Direction * (Speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            ElapsedTravelTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (CurrentDesination.Intersects(Bounds) || ElapsedTravelTime > (EstimatedTimeToDestination * 1.2))
            {
                Path.RemoveAt(0);
                CurrentDesination = RectangleF.Empty;
            }
        }
        else
        {
            IsMoving = false; //No items in the Path list, disable moving
        }
    }
    public void NewPathCalc()
    {
        CurrentDesination = Path[0];
        Rotation = (float)Math.Atan2((CurrentDesination.Y - Position.Y), (CurrentDesination.X - Position.X)) + MathHelper.ToRadians(90);
        EstimatedTimeToDestination = (Math.Sqrt((CurrentDesination.X - Position.X) * (CurrentDesination.X - Position.X) + (CurrentDesination.Y - Position.Y) * (CurrentDesination.Y - Position.Y))) / Speed;
        ElapsedTravelTime = 0;
        CurrentPathCompare = PathsToString(Path);
        IsMoving = true;
    }
    public string PathsToString(List<RectangleF> pathsIn)
    {
        string pathConcat = String.Empty;

        foreach (RectangleF r in pathsIn)
        {
            pathConcat += r.ToString();
        }

        return pathConcat;
    }
}