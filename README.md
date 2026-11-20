# Unidad 6 · Métodos de búsqueda

Aplicación en C# con búsqueda secuencial, búsqueda binaria iterativa y tabla hash propia por encadenamiento separado.

## Abrir en Windows

1. Descomprime la carpeta completa y abre `BusquedasU6.sln` en Visual Studio 2022 o posterior.
2. Instala la carga **Desarrollo de escritorio de .NET** y un SDK de .NET 8 si Visual Studio los solicita.
3. Selecciona **WinForms** como proyecto de inicio y ejecuta con **Ctrl+F5**.
4. Elige un tamaño y pulsa **Generar y medir**. La clave sugerida pertenece al conjunto. **Buscar clave** muestra el conteo de cada método y **Colisión de ejemplo** recorre una cadena con tres claves. **Exportar CSV** guarda las seis filas del tamaño activo.

Desde PowerShell también puedes ejecutar `dotnet run --project WinForms/WinForms.csproj -c Release`.

## Reproducir toda la práctica

```powershell
dotnet run --project BancoConsola/BancoConsola.csproj -c Release -- Resultados_U6.csv
```

La corrida cubre 100, 1 000 y 10 000 claves. Usa una semilla de 2026 + N por tamaño, 100 consultas presentes seleccionadas sin reemplazo y 100 ausentes. Cada consulta pasa por los tres métodos. Los contadores solo incluyen la búsqueda, con una comparación por clave visitada y una iteración por clave visitada. Hash suma una iteración por acceder a la cubeta. La verificación comprueba si cada respuesta coincide con el resultado esperado.

`Evidencias` contiene el CSV y la salida de la consola usados para el reporte. El código de los métodos está en `Core/Busquedas.cs` y `Core/TablaHash.cs`. La generación, verificación y exportación están en `Core/Experimento.cs`.
