using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TxtProcessor
{
    public class MainForm : Form
    {
        private Button btnNuevo;
        private Button btnCargar;
        private Button btnGuardar;
        private Button btnGuardarComo;
        private Button btnBuscarReemplazar;
        private Button btnEstadisticas;
        private Label lblArchivo;
        private Label lblStatus;
        private RichTextBox rtbContenido;
        private Panel panelBotones;

        private string rutaArchivoActual = string.Empty;
        private bool haycambios = false;

        public MainForm()
        {
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            this.Text = "Procesador de Archivos de Texto";
            this.Size = new System.Drawing.Size(900, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.FormClosing += MainForm_FormClosing;

            // Panel botones
            panelBotones = new Panel();
            panelBotones.Dock = DockStyle.Top;
            panelBotones.Height = 45;
            panelBotones.Padding = new Padding(5);

            btnNuevo = new Button();
            btnNuevo.Text = "Nuevo";
            btnNuevo.Location = new System.Drawing.Point(5, 8);
            btnNuevo.Size = new System.Drawing.Size(75, 28);
            btnNuevo.Click += BtnNuevo_Click;

            btnCargar = new Button();
            btnCargar.Text = "Abrir";
            btnCargar.Location = new System.Drawing.Point(90, 8);
            btnCargar.Size = new System.Drawing.Size(75, 28);
            btnCargar.Click += BtnCargar_Click;

            btnGuardar = new Button();
            btnGuardar.Text = "Guardar";
            btnGuardar.Location = new System.Drawing.Point(175, 8);
            btnGuardar.Size = new System.Drawing.Size(75, 28);
            btnGuardar.Enabled = false;
            btnGuardar.Click += BtnGuardar_Click;

            btnGuardarComo = new Button();
            btnGuardarComo.Text = "Guardar como";
            btnGuardarComo.Location = new System.Drawing.Point(260, 8);
            btnGuardarComo.Size = new System.Drawing.Size(100, 28);
            btnGuardarComo.Enabled = false;
            btnGuardarComo.Click += BtnGuardarComo_Click;

            btnBuscarReemplazar = new Button();
            btnBuscarReemplazar.Text = "Buscar / Reemplazar";
            btnBuscarReemplazar.Location = new System.Drawing.Point(370, 8);
            btnBuscarReemplazar.Size = new System.Drawing.Size(145, 28);
            btnBuscarReemplazar.Enabled = false;
            btnBuscarReemplazar.Click += BtnBuscarReemplazar_Click;

            btnEstadisticas = new Button();
            btnEstadisticas.Text = "Estadísticas";
            btnEstadisticas.Location = new System.Drawing.Point(525, 8);
            btnEstadisticas.Size = new System.Drawing.Size(95, 28);
            btnEstadisticas.Enabled = false;
            btnEstadisticas.Click += BtnEstadisticas_Click;

            panelBotones.Controls.AddRange(new Control[] {
                btnNuevo, btnCargar, btnGuardar, btnGuardarComo,
                btnBuscarReemplazar, btnEstadisticas
            });

            // Label archivo
            lblArchivo = new Label();
            lblArchivo.Dock = DockStyle.Top;
            lblArchivo.Height = 22;
            lblArchivo.Padding = new Padding(5, 3, 0, 0);
            lblArchivo.Text = "Ningún archivo abierto";

            // Editor de texto
            rtbContenido = new RichTextBox();
            rtbContenido.Dock = DockStyle.Fill;
            rtbContenido.Font = new System.Drawing.Font("Consolas", 10f);
            rtbContenido.ScrollBars = RichTextBoxScrollBars.Both;
            rtbContenido.WordWrap = false;
            rtbContenido.AcceptsTab = true;
            rtbContenido.TextChanged += RtbContenido_TextChanged;

            // Label status
            lblStatus = new Label();
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 22;
            lblStatus.Padding = new Padding(5, 3, 0, 0);
            lblStatus.Text = "Listo";
            lblStatus.BorderStyle = BorderStyle.Fixed3D;

            this.Controls.Add(rtbContenido);
            this.Controls.Add(lblArchivo);
            this.Controls.Add(panelBotones);
            this.Controls.Add(lblStatus);
        }

        // ─── NUEVO ────────────────────────────────────────────────────────────────
        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            if (!VerificarCambiosSinGuardar()) return;

            rtbContenido.Clear();
            rutaArchivoActual = string.Empty;
            haycambios = false;
            lblArchivo.Text = "Nuevo archivo (sin guardar)";
            HabilitarControles(true);
            ActualizarStatus();
        }

        // ─── ABRIR ────────────────────────────────────────────────────────────────
        private void BtnCargar_Click(object sender, EventArgs e)
        {
            if (!VerificarCambiosSinGuardar()) return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
                ofd.Title = "Abrir archivo de texto";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string contenido = File.ReadAllText(ofd.FileName, Encoding.UTF8);
                        rtbContenido.Text = contenido;
                        rutaArchivoActual = ofd.FileName;
                        haycambios = false;
                        lblArchivo.Text = $"Archivo: {rutaArchivoActual}";
                        HabilitarControles(true);
                        ActualizarStatus();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al abrir el archivo:\n{ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ─── GUARDAR ──────────────────────────────────────────────────────────────
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(rutaArchivoActual))
            {
                BtnGuardarComo_Click(sender, e);
                return;
            }

            GuardarArchivo(rutaArchivoActual);
        }

        private void BtnGuardarComo_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Archivos de texto (*.txt)|*.txt";
                sfd.Title = "Guardar archivo de texto";
                sfd.FileName = string.IsNullOrEmpty(rutaArchivoActual)
                    ? "documento.txt"
                    : Path.GetFileName(rutaArchivoActual);

                if (sfd.ShowDialog() == DialogResult.OK)
                    GuardarArchivo(sfd.FileName);
            }
        }

        private void GuardarArchivo(string ruta)
        {
            try
            {
                File.WriteAllText(ruta, rtbContenido.Text, Encoding.UTF8);
                rutaArchivoActual = ruta;
                haycambios = false;
                lblArchivo.Text = $"Archivo: {rutaArchivoActual}";
                lblStatus.Text = $"Guardado: {ruta}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── BUSCAR / REEMPLAZAR ──────────────────────────────────────────────────
        private void BtnBuscarReemplazar_Click(object sender, EventArgs e)
        {
            using (BuscarForm form = new BuscarForm(rtbContenido))
            {
                form.ShowDialog();
                ActualizarStatus();
            }
        }

        // ─── ESTADÍSTICAS ─────────────────────────────────────────────────────────
        private void BtnEstadisticas_Click(object sender, EventArgs e)
        {
            string texto = rtbContenido.Text;

            int totalCaracteres = texto.Length;
            int caracteresSinEspacios = texto.Replace(" ", "").Replace("\n", "").Replace("\r", "").Length;
            int totalLineas = rtbContenido.Lines.Length;
            int lineasNoVacias = rtbContenido.Lines.Count(l => !string.IsNullOrWhiteSpace(l));
            int totalPalabras = ContarPalabras(texto);
            int totalParrafos = texto.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries).Length;

            string msg =
                $"Caracteres (con espacios):   {totalCaracteres}\n" +
                $"Caracteres (sin espacios):   {caracteresSinEspacios}\n" +
                $"Palabras:                    {totalPalabras}\n" +
                $"Líneas totales:              {totalLineas}\n" +
                $"Líneas no vacías:            {lineasNoVacias}\n" +
                $"Párrafos:                    {totalParrafos}";

            MessageBox.Show(msg, "Estadísticas del documento",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private int ContarPalabras(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return 0;
            return Regex.Matches(texto.Trim(), @"\b\w+\b").Count;
        }

        // ─── EVENTOS AUXILIARES ───────────────────────────────────────────────────
        private void RtbContenido_TextChanged(object sender, EventArgs e)
        {
            haycambios = true;
            ActualizarStatus();
        }

        private void ActualizarStatus()
        {
            int linea = rtbContenido.GetLineFromCharIndex(rtbContenido.SelectionStart) + 1;
            int col = rtbContenido.SelectionStart - rtbContenido.GetFirstCharIndexOfCurrentLine() + 1;
            int palabras = ContarPalabras(rtbContenido.Text);
            string modificado = haycambios ? " [modificado]" : "";
            lblStatus.Text = $"Línea: {linea}   Col: {col}   Palabras: {palabras}   Caracteres: {rtbContenido.Text.Length}{modificado}";
        }

        private bool VerificarCambiosSinGuardar()
        {
            if (!haycambios) return true;

            DialogResult r = MessageBox.Show(
                "Hay cambios sin guardar. ¿Deseas guardar antes de continuar?",
                "Cambios sin guardar", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            if (r == DialogResult.Yes) { BtnGuardar_Click(null, null); return true; }
            if (r == DialogResult.No) return true;
            return false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!VerificarCambiosSinGuardar())
                e.Cancel = true;
        }

        private void HabilitarControles(bool estado)
        {
            btnGuardar.Enabled = estado;
            btnGuardarComo.Enabled = estado;
            btnBuscarReemplazar.Enabled = estado;
            btnEstadisticas.Enabled = estado;
        }
    }
}
