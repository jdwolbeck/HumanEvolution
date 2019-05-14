using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Ai : IAi
{
    public virtual Animal ThinkingAnimal { get; set; }

    public Ai()
    {
    }

    public virtual List<PathLocation> GetPath(GameData gameData)
    {
        throw new Exception("Unimplemented GetPath");
    }

    public static string PathsToString(List<PathLocation> pathsIn)
    {
        string pathConcat = String.Empty;

        foreach (PathLocation r in pathsIn)
        {
            pathConcat += r.Position.ToString();
        }

        return pathConcat;
    }
}
