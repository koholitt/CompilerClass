using System;
using System.Collections.Generic;

namespace CompilerClass
{
    public class SemanticAnalyzer
    {
        private Dictionary<string, string> symbolTable;

        public SemanticAnalyzer()
        {
            symbolTable = new Dictionary<string, string>();
        }

        public void AnalyzeTokens(List<Tokens> tokens)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                // Regla 1: Declaración de variables
                if (token.Tipo == "Keyword" && 
                    (token.Valor == "int" || token.Valor == "string" || token.Valor == "bool"))
                {
                    if (i + 1 < tokens.Count && tokens[i + 1].Tipo == "Identifier")
                    {
                        string varName = tokens[i + 1].Valor;
                        string varType = token.Valor;

                        if (symbolTable.ContainsKey(varName))
                            throw new Exception($"Error semántico: La variable '{varName}' ya está declarada.");

                        symbolTable[varName] = varType;
                    }
                }

                // Regla 2: Asignación de variables
                if (token.Tipo == "Identifier" && i + 1 < tokens.Count && tokens[i + 1].Valor == "=")
                {
                    string varName = token.Valor;

                    if (!symbolTable.ContainsKey(varName))
                        throw new Exception($"Error semántico: La variable '{varName}' no está declarada.");

                    if (i + 2 < tokens.Count)
                    {
                        string declaredType = symbolTable[varName];
                        string assignedType = InferType(tokens[i + 2]);

                        if (declaredType != assignedType)
                            throw new Exception($"Error semántico: Tipo incompatible. '{varName}' es {declaredType}, no {assignedType}.");
                    }
                }

                // Regla 3: Uso de variables
                if (token.Tipo == "Identifier" && (i == 0 || tokens[i - 1].Valor != "int"))
                {
                    if (!symbolTable.ContainsKey(token.Valor))
                        throw new Exception($"Error semántico: La variable '{token.Valor}' no está declarada.");
                }
            }
        }

        // Método auxiliar para deducir tipo de un literal
        private string InferType(Tokens token)
        {
            if (token.Tipo == "Literal")
            {
                if (int.TryParse(token.Valor, out _)) return "int";
                if (token.Valor.StartsWith("\"") && token.Valor.EndsWith("\"")) return "string";
                if (token.Valor == "true" || token.Valor == "false") return "bool";
            }
            return "unknown";
        }
    }
}
