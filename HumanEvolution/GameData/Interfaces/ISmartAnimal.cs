﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

interface ISmartAnimal
{
    Ai AnimalAi { get; set; }
    double ThinkingCooldownMs { get; set; }
    double ElapsedTimeSinceLastThought { get; set; }
}