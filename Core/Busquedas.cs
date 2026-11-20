namespace BusquedasU6;

public readonly record struct Medicion(bool Encontrada, int Comparaciones, int Iteraciones);

public static class Busquedas
{
    public static Medicion Secuencial(int[] datos, int clave)
    {
        for (int i = 0; i < datos.Length; i++)
        {
            if (datos[i] == clave)
                return new Medicion(true, i + 1, i + 1);
        }
        return new Medicion(false, datos.Length, datos.Length);
    }

    public static Medicion Binaria(int[] datosOrdenados, int clave)
    {
        int izquierda = 0;
        int derecha = datosOrdenados.Length - 1;
        int pasos = 0;
        while (izquierda <= derecha)
        {
            int medio = izquierda + (derecha - izquierda) / 2;
            // Una comparación de claves por visita, con tres resultados posibles.
            int comparacion = datosOrdenados[medio].CompareTo(clave);
            pasos++;
            if (comparacion == 0)
                return new Medicion(true, pasos, pasos);
            if (comparacion < 0)
                izquierda = medio + 1;
            else
                derecha = medio - 1;
        }
        return new Medicion(false, pasos, pasos);
    }
}
