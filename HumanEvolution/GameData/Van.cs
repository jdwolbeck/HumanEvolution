﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Van : SpriteBase, ILiving
{
    public override bool IsMovable { get; set; } = true;

    public Van()
    {
    }
}