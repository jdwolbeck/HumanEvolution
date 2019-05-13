using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAi
{
    Animal ThinkingAnimal { get; set; }
    List<RectangleF> GetPath(GameData gameData);
}