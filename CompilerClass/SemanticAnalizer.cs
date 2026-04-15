using System;
using System.Collections.Generic;
using CompilerClass.Models;

namespace CompilerClass
{
    public class SemanticAnalizer
    {
        private Dictionary<string, string> symbolTable = new Dictionary<string, string>();
        public List<string> ErroresSemanticos { get; private set; } = new List<string>();

        // Renamed to Validar to match your MainWindow call
        public void Validar(List<Tokens> tokens)
        {
            symbolTable.Clear();
            ErroresSemanticos.Clear();

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                // RULE 1: DECLARATION
                // Changed "Keyword" to "Reservada" to match your LexicAnalizer output
                if (token.Tipo == "Reservada" && IsTypeKeyword(token.Lexema))
                {
                    if (i + 1 < tokens.Count && tokens[i + 1].Tipo == "Identificador")
                    {
                        string varName = tokens[i + 1].Lexema;
                        string varType = token.Lexema;

                        if (symbolTable.ContainsKey(varName))
                        {
                            ErroresSemanticos.Add($"Semántico: La variable '{varName}' ya ha sido declarada. (Línea {token.Linea})");
                        }
                        else
                        {
                            symbolTable.Add(varName, varType);
                        }
                        i++;
                        continue;
                    }
                }

                // RULE 2: ASSIGNMENT
                if (token.Tipo == "Identificador" && i + 1 < tokens.Count && tokens[i + 1].Lexema == "=")
                {
                    string varName = token.Lexema;

                    if (!symbolTable.ContainsKey(varName))
                    {
                        ErroresSemanticos.Add($"Semántico: Uso de variable no declarada '{varName}'. (Línea {token.Linea})");
                    }
                    else if (i + 2 < tokens.Count)
                    {
                        string declaredType = symbolTable[varName];
                        string assignedType = InferType(tokens[i + 2]);

                        if (assignedType != "unknown" && declaredType != assignedType)
                        {
                            ErroresSemanticos.Add($"Semántico: Incompatibilidad de tipos. '{varName}' es {declaredType} pero se asignó {assignedType}. (Línea {token.Linea})");
                        }
                    }
                }

                // RULE 3: GENERAL USAGE
                if (token.Tipo == "Identificador")
                {
                    if (!symbolTable.ContainsKey(token.Lexema))
                    {
                        ErroresSemanticos.Add($"Semántico: La variable '{token.Lexema}' no ha sido declarada. (Línea {token.Linea})");
                    }
                }
            }
        }

        private bool IsTypeKeyword(string lexema)
        {
            return lexema == "int" || lexema == "float" || lexema == "string" || lexema == "bool";
        }

        private string InferType(Tokens token)
        {
            // Matches "Número" from your LexicAnalizer
            if (token.Tipo == "Número")
            {
                return token.Lexema.Contains(".") ? "float" : "int";
            }
            if (token.Tipo == "Cadena") return "string";
            if (token.Lexema == "true" || token.Lexema == "false") return "bool";

            if (token.Tipo == "Identificador" && symbolTable.ContainsKey(token.Lexema))
            {
                return symbolTable[token.Lexema];
            }

            return "unknown";
        }
    }
}