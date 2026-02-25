using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TxtProcessor
{
    public class BuscarForm : Form
    {
        private Label lblBuscar;
        private TextBox txtBuscar;
        private Label lblReemplazar;
        private TextBox txtReemplazar;
        private CheckBox chkMatchCase;
        private CheckBox chkPalabraCompleta;
        private Button btnBuscarSiguiente;
        private Button btnBuscarAnterior;
        private Button btnReemplazar;
        private Button btnReemplazarTodo;
        private Button btnCerrar;
        private Label lblResultado;

        private RichTextBox editor;
        private int ultimaPosicion = 0;

        public BuscarForm(RichTextBox editorReferencia)
        {
            this.editor = editorReferencia;
            InicializarFormulario();
        }

        private void InicializarFormulario()
        {
            this.Text = "Buscar y Reemplazar";
            this.Size = new Size(420, 240);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 12;

            lblBuscar = new Label();
            lblBuscar.Text = "Buscar:";
            lblBuscar.Location = new Point(12, y);
            lblBuscar.AutoSize = true;
            y += 18;

            txtBuscar = new TextBox();
            txtBuscar.Location = new Point(12, y);
            txtBuscar.Size = new Size(385, 22);
            y += 30;

            lblReemplazar = new Label();
            lblReemplazar.Text = "Reemplazar con:";
            lblReemplazar.Location = new Point(12, y);
            lblReemplazar.AutoSize = true;
            y += 18;

            txtReemplazar = new TextBox();
            txtReemplazar.Location = new Point(12, y);
            txtReemplazar.Size = new Size(385, 22);
            y += 30;

            chkMatchCase = new CheckBox();
            chkMatchCase.Text = "Distinguir mayúsculas";
            chkMatchCase.Location = new Point(12, y);
            chkMatchCase.AutoSize = true;

            chkPalabraCompleta = new CheckBox();
            chkPalabraCompleta.Text = "Solo palabra completa";
            chkPalabraCompleta.Location = new Point(175, y);
            chkPalabraCompleta.AutoSize = true;
            y += 28;

            lblResultado = new Label();
            lblResultado.Location = new Point(12, y);
            lblResultado.Size = new Size(390, 18);
            lblResultado.ForeColor = Color.DarkBlue;
            y += 22;

            // Botones
            btnBuscarAnterior = new Button();
            btnBuscarAnterior.Text = "← Anterior";
            btnBuscarAnterior.Location = new Point(12, y);
            btnBuscarAnterior.Size = new Size(90, 26);
            btnBuscarAnterior.Click += BtnBuscarAnterior_Click;

            btnBuscarSiguiente = new Button();
            btnBuscarSiguiente.Text = "Siguiente →";
            btnBuscarSiguiente.Location = new Point(112, y);
            btnBuscarSiguiente.Size = new Size(90, 26);
            btnBuscarSiguiente.Click += BtnBuscarSiguiente_Click;

            btnReemplazar = new Button();
            btnReemplazar.Text = "Reemplazar";
            btnReemplazar.Location = new Point(212, y);
            btnReemplazar.Size = new Size(90, 26);
            btnReemplazar.Click += BtnReemplazar_Click;

            btnReemplazarTodo = new Button();
            btnReemplazarTodo.Text = "Reemplazar todo";
            btnReemplazarTodo.Location = new Point(310, y);
            btnReemplazarTodo.Size = new Size(105, 26);
            btnReemplazarTodo.Click += BtnReemplazarTodo_Click;

            this.ClientSize = new Size(425, y + 38);

            btnCerrar = new Button();
            btnCerrar.Text = "Cerrar";
            btnCerrar.Size = new Size(75, 26);
            btnCerrar.Location = new Point(this.ClientSize.Width - 87, 8);
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblBuscar, txtBuscar,
                lblReemplazar, txtReemplazar,
                chkMatchCase, chkPalabraCompleta,
                lblResultado,
                btnBuscarAnterior, btnBuscarSiguiente,
                btnReemplazar, btnReemplazarTodo,
                btnCerrar
            });

            this.AcceptButton = btnBuscarSiguiente;
            txtBuscar.TextChanged += (s, e) => { ultimaPosicion = 0; lblResultado.Text = ""; };
        }

        // ─── BUSCAR SIGUIENTE ─────────────────────────────────────────────────────
        private void BtnBuscarSiguiente_Click(object sender, EventArgs e)
        {
            Buscar(true);
        }

        private void BtnBuscarAnterior_Click(object sender, EventArgs e)
        {
            Buscar(false);
        }

        private void Buscar(bool haciaAdelante)
        {
            string termino = txtBuscar.Text;
            if (string.IsNullOrEmpty(termino)) return;

            string texto = editor.Text;
            StringComparison comparacion = chkMatchCase.Checked
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            int inicio = haciaAdelante
                ? (ultimaPosicion < texto.Length ? ultimaPosicion : 0)
                : Math.Max(0, ultimaPosicion - termino.Length - 1);

            int pos = -1;

            if (haciaAdelante)
            {
                pos = texto.IndexOf(termino, inicio, comparacion);
                if (pos == -1) // Vuelve al inicio
                    pos = texto.IndexOf(termino, 0, comparacion);
            }
            else
            {
                pos = texto.LastIndexOf(termino, inicio, comparacion);
                if (pos == -1) // Va al final
                    pos = texto.LastIndexOf(termino, comparacion);
            }

            if (pos >= 0 && VerificarPalabraCompleta(texto, pos, termino.Length))
            {
                editor.Select(pos, termino.Length);
                editor.ScrollToCaret();
                ultimaPosicion = haciaAdelante ? pos + termino.Length : pos;
                lblResultado.ForeColor = System.Drawing.Color.DarkBlue;
                lblResultado.Text = $"Encontrado en posición {pos + 1}.";
            }
            else
            {
                lblResultado.ForeColor = System.Drawing.Color.Red;
                lblResultado.Text = "No se encontró el texto.";
            }
        }

        private bool VerificarPalabraCompleta(string texto, int pos, int longitud)
        {
            if (!chkPalabraCompleta.Checked) return true;

            bool izq = pos == 0 || !char.IsLetterOrDigit(texto[pos - 1]);
            bool der = (pos + longitud) >= texto.Length || !char.IsLetterOrDigit(texto[pos + longitud]);
            return izq && der;
        }

        // ─── REEMPLAZAR ───────────────────────────────────────────────────────────
        private void BtnReemplazar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBuscar.Text)) return;

            if (editor.SelectedText.Equals(txtBuscar.Text,
                chkMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
            {
                editor.SelectedText = txtReemplazar.Text;
                lblResultado.ForeColor = System.Drawing.Color.DarkGreen;
                lblResultado.Text = "Texto reemplazado.";
            }

            Buscar(true);
        }

        // ─── REEMPLAZAR TODO ──────────────────────────────────────────────────────
        private void BtnReemplazarTodo_Click(object sender, EventArgs e)
        {
            string termino = txtBuscar.Text;
            if (string.IsNullOrEmpty(termino)) return;

            RegexOptions opciones = chkMatchCase.Checked
                ? RegexOptions.None
                : RegexOptions.IgnoreCase;

            string patron = chkPalabraCompleta.Checked
                ? $@"\b{Regex.Escape(termino)}\b"
                : Regex.Escape(termino);

            string textoOriginal = editor.Text;
            string textoNuevo = Regex.Replace(textoOriginal, patron, txtReemplazar.Text, opciones);
            int cuenta = Regex.Matches(textoOriginal, patron, opciones).Count;

            editor.Text = textoNuevo;
            ultimaPosicion = 0;

            lblResultado.ForeColor = System.Drawing.Color.DarkGreen;
            lblResultado.Text = $"{cuenta} reemplazo(s) realizados.";
        }
    }
}
