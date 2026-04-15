using System;
using System.Collections.Generic;
using CompilerClass.Models;

namespace CompilerClass
{
    public class SemanticAnalizer
    {
        // Symbol Table: Stores Variable Name and its Type
        private Dictionary<string, string> tablaSimbolos = new Dictionary<string, string>();
        public List<string> ErroresSemanticos { get; private set; } = new List<string>();

        public void Validar(List<Tokens> tokens)
        {
            ErroresSemanticos.Clear();
            tablaSimbolos.Clear();

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                // Check for declarations: "int x" or "float y"
                if (token.Lexema == "int" || token.Lexema == "float")
                {
                    if (i + 1 < tokens.Count && tokens[i + 1].Tipo == "Identificador")
                    {
                        string varName = tokens[i + 1].Lexema;
                        if (!tablaSimbolos.ContainsKey(varName))
                        {
                            tablaSimbolos.Add(varName, token.Lexema);
                        }
                        else
                        {
                            ErroresSemanticos.Add($"Semántico: La variable '{varName}' ya existe (Línea {token.Linea})");
                        }
                    }
                }

                // Check for usage: Is the variable declared?
                if (token.Tipo == "Identificador")
                {
                    bool isBeingDeclared = (i > 0 && (tokens[i - 1].Lexema == "int" || tokens[i - 1].Lexema == "float"));

                    if (!isBeingDeclared && !tablaSimbolos.ContainsKey(token.Lexema))
                    {
                        ErroresSemanticos.Add($"Semántico: Variable '{token.Lexema}' no declarada (Línea {token.Linea})");
                    }
                }
            }
        }
    }
}