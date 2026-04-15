using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using AccesoDatos;
using CompilerClass;
using CompilerClass.Models;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows.Controls;

namespace CompilerUi
{
    public class Nodo
    {
        public string valor { get; set; }
        public List<Nodo> hijos { get; set; } = new List<Nodo>();
        public Nodo(string v) { valor = v; }
    }

    public partial class MainWindow : Window
    {
        private LexicAnalizer _analizador = new LexicAnalizer();
        private SemanticAnalizer _semantico = new SemanticAnalizer(); // New instance

        public MainWindow()
        {
            InitializeComponent();
            txtCodeEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            ApplyVsCodeColors();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            lstReservadas.Items.Clear();
            lstOperadores.Items.Clear();
            lstDelimitadores.Items.Clear();

            foreach (var r in _analizador.PalabrasReservadas) lstReservadas.Items.Add(r);
            foreach (var op in _analizador.Operadores) lstOperadores.Items.Add(op);
            foreach (var d in _analizador.Delimitadores) lstDelimitadores.Items.Add(d);
        }

        private void BtnAnalizar_Click(object sender, RoutedEventArgs e)
        {
            string codigo = txtCodeEditor.Text;

            // 1. Clear previous errors and UI state
            lstErrores.Items.Clear();
            treeView1.Items.Clear();

            // 2. Lexical Analysis
            List<Tokens> listaTokens = _analizador.Analizar(codigo);
            dgTokens.ItemsSource = null;
            dgTokens.ItemsSource = listaTokens;

            // 3. Semantic Analysis
            _semantico.Validar(listaTokens);

            // 4. Update UI Lists (Lexical)
            ActualizarListasDinamicas(listaTokens);

            // 5. Add Semantic Errors to the error box
            foreach (var error in _semantico.ErroresSemanticos)
            {
                lstErrores.Items.Add(error);
            }

            // 6. Build and Show Tree
            Nodo raiz = ConstruirArbol(listaTokens);
            TreeViewItem nodoRaizUI = new TreeViewItem { Header = raiz.valor };
            treeView1.Items.Add(nodoRaizUI);
            MostrarArbol(raiz, nodoRaizUI);
        }

        public Nodo ConstruirArbol(List<Tokens> tokens)
        {
            Nodo raiz = new Nodo("Programa");
            Stack<Nodo> pila = new Stack<Nodo>();
            pila.Push(raiz);

            foreach (var token in tokens)
            {
                Nodo nuevo = new Nodo(token.Lexema);
                pila.Peek().hijos.Add(nuevo);

                if (token.Lexema == "(" || token.Lexema == "{")
                {
                    pila.Push(nuevo);
                }
                else if (token.Lexema == ")" || token.Lexema == "}")
                {
                    if (pila.Count > 1) pila.Pop();
                }
            }
            return raiz;
        }

        void MostrarArbol(Nodo nodo, TreeViewItem treeNodeUI)
        {
            foreach (var hijo in nodo.hijos)
            {
                TreeViewItem nuevoUI = new TreeViewItem { Header = hijo.valor };
                treeNodeUI.Items.Add(nuevoUI);
                MostrarArbol(hijo, nuevoUI);
            }
        }

        private void ActualizarListasDinamicas(List<Tokens> tokens)
        {
            lstIdentificadores.Items.Clear();
            lstNumeros.Items.Clear();
            // Note: lstErrores is cleared at the start of BtnAnalizar_Click

            foreach (var token in tokens)
            {
                switch (token.Tipo)
                {
                    case "Identificador":
                        if (!lstIdentificadores.Items.Contains(token.Lexema))
                            lstIdentificadores.Items.Add(token.Lexema);
                        break;
                    case "Número":
                        if (!lstNumeros.Items.Contains(token.Lexema))
                            lstNumeros.Items.Add(token.Lexema);
                        break;
                    case "Error":
                        // Add lexical errors (invalid characters) to the box
                        lstErrores.Items.Add($"Léxico: Caracter inválido '{token.Lexema}' (Línea {token.Linea})");
                        break;
                }
            }
        }

        private void ApplyVsCodeColors()
        {
            var highlighting = txtCodeEditor.SyntaxHighlighting;
            if (highlighting == null) return;

            void SetColor(string name, Color color)
            {
                var colorDefinition = highlighting.GetNamedColor(name);
                if (colorDefinition != null)
                {
                    colorDefinition.Foreground = new SimpleHighlightingBrush(color);
                }
            }

            SetColor("Keywords", Color.FromRgb(86, 156, 214));
            SetColor("NumberLiteral", Color.FromRgb(181, 206, 168));
            SetColor("String", Color.FromRgb(206, 145, 120));
            SetColor("Comment", Color.FromRgb(106, 153, 85));
            SetColor("MethodName", Color.FromRgb(220, 220, 170));
            SetColor("Punctuation", Color.FromRgb(204, 204, 204));
            SetColor("ValueTypes", Color.FromRgb(197, 134, 192));
        }
    }
}