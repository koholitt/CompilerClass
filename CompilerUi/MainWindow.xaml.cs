using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using AccesoDatos;
using CompilerClass.Models;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows.Controls; // For TreeView

namespace CompilerUi
{
    // Basic Node class to support the Tree structure
    public class Nodo
    {
        public string valor { get; set; }
        public List<Nodo> hijos { get; set; } = new List<Nodo>();
        public Nodo(string v) { valor = v; }
    }

    public partial class MainWindow : Window
    {
        private LexicAnalizer _analizador = new LexicAnalizer();

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
            List<Tokens> listaTokens = _analizador.Analizar(codigo);

            dgTokens.ItemsSource = null;
            dgTokens.ItemsSource = listaTokens;

            ActualizarListasDinamicas(listaTokens);

            // --- Merged Tree Logic ---
            treeView1.Items.Clear(); // Clear previous tree
            Nodo raiz = ConstruirArbol(listaTokens);

            TreeViewItem nodoRaizUI = new TreeViewItem { Header = raiz.valor };
            treeView1.Items.Add(nodoRaizUI);

            MostrarArbol(raiz, nodoRaizUI);
            // In WPF, expansion is usually handled via ItemContainerStyle or manual loop
        }

        // Logic merged from snippet 1: Creates the hierarchy
        public Nodo ConstruirArbol(List<Tokens> tokens)
        {
            Nodo raiz = new Nodo("Programa");
            Stack<Nodo> pila = new Stack<Nodo>();
            pila.Push(raiz);

            foreach (var token in tokens)
            {
                Nodo nuevo = new Nodo(token.Lexema);
                pila.Peek().hijos.Add(nuevo);

                // Logic from snippet 1: nesting logic
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

        // Logic merged from snippet 1: Populates the UI TreeView
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
            lstErrores.Items.Clear();

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
                        if (!lstErrores.Items.Contains(token.Lexema))
                            lstErrores.Items.Add(token.Lexema);
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