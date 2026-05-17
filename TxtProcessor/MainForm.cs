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
        // Controles de la interfaz
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

        // Variables de estado
        private string rutaArchivoActual = string.Empty;
        private bool haycambios = false;

        // 1. CONSTRUCTOR ORIGINAL: Usado cuando abres el bloc de notas por sí solo
        public MainForm()
        {
            InicializarComponentes();
            ActualizarStatus();
        }

        // 2. NUEVO CONSTRUCTOR SOBRECARGADO: Usado por tu explorador de archivos principal
        public MainForm(string ruta)
        {
            InicializarComponentes();

            // Usamos el evento Load para garantizar que la UI exista en memoria
            // antes de inyectar las líneas de texto del archivo
            this.Load += (s, e) => CargarArchivoDirecto(ruta);
        }

        // 3. NUEVO MÉTODO: Centraliza la lógica de lectura directa sin cuadros de diálogo
        public void CargarArchivoDirecto(string ruta)
        {
            try
            {
                rutaArchivoActual = ruta;

                // Leemos todo el contenido del archivo de texto plano
                rtbContenido.Text = File.ReadAllText(ruta, Encoding.UTF8);

                // Al cargar un archivo limpio, reseteamos el estado de modificación
                haycambios = false;

                // Actualizamos los elementos visuales con la información cargada
                lblArchivo.Text = $"Archivo: {rutaArchivoActual}";
                ActualizarStatus();
                HabilitarControles(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el archivo de texto:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── DISEÑO Y CONSTRUCCIÓN DE INTERFAZ ──────────────────────────────────
        private void InicializarComponentes()
        {
            this.Text = "Procesador de Archivos de Texto";
            this.Size = new System.Drawing.Size(900, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.FormClosing += MainForm_FormClosing;

            // Panel superior para la botonera
            panelBotones = new Panel();
            panelBotones.Dock = DockStyle.Top;
            panelBotones.Height = 45;
            panelBotones.Padding = new Padding(5);

            btnNuevo = new Button();
            btnNuevo.Text = "Nuevo";
            btnNuevo.Location = new System.Drawing.Point(5, 8);
            btnNuevo.Size = new System.Drawing.Size(100, 28);
            btnNuevo.Click += BtnNuevo_Click;

            btnCargar = new Button();
            btnCargar.Text = "Abrir Archivo";
            btnCargar.Location = new System.Drawing.Point(115, 8);
            btnCargar.Size = new System.Drawing.Size(100, 28);
            btnCargar.Click += BtnCargar_Click;

            btnGuardar = new Button();
            btnGuardar.Text = "Guardar";
            btnGuardar.Location = new System.Drawing.Point(225, 8);
            btnGuardar.Size = new System.Drawing.Size(100, 28);
            btnGuardar.Enabled = false;
            btnGuardar.Click += BtnGuardar_Click;

            btnGuardarComo = new Button();
            btnGuardarComo.Text = "Guardar Como";
            btnGuardarComo.Location = new System.Drawing.Point(335, 8);
            btnGuardarComo.Size = new System.Drawing.Size(110, 28);
            btnGuardarComo.Enabled = false;
            btnGuardarComo.Click += BtnGuardarComo_Click;

            btnBuscarReemplazar = new Button();
            btnBuscarReemplazar.Text = "Buscar / Reemplazar";
            btnBuscarReemplazar.Location = new System.Drawing.Point(455, 8);
            btnBuscarReemplazar.Size = new System.Drawing.Size(140, 28);
            btnBuscarReemplazar.Enabled = false;
            btnBuscarReemplazar.Click += BtnBuscarReemplazar_Click;

            btnEstadisticas = new Button();
            btnEstadisticas.Text = "Estadísticas";
            btnEstadisticas.Location = new System.Drawing.Point(605, 8);
            btnEstadisticas.Size = new System.Drawing.Size(100, 28);
            btnEstadisticas.Enabled = false;
            btnEstadisticas.Click += BtnEstadisticas_Click;

            panelBotones.Controls.AddRange(new Control[] { btnNuevo, btnCargar, btnGuardar, btnGuardarComo, btnBuscarReemplazar, btnEstadisticas });

            // Etiqueta indicadora del archivo actual
            lblArchivo = new Label();
            lblArchivo.Dock = DockStyle.Top;
            lblArchivo.Height = 22;
            lblArchivo.Padding = new Padding(5, 3, 0, 0);
            lblArchivo.Text = "Ningún archivo activo";

            // Control de edición de texto enriquecido / plano
            rtbContenido = new RichTextBox();
            rtbContenido.Dock = DockStyle.Fill;
            rtbContenido.Font = new System.Drawing.Font("Segoe UI", 11F);
            rtbContenido.TextChanged += RtbContenido_TextChanged;
            rtbContenido.SelectionChanged += RtbContenido_SelectionChanged;

            // Barra de estado inferior
            lblStatus = new Label();
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 22;
            lblStatus.Padding = new Padding(5, 3, 0, 0);
            lblStatus.Text = "Listo";
            lblStatus.BorderStyle = BorderStyle.Fixed3D;

            // Ensamblaje de controles en el formulario principal
            this.Controls.Add(rtbContenido);
            this.Controls.Add(lblArchivo);
            this.Controls.Add(panelBotones);
            this.Controls.Add(lblStatus);
        }

        // ─── ACCIONES DE BOTONES ──────────────────────────────────────────────────
        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            if (VerificarCambiosSinGuardar())
            {
                rtbContenido.Clear();
                rutaArchivoActual = string.Empty;
                haycambios = false;
                lblArchivo.Text = "Nuevo documento de texto";
                ActualizarStatus();
                HabilitarControles(true);
            }
        }

        private void BtnCargar_Click(object sender, EventArgs e)
        {
            if (VerificarCambiosSinGuardar())
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
                    ofd.Title = "Seleccionar archivo de texto";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        CargarArchivoDirecto(ofd.FileName);
                    }
                }
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(rutaArchivoActual))
            {
                BtnGuardarComo_Click(sender, e);
            }
            else
            {
                try
                {
                    File.WriteAllText(rutaArchivoActual, rtbContenido.Text, Encoding.UTF8);
                    haycambios = false;
                    ActualizarStatus();
                    MessageBox.Show("Archivo guardado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnGuardarComo_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Archivos de texto (*.txt)|*.txt";
                sfd.Title = "Guardar como...";
                if (!string.IsNullOrEmpty(rutaArchivoActual))
                    sfd.FileName = Path.GetFileName(rutaArchivoActual);

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    rutaArchivoActual = sfd.FileName;
                    BtnGuardar_Click(sender, e);
                    lblArchivo.Text = $"Archivo: {rutaArchivoActual}";
                }
            }
        }

        private void BtnBuscarReemplazar_Click(object sender, EventArgs e)
        {
            // Instanciamos pasándole la referencia del RichTextBox de este formulario
            using (BuscarForm formBusqueda = new BuscarForm(rtbContenido))
            {
                formBusqueda.ShowDialog(this);
            }
        }

        private void BtnEstadisticas_Click(object sender, EventArgs e)
        {
            int palabras = ContarPalabras(rtbContenido.Text);
            int caracteres = rtbContenido.Text.Length;
            int lineas = rtbContenido.Lines.Length;

            MessageBox.Show($"Estadísticas del documento:\n\n" +
                            $"• Total de Líneas: {lineas}\n" +
                            $"• Total de Palabras: {palabras}\n" +
                            $"• Total de Caracteres: {caracteres}",
                            "Estadísticas del Texto", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ─── CONTROL DE EVENTOS INTERNOS Y MÉTODOS AUXILIARES ─────────────────────
        private void RtbContenido_TextChanged(object sender, EventArgs e)
        {
            haycambios = true;
            ActualizarStatus();
        }

        private void rtbContenido_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarStatus();
        }

        private void RtbContenido_SelectionChanged(object sender, EventArgs e)
        {
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

        private int ContarPalabras(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return 0;
            return Regex.Matches(texto, @"\b\w+\b").Count;
        }

        private bool VerificarCambiosSinGuardar()
        {
            if (!haycambios) return true;

            DialogResult r = MessageBox.Show(
                "Hay cambios sin guardar. ¿Deseas guardar antes de continuar?",
                "Cambios sin guardar", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            if (r == DialogResult.Yes) { BtnGuardar_Click(null, null); return !haycambios; }
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