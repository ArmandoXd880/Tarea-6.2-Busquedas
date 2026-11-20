using BusquedasU6;

int[] tamanos = [100, 1_000, 10_000];
var filas = new List<Fila>();

foreach (int n in tamanos)
{
    var prueba = new Experimento(n);
    filas.AddRange(prueba.Medir());
    Console.WriteLine($"N={n} semilla={prueba.Semilla} cubetas={prueba.Hash.Cubetas} " +
        $"colisiones={prueba.Hash.Colisiones} cadena_maxima={prueba.Hash.CadenaMaxima}");
}

var colision = new TablaHash([1, 6, 11], 5);
foreach (int clave in new[] { 1, 16, 2 })
{
    Medicion r = colision.Buscar(clave);
    Console.WriteLine($"Clave={clave} cubeta={colision.Dispersar(clave)} encontrada={r.Encontrada} " +
        $"comparaciones={r.Comparaciones} iteraciones={r.Iteraciones}");
}

Experimento.ExportarCsv(args.Length > 0 ? args[0] : "Resultados_U6.csv", filas);
Console.WriteLine($"{filas.Sum(f => f.Consultas)} búsquedas verificadas. {filas.Count} filas de resumen.");
