namespace BusquedasU6;

public sealed class TablaHash
{
    private readonly int[] cabezas;
    private readonly Entrada[] entradas;

    private readonly record struct Entrada(int Clave, int Siguiente);

    public int Cubetas => cabezas.Length;
    public int Colisiones { get; }
    public int CadenaMaxima { get; }

    // Recibe las claves del conjunto. Cada entrada se antepone en su cubeta.
    public TablaHash(ReadOnlySpan<int> claves, int cubetas)
    {
        if (cubetas < 1)
            throw new ArgumentOutOfRangeException(nameof(cubetas));

        cabezas = new int[cubetas];
        Array.Fill(cabezas, -1);
        entradas = new Entrada[claves.Length];
        int colisiones = 0;

        for (int i = 0; i < claves.Length; i++)
        {
            int cubeta = Dispersar(claves[i]);
            int anterior = cabezas[cubeta];
            if (anterior >= 0)
                colisiones++;

            entradas[i] = new Entrada(claves[i], anterior);
            cabezas[cubeta] = i;
        }
        Colisiones = colisiones;

        int maximo = 0;
        foreach (int cabeza in cabezas)
        {
            int longitud = 0;
            for (int indice = cabeza; indice >= 0; indice = entradas[indice].Siguiente)
                longitud++;
            maximo = Math.Max(maximo, longitud);
        }
        CadenaMaxima = maximo;
    }

    public int Dispersar(int clave)
    {
        int resto = clave % cabezas.Length;
        return resto < 0 ? resto + cabezas.Length : resto;
    }

    public Medicion Buscar(int clave)
    {
        int cubeta = Dispersar(clave);
        int comparaciones = 0;
        // Se cuenta una visita a la cubeta incluso cuando está vacía.
        for (int indice = cabezas[cubeta]; indice >= 0; indice = entradas[indice].Siguiente)
        {
            comparaciones++;
            if (entradas[indice].Clave == clave)
                return new Medicion(true, comparaciones, comparaciones + 1);
        }
        return new Medicion(false, comparaciones, comparaciones + 1);
    }
}
