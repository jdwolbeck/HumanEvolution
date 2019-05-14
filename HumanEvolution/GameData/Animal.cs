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
    public List<PathLocation> Path { get; set; }
    public PathLocation FinalDestination { get; set; }
    public PathLocation CurrentDesination { get; set; }
    public double EstimatedTimeToDestination { get; set; }
    public double ElapsedTravelTime { get; set; }
    public string CurrentPathCompare { get; set; }
    public double ThinkingCooldownMs { get; set; }
    public double ElapsedTimeSinceLastThought { get; set; }

    public Animal()
    {
        CurrentDesination = new PathLocation() { Position = RectangleF.Empty };
        FinalDestination = new PathLocation() { Position = RectangleF.Empty };
        Path = new List<PathLocation>();
        ElapsedTravelTime = 0;
        ElapsedTimeSinceLastThought = 0;
    }

    public override void UpdateTick(GameTime gameTime, ref GameData gameData)
    {
    }
    public override void Update(GameTime gameTime, ref GameData gameData)
    {
        ElapsedTimeSinceLastThought += gameTime.ElapsedGameTime.TotalMilliseconds;

        base.Update(gameTime, ref gameData);
    }
    public override void UpdateMovement(GameTime gameTime, ref GameData gameData)
    {
        if (Path.Count > 0)
        {
            if (CurrentDesination.Position == RectangleF.Empty)
            {
                NewPathCalc();
            }

            Position += Direction * (Speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            ElapsedTravelTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (CurrentDesination.Position.Intersects(Bounds) || ElapsedTravelTime > (EstimatedTimeToDestination * 1.2))
            {
                Path.RemoveAt(0);
                CurrentDesination = new PathLocation() { Position = RectangleF.Empty };
            }
        }
        else
        {
            IsMoving = false; //No items in the Path list, disable moving
        }
    }
    public void NewPathCalc(string currentPathCompareIn = "") //Default the parameter to empty string. In the AiThread we will pass it in since we calculate it to compare. Save on building the string
    {
        CurrentDesination = Path[0];
        SetRotationAndDirection(Path[0].Rotation, Path[0].Direction); //Use this function so that the SpriteBase Rotation setter does not run the Direction calculation
        EstimatedTimeToDestination = Path[0].EstimatedTime;
        ElapsedTravelTime = 0;
        if (String.IsNullOrEmpty(currentPathCompareIn))
        {
            CurrentPathCompare = Ai.PathsToString(Path);
        }
        IsMoving = true;
    }
}