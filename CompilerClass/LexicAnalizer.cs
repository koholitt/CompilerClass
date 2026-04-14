using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Text.RegularExpressions; 
using System.Threading.Tasks;
using CompilerClass.Models;

namespace AccesoDatos
{
    public class LexicAnalizer
    {
        // Listas que definen el lenguaje 
        public List<string> PalabrasReservadas { get; } = new List<string> { "int", "float", "if", "else", "while", "return" };
        public List<string> Operadores { get; } = new List<string> { "+", "-", "*", "/", "=", ">" };
        public List<string> Delimitadores { get; } = new List<string> { ";", "(", ")", "{", "}" };

        public List<Tokens> Analizar(string codigo)
        {
            List<Tokens> tokens = new List<Tokens>();

            // Regex para números y para identificadores
            Regex numeroRegex = new Regex(@"^\d+$");
            Regex idRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");

            string[] lineas = codigo.Split('\n');

            for (int i = 0; i < lineas.Length; i++)
            {
                string linea = lineas[i];
                string palabraActual = "";

                foreach (char c in linea)
                {
                    // 1. Manejo de espacios en blanco
                    if (char.IsWhiteSpace(c))
                    {
                        ProcesarPalabra(palabraActual, i, tokens, numeroRegex, idRegex);
                        palabraActual = "";
                    }
                    // 2. Manejo de delimitadores y operadores
                    else if (Delimitadores.Contains(c.ToString()) || Operadores.Contains(c.ToString()))
                    {
                        // Procesamos lo que veníamos acumulando antes del símbolo
                        ProcesarPalabra(palabraActual, i, tokens, numeroRegex, idRegex);
                        palabraActual = "";

                        // Clasificamos el símbolo actual
                        string tipo = Delimitadores.Contains(c.ToString()) ? "Delimitador" : "Operador";
                        tokens.Add(new Tokens(c.ToString(), tipo, i + 1));
                    }
                    // 3. Formación de palabras (letras, números, guiones)
                    else if (char.IsLetterOrDigit(c) || c == '_')
                    {
                        palabraActual += c;
                    }
                    // 4. Caracteres no permitidos
                    else
                    {
                        ProcesarPalabra(palabraActual, i, tokens, numeroRegex, idRegex);
                        palabraActual = "";
                        tokens.Add(new Tokens(c.ToString(), "Error", i + 1));
                    }
                }

                // Procesar remanente al final de la línea
                ProcesarPalabra(palabraActual, i, tokens, numeroRegex, idRegex);
            }
            return tokens;
        }

        private void ProcesarPalabra(string palabra, int linea, List<Tokens> tokens, Regex numeroRegex, Regex idRegex)
        {
            if (string.IsNullOrWhiteSpace(palabra)) return;

            if (PalabrasReservadas.Contains(palabra))
                tokens.Add(new Tokens(palabra, "Reservada", linea + 1));
            else if (numeroRegex.IsMatch(palabra))
                tokens.Add(new Tokens(palabra, "Número", linea + 1));
            else if (idRegex.IsMatch(palabra))
                tokens.Add(new Tokens(palabra, "Identificador", linea + 1));
            else
                tokens.Add(new Tokens(palabra, "Error", linea + 1));
        }
    }
}