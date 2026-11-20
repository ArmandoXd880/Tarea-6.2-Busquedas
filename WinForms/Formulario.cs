using System.Globalization;

namespace BusquedasU6;

public sealed class Formulario : Form
{
    private readonly ComboBox tamanos = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
    private readonly TextBox clave = new() { Width = 115, AccessibleName = "Clave entera" };
    private readonly Button buscar = new() { Text = "Buscar clave", AutoSize = true, Enabled = false };
    private readonly Button exportar = new() { Text = "Exportar CSV", AutoSize = true, Enabled = false };
    private readonly Label resumen = new() { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 8, 0, 9) };
    private readonly Label detalle = new() { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 6, 0, 8) };
    private readonly TabControl pestanas = new() { Dock = DockStyle.Fill };
    private readonly TabPage lote = new("Resultados del experimento");
    private readonly TabPage individual = new("Búsqueda individual");
    private readonly DataGridView tablaLote = CrearTabla(
        "N", "Caso", "Método", "Consultas", "Comp. promedio", "Iter. promedio");
    private readonly DataGridView tablaIndividual = CrearTabla(
        "Clave", "Método", "Resultado", "Cubeta", "Comparaciones", "Iteraciones");
    private static readonly string[] Metodos = ["Secuencial", "Binaria", "Hash"];

    private Experimento? experimento;
    private IReadOnlyList<Fila> filas = Array.Empty<Fila>();

    public Formulario()
    {
        Text = "Unidad 6 | Banco de búsquedas";
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 10f);
        ClientSize = new Size(940, 520);
        MinimumSize = new Size(800, 470);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;

        tamanos.Items.AddRange(["100", "1 000", "10 000"]);
        tamanos.SelectedIndex = 1;
        tamanos.AccessibleName = "Cantidad de datos";
        var ejecutar = new Button { Text = "Generar y medir", AutoSize = true };
        var colision = new Button { Text = "Colisión de ejemplo", AutoSize = true };

        var controles = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        controles.Controls.AddRange([
            new Label { Text = "Registros", AutoSize = true, Margin = new Padding(0, 8, 5, 0) },
            tamanos, ejecutar, exportar
        ]);
        var consultas = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        consultas.Controls.AddRange([
            new Label { Text = "Clave entera", AutoSize = true, Margin = new Padding(0, 8, 5, 0) },
            clave, buscar, colision
        ]);

        var principal = new TableLayoutPanel
        {
            ColumnCount = 1, RowCount = 5, Dock = DockStyle.Fill, Padding = new Padding(16)
        };
        principal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        principal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        principal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        principal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        principal.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        resumen.Text = "Elige el tamaño y pulsa Generar y medir. También puedes abrir el ejemplo de colisión.";
        principal.Controls.Add(controles, 0, 0);
        principal.Controls.Add(consultas, 0, 1);
        principal.Controls.Add(resumen, 0, 2);
        principal.Controls.Add(pestanas, 0, 3);
        principal.Controls.Add(new Label
        {
            Text = "Cada visita a una clave cuenta como comparación. Hash también cuenta la visita a la cubeta.",
            AutoSize = true,
            Padding = new Padding(0, 9, 0, 0)
        }, 0, 4);

        lote.Padding = new Padding(8);
        lote.Controls.Add(tablaLote);
        var panelDetalle = new TableLayoutPanel { ColumnCount = 1, RowCount = 2, Padding = new Padding(8), Dock = DockStyle.Fill };
        panelDetalle.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panelDetalle.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        detalle.Text = "Aquí aparecerán las mediciones de una clave o del ejemplo de colisión.";
        panelDetalle.Controls.Add(detalle, 0, 0);
        panelDetalle.Controls.Add(tablaIndividual, 0, 1);
        individual.Controls.Add(panelDetalle);
        pestanas.TabPages.AddRange([lote, individual]);
        Controls.Add(principal);

        ejecutar.Click += (_, _) => Generar();
        buscar.Click += (_, _) => BuscarClave();
        colision.Click += (_, _) => MostrarColision();
        exportar.Click += (_, _) => GuardarCsv();
    }

    private void Generar()
    {
        int[] opciones = [100, 1_000, 10_000];
        var nuevo = new Experimento(opciones[tamanos.SelectedIndex]);
        IReadOnlyList<Fila> medidas = nuevo.Medir();

        experimento = nuevo;
        filas = medidas;
        tablaLote.Rows.Clear();
        foreach (Fila fila in filas)
            tablaLote.Rows.Add(fila.N, fila.Caso, fila.Metodo, fila.Consultas,
                fila.ComparacionesPromedio.ToString("F2"), fila.IteracionesPromedio.ToString("F2"));
        tablaIndividual.Rows.Clear();
        detalle.Text = "Busca la clave sugerida o escribe otro entero.";
        resumen.Text = $"N activo: {nuevo.N}   ·   Semilla: {nuevo.Semilla}   ·   Cubetas: {nuevo.Hash.Cubetas}   ·   " +
            $"Colisiones al insertar: {nuevo.Hash.Colisiones}   ·   Cadena máxima: {nuevo.Hash.CadenaMaxima}";
        clave.Text = nuevo.EjemploPresente.ToString(CultureInfo.InvariantCulture);
        buscar.Enabled = true;
        exportar.Enabled = true;
        pestanas.SelectedTab = lote;
    }

    private void BuscarClave()
    {
        if (experimento is null)
            return;
        if (!int.TryParse(clave.Text, out int valor))
        {
            detalle.Text = "Escribe un entero de 32 bits para hacer la búsqueda.";
            tablaIndividual.Rows.Clear();
            pestanas.SelectedTab = individual;
            clave.Focus();
            return;
        }

        tablaIndividual.Rows.Clear();
        Medicion[] resultados = experimento.Buscar(valor);
        for (int i = 0; i < resultados.Length; i++)
        {
            string cubeta = i == 2 ? experimento.Hash.Dispersar(valor).ToString() : "—";
            AgregarDetalle(valor, Metodos[i], cubeta, resultados[i]);
        }
        detalle.Text = $"Consulta al conjunto activo de {experimento.N} registros.";
        pestanas.SelectedTab = individual;
    }

    private void MostrarColision()
    {
        var hash = new TablaHash([1, 6, 11], 5);
        tablaIndividual.Rows.Clear();
        foreach (int valor in new[] { 1, 16, 2 })
            AgregarDetalle(valor, "Hash", hash.Dispersar(valor).ToString(), hash.Buscar(valor));
        detalle.Text = "Ejemplo independiente · 5 cubetas · Cubeta 1: 11 → 6 → 1 · Cubeta 2 vacía.";
        pestanas.SelectedTab = individual;
    }

    private void AgregarDetalle(int valor, string metodo, string cubeta, Medicion r) =>
        tablaIndividual.Rows.Add(valor, metodo, r.Encontrada ? "Presente" : "Ausente",
            cubeta, r.Comparaciones, r.Iteraciones);

    private void GuardarCsv()
    {
        using var dialogo = new SaveFileDialog { Filter = "Archivo CSV (*.csv)|*.csv", FileName = "Resultados_U6.csv" };
        if (dialogo.ShowDialog(this) != DialogResult.OK)
            return;
        try
        {
            Experimento.ExportarCsv(dialogo.FileName, filas);
        }
        catch (IOException ex)
        {
            MessageBox.Show(this, ex.Message, "No se pudo guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show(this, ex.Message, "No se pudo guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static DataGridView CrearTabla(params string[] columnas)
    {
        var tabla = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            EnableHeadersVisualStyles = false
        };
        tabla.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(27, 53, 86);
        tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        tabla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        tabla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(239, 244, 249);

        for (int i = 0; i < columnas.Length; i++)
        {
            tabla.Columns.Add($"col{i}", columnas[i]);
            tabla.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        return tabla;
    }
}
