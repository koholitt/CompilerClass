using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerClass.Models
{
    public class Tokens
    {
        public string Lexema { get; set; }
        public string Tipo { get; set; }
        public int Linea { get; set; }

        public Tokens(string lexema, string tipo, int linea)
        {
            Lexema = lexema;
            Tipo = tipo;
            Linea = linea;
        }
    }
}