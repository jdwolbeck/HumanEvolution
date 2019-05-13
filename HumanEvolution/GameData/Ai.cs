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

    public virtual List<RectangleF> GetPath(GameData gameData)
    {
        throw new Exception("Unimplemented GetPath");
    }
}
