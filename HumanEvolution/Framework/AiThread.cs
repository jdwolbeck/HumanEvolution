using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class AiThread
{
    public bool _stop;
    private GameData _gameData;
    private Random _rand;

    public AiThread(GameData gameDataFromMain, Random rand)
    {
        _gameData = gameDataFromMain;
        _rand = rand;
        _stop = false;
    }

    public void Start()
    {
        RunAiLoop();
    }

    public void RunAiLoop()
    {
        while (!_stop)
        {
            for (int i = 0; i < _gameData.Sprites.Count; i++)
            {
                try
                {
                    Animal animal = _gameData.Sprites[i] as Animal;

                    if (animal != null) //If animal is null that means the sprite is not of type Animal
                    {
                        if (animal.ElapsedTimeSinceLastThought >= animal.ThinkingCooldownMs && animal.Path.Count() == 0)
                        {
                            List<RectangleF> newPaths = animal.AnimalAi.GetPath(_gameData);
                            string newCompare = animal.PathsToString(newPaths);

                            //animal.Path.Add(RectangleF.PositionToRectangle(new Vector2(_rand.Next(0, 9900), _rand.Next(0, 9900))));
                            //animal.NewPathCalc();

                            if (animal.CurrentPathCompare != newCompare)
                            {
                                animal.Path = newPaths;

                                if (animal.Path.Count() > 0) //Only set the animal moving if we came back with a new path
                                {
                                    animal.NewPathCalc(newCompare);
                                }
                            }
                            else
                            {
                                newPaths = null;
                                newCompare = null;
                            }

                            animal.ElapsedTimeSinceLastThought = 0; //Blank it out. If the Ai call takes longer than ThinkingCooldownMs we dont want a single Animal to hog up the processing time
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(System.IO.Path.Combine("", "AiThreadException.txt"), DateTime.Now.ToString() + " - AI Thread Uncaught error: " + ex.Message + Environment.NewLine + "Stacktrace: " + ex.StackTrace);
                }
            }

            Thread.Sleep(1);
        }
    }
}