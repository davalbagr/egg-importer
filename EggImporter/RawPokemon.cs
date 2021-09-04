using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggImporter
{
    class RawPokemon
    {
        public int Species { get; set; }
        public int Ability { get; set; }
        public int Gender { get; set; }
        public bool IsShiny { get; set; }
        public int Nature { get; set; }
        public int HP { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int SpA { get; set; }
        public int SpD { get; set; }
        public int Spe { get; set; }
        public int MoveOne { get; set; }
        public int MoveTwo { get; set; }
        public int MoveThree { get; set; }
        public int MoveFour { get; set; }
    }
}
