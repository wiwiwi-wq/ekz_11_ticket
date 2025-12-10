using System;
using System.Data.SqlClient; 
using System.Threading.Tasks;
using System.Timers;

namespace ConnectionPoolLeakDemo
{
    class Program
    {
        private const string ConnectionString =
            "Server=localhost;Database=master;Integrated Security=true;Pooling=true;Max Pool Size=100;Min Pool Size=0;Connection Timeout=30;";

        private static Timer _monitorTimer;

        static async Task Main(string[] args)
        {

            StartPoolMonitoring();

            Console.WriteLine("Нажмите 1 - создать нормальные соединения (закрываются через using)");
            Console.WriteLine("Нажмите 2 — создать утечку (соединения НЕ закрываются)");
            Console.WriteLine("Нажмите Q — выход\n");

            while (true)
            {
                var key = Console.ReadKey(intercept: true).Key;

                if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1)
                {
                    await CreateSafeConnections();
                }
                else if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2)
                {
                    CreateLeakedConnections();
                }
                else if (key == ConsoleKey.Q)
                {
                    _monitorTimer?.Stop();
                    _monitorTimer?.Dispose();
                    Console.WriteLine("Выход...");
                    break;
                }
            }
        }

        private static async Task CreateSafeConnections()
        {
            Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] Создаём 20 безопасных соединений (using)...");

            for (int i = 0; i < 20; i++)
            {
                using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT 1";
                await cmd.ExecuteScalarAsync();
            }

            Console.WriteLine("  Все соединения закрыты и возвращены в пул.");
        }

        private static void CreateLeakedConnections()
        {
            Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] Создаём 15 соединений с утечкой");

            for (int i = 0; i < 15; i++)
            {
                var connection = new SqlConnection(ConnectionString);
                connection.OpenAsync().Wait(); 

                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT @@SPID"; 
                cmd.ExecuteScalarAsync().Wait();

            }

            Console.WriteLine("   15 соединений остались открытыми");
        }

        private static void StartPoolMonitoring()
        {
            _monitorTimer = new Timer(3000); 
            _monitorTimer.Elapsed += (s, e) =>
            {
                try
                {
                    var pool = SqlConnectionStringBuilder.GetPool(ConnectionString);
                    if (pool != null)
                    {
                        var poolField = pool.GetType().GetField("_pool",
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);

                        var internalPool = poolField?.GetValue(pool);

                        if (internalPool != null)
                        {
                            var activeCountField = internalPool.GetType().GetProperty("ActiveConnectionCount",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                            var totalCountField = internalPool.GetType().GetProperty("TotalConnectionCount",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                            var active = activeCountField?.GetValue(internalPool) ?? "N/A";
                            var total = totalCountField?.GetValue(internalPool) ?? "N/A";

                            Console.WriteLine($"[МОНИТОРИНГ {e.SignalTime:HH:mm:ss}] Активных: {active} | Всего в пуле: {total}");
                        }
                    }
                }
                catch
                {
                    ShowPerformanceCounters();
                }
            };

            _monitorTimer.AutoReset = true;
            _monitorTimer.Start();

            ShowPerformanceCounters();
        }

        private static void ShowPerformanceCounters()
        {
            try
            {
                var category = new System.Diagnostics.PerformanceCounterCategory(".NET Data Provider for SqlServer");
                var instances = category.GetInstanceNames();

                foreach (var inst in instances)
                {
                    if (inst.Contains("SqlClient")) 
                    {
                        var active = new System.Diagnostics.PerformanceCounter(".NET Data Provider for SqlServer", "NumberOfActiveConnections", inst, true);
                        var pooled = new System.Diagnostics.PerformanceCounter(".NET Data Provider for SqlServer", "NumberOfPooledConnections", inst, true);
                        var nonPooled = new System.Diagnostics.PerformanceCounter(".NET Data Provider for SqlServer", "NumberOfNonPooledConnections", inst, true);

                        Console.WriteLine($"[PerfCounter {DateTime.Now:HH:mm:ss}] " +
                                          $"Активных: {active.NextValue():F0} | " +
                                          $"В пуле: {pooled.NextValue():F0} | " +
                                          $"Не в пуле: {nonPooled.NextValue():F0} | Инстанс: {inst}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PerfCounter ошибка] {ex.Message}");
            }
        }
    }
}