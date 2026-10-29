
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RamsCottons.Data;

namespace RamsCottons.Services
{
    public class SaldoClienteService
    {
        private readonly CycDataContext _cycData;

        public SaldoClienteService(CycDataContext cycData)
        {
            _cycData = cycData;
        }

        public async Task<decimal?> ObtenerSaldoPorTelefonoAsync(
            string telefono,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return null;

            // ==========================================
            // 1. NORMALIZAR TELÉFONO
            // ==========================================

            var numero = Regex.Replace(telefono, @"\D", "");

            if (numero.Length == 13 && numero.StartsWith("521"))
            {
                numero = numero[3..];
            }
            else if (numero.Length == 12 && numero.StartsWith("52"))
            {
                numero = numero[2..];
            }

            if (numero.Length != 10)
                return null;

            var telefono10 = numero;
            var telefono52 = "52" + numero;
            var telefono521 = "521" + numero;

            // ==========================================
            // 2. VIGENCIA ANUAL DE PUNTOS RAMS
            // ==========================================

            // Primer día del año actual.
            var inicioAnio = new DateTime(
                DateTime.Today.Year, 1, 1);

            // Primer día del siguiente año.
            // Los puntos anteriores dejan de contar.
            var inicioSiguienteAnio = inicioAnio.AddYears(1);

            // ==========================================
            // 3. CONSULTAR SALDO VIGENTE
            // ==========================================

            // La columna saldo se calcula en SQL:
            // puntos - pagos.
            //
            // Solo se suman movimientos del año actual.
            //
            // Si existen movimientos históricos,
            // pero ninguno vigente, devuelve 0.
            //
            // Si nunca tuvo movimientos, devuelve NULL.
            //
            // Compatible con SQL Server 2008.

            var saldo = await _cycData.Database
                .SqlQuery<decimal?>($"""
                    SELECT
                        CASE
                            WHEN COUNT_BIG(*) = 0
                            THEN CAST(NULL AS DECIMAL(18,2))

                            ELSE CAST(
                                SUM(
                                    CASE
                                        WHEN [fecha] >= {inicioAnio}
                                         AND [fecha] < {inicioSiguienteAnio}
                                        THEN CAST(
                                            ISNULL([saldo], 0)
                                            AS BIGINT
                                        )

                                        ELSE CAST(0 AS BIGINT)
                                    END
                                )
                                AS DECIMAL(18,2)
                            )
                        END AS [Value]

                    FROM [dbo].[puntosrams]

                    WHERE
                        REPLACE(
                            REPLACE(
                                REPLACE(
                                    REPLACE(
                                        REPLACE(
                                            LTRIM(RTRIM([cliente])),
                                            ' ', ''
                                        ),
                                        '-', ''
                                    ),
                                    '+', ''
                                ),
                                '(', ''
                            ),
                            ')', ''
                        ) IN (
                            {telefono10},
                            {telefono52},
                            {telefono521}
                        )
                    """)
                .FirstOrDefaultAsync(cancellationToken);

            return saldo;
        }
    }
}
