using System.Text;

namespace BusquedasU6;

public sealed record Fila(
    int N, string Caso, string Metodo, int Consultas,
    long ComparacionesTotal, long IteracionesTotal)
{
    public double ComparacionesPromedio => (double)ComparacionesTotal / Consultas;
    public double IteracionesPromedio => (double)IteracionesTotal / Consultas;
}

public sealed class Experimento
{
    private const int ConsultasPorCaso = 100;
    private static readonly string[] metodos = ["Secuencial", "Binaria", "Hash"];
    private readonly int[] datos;
    private readonly int[] ordenados;
    private readonly int[] presentes;
    private readonly int[] ausentes;

    public int N => datos.Length;
    public int Semilla => 2026 + N;
    public int EjemploPresente => presentes[0];
    public TablaHash Hash { get; }

    public Experimento(int n)
    {
        if (n < ConsultasPorCaso || n > 10_000)
            throw new ArgumentOutOfRangeException(nameof(n));

        var azar = new Random(2026 + n);
        int rango = 1000 * n;
        datos = GenerarDistintos(n, azar, rango, paridad: 0);
        ausentes = GenerarDistintos(ConsultasPorCaso, azar, rango, paridad: 1);

        ordenados = (int[])datos.Clone();
        Array.Sort(ordenados);
        Hash = new TablaHash(datos, 2 * n + 1);

        // Muestreo sin reemplazo mediante las primeras rondas de Fisher–Yates.
        var muestra = (int[])datos.Clone();
        for (int i = 0; i < ConsultasPorCaso; i++)
        {
            int j = azar.Next(i, muestra.Length);
            (muestra[i], muestra[j]) = (muestra[j], muestra[i]);
        }
        presentes = muestra[..ConsultasPorCaso];
    }

    private static int[] GenerarDistintos(int cantidad, Random azar, int rango, int paridad)
    {
        var vistos = new HashSet<int>();
        var valores = new int[cantidad];
        int i = 0;
        while (i < cantidad)
        {
            int valor = 2 * azar.Next(-rango, rango) + paridad;
            if (vistos.Add(valor))
                valores[i++] = valor;
        }
        return valores;
    }

    public Medicion[] Buscar(int clave) =>
    [
        Busquedas.Secuencial(datos, clave),
        Busquedas.Binaria(ordenados, clave),
        Hash.Buscar(clave)
    ];

    public IReadOnlyList<Fila> Medir() =>
        MedirCaso(presentes, true).Concat(MedirCaso(ausentes, false)).ToArray();

    private IEnumerable<Fila> MedirCaso(int[] claves, bool esperada)
    {
        var comparaciones = new long[metodos.Length];
        var iteraciones = new long[metodos.Length];

        foreach (int clave in claves)
        {
            Medicion[] resultados = Buscar(clave);
            for (int i = 0; i < resultados.Length; i++)
            {
                Medicion resultado = resultados[i];
                if (resultado.Encontrada != esperada)
                    throw new InvalidOperationException($"{metodos[i]} dio un resultado incorrecto para {clave}.");
                comparaciones[i] += resultado.Comparaciones;
                iteraciones[i] += resultado.Iteraciones;
            }
        }

        string caso = esperada ? "Presente" : "Ausente";
        for (int i = 0; i < metodos.Length; i++)
            yield return new Fila(N, caso, metodos[i], claves.Length, comparaciones[i], iteraciones[i]);
    }

    public static void ExportarCsv(string ruta, IEnumerable<Fila> filas)
    {
        using var escritor = new StreamWriter(ruta, append: false, new UTF8Encoding(true));
        escritor.WriteLine("n,caso,metodo,consultas,comparaciones_total,iteraciones_total,comparaciones_promedio,iteraciones_promedio");
        foreach (Fila f in filas)
        {
            escritor.WriteLine(FormattableString.Invariant(
                $"{f.N},{f.Caso},{f.Metodo},{f.Consultas},{f.ComparacionesTotal},{f.IteracionesTotal},{f.ComparacionesPromedio:F2},{f.IteracionesPromedio:F2}"));
        }
    }
}
