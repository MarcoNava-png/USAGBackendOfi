using Microsoft.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;

namespace WebApplication2.DataMigrator;

internal static class Program
{
    private static string SrcConn = "";
    private static string DstConn = "";

    private static async Task<int> Main(string[] args)
    {
        var mode = args.Length > 0 ? args[0].ToLowerInvariant() : "audit";
        SrcConn = Environment.GetEnvironmentVariable("SOURCE_CONN")
            ?? throw new InvalidOperationException("Falta variable SOURCE_CONN (SQL Server).");
        DstConn = Environment.GetEnvironmentVariable("DEST_CONN")
            ?? throw new InvalidOperationException("Falta variable DEST_CONN (PostgreSQL).");

        var soloTablas = args.Skip(1).Where(a => !a.StartsWith("--")).ToList();

        try
        {
            return mode switch
            {
                "migrate" => await RunMigrateAsync(soloTablas),
                "audit" => await RunAuditAsync(),
                _ => Fail($"Modo desconocido '{mode}'. Usa: migrate | audit")
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\n❌ ERROR FATAL: {ex.Message}\n{ex}");
            return 1;
        }
    }

    private static int Fail(string msg)
    {
        Console.Error.WriteLine($"❌ {msg}");
        return 1;
    }

    // ─────────────────────────────────────────────────────────────
    //  MIGRACIÓN
    // ─────────────────────────────────────────────────────────────
    private static async Task<int> RunMigrateAsync(List<string> soloTablas)
    {
        Console.WriteLine("=== MIGRACIÓN DE DATOS SQL Server → PostgreSQL ===\n");

        var tablasDst = await GetPostgresTablesAsync();
        if (soloTablas.Count > 0)
            tablasDst = tablasDst.Where(t => soloTablas.Contains(t, StringComparer.OrdinalIgnoreCase)).ToList();

        var tablasSrc = await GetSqlServerTablesAsync();
        var srcSet = new HashSet<string>(tablasSrc, StringComparer.OrdinalIgnoreCase);

        await using var dst = new NpgsqlConnection(DstConn);
        await dst.OpenAsync();

        // Desactivar validación de FKs/triggers durante la carga masiva.
        await using (var cmd = new NpgsqlCommand("SET session_replication_role = replica;", dst))
            await cmd.ExecuteNonQueryAsync();

        // Vaciar TODAS las tablas destino ANTES de copiar (un solo TRUNCATE, evita que
        // un CASCADE posterior borre filas ya copiadas). Idempotente.
        var aTruncar = tablasDst.Where(srcSet.Contains).ToList();
        if (aTruncar.Count > 0)
        {
            var lista = string.Join(", ", aTruncar.Select(t => $"\"{t}\""));
            await using var trunc = new NpgsqlCommand($"TRUNCATE TABLE {lista} CASCADE;", dst);
            await trunc.ExecuteNonQueryAsync();
            Console.WriteLine($"Tablas destino vaciadas: {aTruncar.Count}\n");
        }

        int tablasOk = 0, tablasOmitidas = 0;
        long totalFilas = 0;
        var errores = new List<string>();

        foreach (var tabla in tablasDst)
        {
            if (!srcSet.Contains(tabla))
            {
                Console.WriteLine($"  ⊘ {tabla}: no existe en SQL Server, se omite.");
                tablasOmitidas++;
                continue;
            }

            var colsDst = await GetPostgresColumnsAsync(dst, tabla);
            var colsSrc = await GetSqlServerColumnsAsync(tabla);
            var srcColSet = new HashSet<string>(colsSrc.Keys, StringComparer.OrdinalIgnoreCase);

            // Solo columnas presentes en AMBOS (y no generadas en destino).
            var cols = colsDst.Where(c => srcColSet.Contains(c.Name)).ToList();
            if (cols.Count == 0)
            {
                Console.WriteLine($"  ⊘ {tabla}: sin columnas en común, se omite.");
                tablasOmitidas++;
                continue;
            }

            try
            {
                var filas = await CopyTableAsync(dst, tabla, cols);
                totalFilas += filas;
                tablasOk++;
                Console.WriteLine($"  ✓ {tabla}: {filas} filas");
            }
            catch (Exception ex)
            {
                errores.Add($"{tabla}: {ex.Message}");
                Console.Error.WriteLine($"  ❌ {tabla}: {ex.Message}");
            }
        }

        // Reactivar validación.
        await using (var cmd = new NpgsqlCommand("SET session_replication_role = DEFAULT;", dst))
            await cmd.ExecuteNonQueryAsync();

        Console.WriteLine("\n--- Reajustando secuencias de identidad ---");
        var secuencias = await ResetSequencesAsync(dst);
        Console.WriteLine($"  {secuencias} secuencias reajustadas.");

        Console.WriteLine($"\n=== RESUMEN: {tablasOk} tablas migradas, {tablasOmitidas} omitidas, {totalFilas} filas totales ===");
        if (errores.Count > 0)
        {
            Console.WriteLine($"\n⚠️  {errores.Count} tablas con error:");
            foreach (var e in errores) Console.WriteLine($"   - {e}");
            return 1;
        }
        return 0;
    }

    private static async Task<long> CopyTableAsync(NpgsqlConnection dst, string tabla, List<ColumnInfo> cols)
    {
        var colListSrc = string.Join(", ", cols.Select(c => $"[{c.Name}]"));
        var colListDst = string.Join(", ", cols.Select(c => $"\"{c.Name}\""));

        await using var src = new SqlConnection(SrcConn);
        await src.OpenAsync();
        await using var read = new SqlCommand($"SELECT {colListSrc} FROM [{tabla}]", src) { CommandTimeout = 600 };
        await using var reader = await read.ExecuteReaderAsync();

        long n = 0;
        await using (var writer = await dst.BeginBinaryImportAsync(
            $"COPY \"{tabla}\" ({colListDst}) FROM STDIN (FORMAT BINARY)"))
        {
            while (await reader.ReadAsync())
            {
                await writer.StartRowAsync();
                for (int i = 0; i < cols.Count; i++)
                {
                    var val = reader.GetValue(i);
                    await WriteValueAsync(writer, val, cols[i]);
                }
                n++;
            }
            await writer.CompleteAsync();
        }
        return n;
    }

    private static async Task WriteValueAsync(NpgsqlBinaryImporter w, object val, ColumnInfo col)
    {
        if (val is null || val is DBNull)
        {
            await w.WriteNullAsync();
            return;
        }

        switch (col.PgType)
        {
            case "timestamp with time zone":
                if (val is DateTimeOffset dto)
                    await w.WriteAsync(dto, NpgsqlDbType.TimestampTz);
                else
                    await w.WriteAsync(DateTime.SpecifyKind((DateTime)val, DateTimeKind.Utc), NpgsqlDbType.TimestampTz);
                break;
            case "timestamp without time zone":
                await w.WriteAsync(DateTime.SpecifyKind((DateTime)val, DateTimeKind.Unspecified), NpgsqlDbType.Timestamp);
                break;
            case "date":
                await w.WriteAsync(DateOnly.FromDateTime((DateTime)val), NpgsqlDbType.Date);
                break;
            case "time without time zone":
                await w.WriteAsync((TimeSpan)val, NpgsqlDbType.Time);
                break;
            case "boolean":
                await w.WriteAsync(Convert.ToBoolean(val), NpgsqlDbType.Boolean);
                break;
            case "jsonb":
                await w.WriteAsync((string)val, NpgsqlDbType.Jsonb);
                break;
            case "uuid":
                await w.WriteAsync((Guid)val, NpgsqlDbType.Uuid);
                break;
            default:
                await w.WriteAsync(val);
                break;
        }
    }

    private static async Task<int> ResetSequencesAsync(NpgsqlConnection dst)
    {
        // Para columnas GENERATED BY DEFAULT AS IDENTITY: ajustar la secuencia al MAX(id) actual.
        const string sql = @"
            SELECT c.table_name, c.column_name, pg_get_serial_sequence(quote_ident(c.table_name), c.column_name) AS seq
            FROM information_schema.columns c
            WHERE c.table_schema='public'
              AND c.is_identity='YES'
              AND pg_get_serial_sequence(quote_ident(c.table_name), c.column_name) IS NOT NULL;";

        var lista = new List<(string tabla, string col, string seq)>();
        await using (var cmd = new NpgsqlCommand(sql, dst))
        await using (var r = await cmd.ExecuteReaderAsync())
            while (await r.ReadAsync())
                lista.Add((r.GetString(0), r.GetString(1), r.GetString(2)));

        int n = 0;
        foreach (var (tabla, col, seq) in lista)
        {
            var setval = $"SELECT setval('{seq}', COALESCE((SELECT MAX(\"{col}\") FROM \"{tabla}\"), 1), (SELECT COUNT(*) FROM \"{tabla}\") > 0);";
            await using var cmd = new NpgsqlCommand(setval, dst);
            await cmd.ExecuteScalarAsync();
            n++;
        }
        return n;
    }

    // ─────────────────────────────────────────────────────────────
    //  AUDITORÍA
    // ─────────────────────────────────────────────────────────────
    private static async Task<int> RunAuditAsync()
    {
        Console.WriteLine("=== AUDITORÍA SQL Server vs PostgreSQL ===\n");

        var tablasSrc = new HashSet<string>(await GetSqlServerTablesAsync(), StringComparer.OrdinalIgnoreCase);
        var tablasDst = await GetPostgresTablesAsync();

        await using var src = new SqlConnection(SrcConn);
        await src.OpenAsync();
        await using var dst = new NpgsqlConnection(DstConn);
        await dst.OpenAsync();

        Console.WriteLine($"{"Tabla",-40} {"SQLServer",12} {"PostgreSQL",12}  Estado");
        Console.WriteLine(new string('-', 80));

        int ok = 0, dif = 0, faltan = 0;
        var problemas = new List<string>();

        foreach (var tabla in tablasDst)
        {
            long pg = await ScalarLongAsync(new NpgsqlCommand($"SELECT COUNT(*) FROM \"{tabla}\"", dst));
            if (!tablasSrc.Contains(tabla))
            {
                Console.WriteLine($"{tabla,-40} {"(no existe)",12} {pg,12}  ⊘ solo en PG");
                continue;
            }
            long sql = await ScalarLongAsync(new SqlCommand($"SELECT COUNT(*) FROM [{tabla}]", src));
            if (sql == pg)
            {
                ok++;
                Console.WriteLine($"{tabla,-40} {sql,12} {pg,12}  ✓");
            }
            else
            {
                dif++;
                problemas.Add($"{tabla}: SQLServer={sql} PostgreSQL={pg} (faltan {sql - pg})");
                Console.WriteLine($"{tabla,-40} {sql,12} {pg,12}  ❌ DIFIERE");
            }
        }

        // Tablas en SQL Server que no llegaron a PG.
        foreach (var t in tablasSrc)
            if (!tablasDst.Contains(t)) { faltan++; Console.WriteLine($"{t,-40} {"?",12} {"(ausente)",12}  ⊘ falta en PG"); }

        // Sumas de control de montos críticos.
        Console.WriteLine("\n--- Sumas de control (montos) ---");
        await CheckSumAsync(src, dst, "Recibo", "Total", problemas);
        await CheckSumAsync(src, dst, "Recibo", "Saldo", problemas);
        await CheckSumAsync(src, dst, "Pago", "Monto", problemas);

        Console.WriteLine($"\n=== RESUMEN: {ok} tablas OK, {dif} con diferencia, {faltan} ausentes en PG ===");
        if (problemas.Count > 0)
        {
            Console.WriteLine("\n⚠️  PROBLEMAS:");
            foreach (var p in problemas) Console.WriteLine($"   - {p}");
            return 1;
        }
        Console.WriteLine("\n✅ AUDITORÍA LIMPIA: conteos y sumas coinciden.");
        return 0;
    }

    private static async Task CheckSumAsync(SqlConnection src, NpgsqlConnection dst, string tabla, string col, List<string> problemas)
    {
        try
        {
            decimal s = await ScalarDecimalAsync(new SqlCommand($"SELECT ISNULL(SUM([{col}]),0) FROM [{tabla}]", src));
            decimal p = await ScalarDecimalAsync(new NpgsqlCommand($"SELECT COALESCE(SUM(\"{col}\"),0) FROM \"{tabla}\"", dst));
            var estado = s == p ? "✓" : "❌ DIFIERE";
            Console.WriteLine($"  SUM({tabla}.{col}): SQLServer={s:N2}  PostgreSQL={p:N2}  {estado}");
            if (s != p) problemas.Add($"SUM({tabla}.{col}): SQLServer={s:N2} PostgreSQL={p:N2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  SUM({tabla}.{col}): no verificable ({ex.Message})");
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  METADATOS
    // ─────────────────────────────────────────────────────────────
    private static async Task<List<string>> GetSqlServerTablesAsync()
    {
        var list = new List<string>();
        await using var c = new SqlConnection(SrcConn);
        await c.OpenAsync();
        await using var cmd = new SqlCommand(
            "SELECT name FROM sys.tables WHERE name <> '__EFMigrationsHistory' ORDER BY name", c);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(r.GetString(0));
        return list;
    }

    private static async Task<List<string>> GetPostgresTablesAsync()
    {
        var list = new List<string>();
        await using var c = new NpgsqlConnection(DstConn);
        await c.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT table_name FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE' AND table_name <> '__EFMigrationsHistory' ORDER BY table_name", c);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(r.GetString(0));
        return list;
    }

    private static async Task<List<ColumnInfo>> GetPostgresColumnsAsync(NpgsqlConnection c, string tabla)
    {
        var list = new List<ColumnInfo>();
        await using var cmd = new NpgsqlCommand(
            @"SELECT column_name, data_type, is_generated
              FROM information_schema.columns
              WHERE table_schema='public' AND table_name=@t
              ORDER BY ordinal_position", c);
        cmd.Parameters.AddWithValue("t", tabla);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            var isGenerated = r.GetString(2);
            if (isGenerated == "ALWAYS") continue; // columnas computed/stored: no se insertan
            list.Add(new ColumnInfo(r.GetString(0), r.GetString(1)));
        }
        return list;
    }

    private static async Task<Dictionary<string, string>> GetSqlServerColumnsAsync(string tabla)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await using var c = new SqlConnection(SrcConn);
        await c.OpenAsync();
        // Incluimos columnas computed: SQL Server devuelve su valor calculado.
        // El lado PostgreSQL decide si las inserta (columna normal) o las excluye (GENERATED ALWAYS).
        await using var cmd = new SqlCommand(
            @"SELECT c.name FROM sys.columns c
              JOIN sys.tables t ON t.object_id=c.object_id
              WHERE t.name=@t", c);
        cmd.Parameters.AddWithValue("@t", tabla);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) dict[r.GetString(0)] = "";
        return dict;
    }

    private static async Task<long> ScalarLongAsync(System.Data.Common.DbCommand cmd)
    {
        await using (cmd) return Convert.ToInt64(await cmd.ExecuteScalarAsync());
    }

    private static async Task<decimal> ScalarDecimalAsync(System.Data.Common.DbCommand cmd)
    {
        await using (cmd) return Convert.ToDecimal(await cmd.ExecuteScalarAsync());
    }

    private record ColumnInfo(string Name, string PgType);
}
