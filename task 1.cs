using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using PluginSystem;

public interface IPlugin : IDisposable
{
    string Name { get; }
    string Version { get; }
    string Author { get; }
    void Initialize();
    void Execute();
}

public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path != null ? LoadFromAssemblyPath(path) : null;
    }
}

public class PluginContainer
{
    public IPlugin Plugin { get; }
    public PluginLoadContext Context { get; }
    public PluginContainer(IPlugin plugin, PluginLoadContext context)
    {
        Plugin = plugin;
        Context = context;
    }
}

public class PluginManager
{
    private readonly string _pluginsPath;
    private readonly List<PluginContainer> _plugins = new();

    public PluginManager(string pluginsPath = "Plugins")
    {
        _pluginsPath = pluginsPath;
        Directory.CreateDirectory(_pluginsPath);
    }

    public void LoadPluginsFromDirectory()
    {
        var dllFiles = Directory.GetFiles(_pluginsPath, "*.dll", SearchOption.AllDirectories);

        foreach (var dllPath in dllFiles)
        {
            try
            {
                var context = new PluginLoadContext(dllPath);
                var assembly = context.LoadFromAssemblyPath(dllPath);

                var pluginType = assembly.GetTypes()
                    .FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                if (pluginType == null) continue;

                if (Activator.CreateInstance(pluginType) is not IPlugin plugin) continue;

                plugin.Initialize();
                _plugins.Add(new PluginContainer(plugin, context));
            }
            catch { }
        }
    }

    public void ExecuteAll()
    {
        foreach (var p in _plugins)
        {
            try { p.Plugin.Execute(); }
            catch { }
        }
    }

    public void GetPluginStats()
    {
        foreach (var p in _plugins)
        {
            Console.WriteLine($"{p.Plugin.Name} версия {p.Plugin.Version} (автор: {p.Plugin.Author})");
        }
        Console.WriteLine();
    }

    public void UnloadAll()
    {
        foreach (var p in _plugins)
        {
            p.Plugin.Dispose();
            p.Context.Unload();
        }
        _plugins.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}

public class MathPlugin : IPlugin
{
    public string Name => "Математический плагин";
    public string Version => "1.0";
    public string Author => "Алиса";
    public void Initialize() { }
    public void Execute() => Console.WriteLine("10 + 20 = " + (10 + 20));
    public void Dispose() { }
}

public class StringPlugin : IPLugin
{
    public string Name => "Строковый плагин";
    public string Version => "1.0";
    public string Author => "Борис";
    public void Initialize() { }
    public void Execute() => Console.WriteLine("Привет от строкового плагина!".ToUpper());
    public void Dispose() { }
}

public class DataPlugin : IPlugin
{
    public string Name => "Плагин данных";
    public string Version => "1.0";
    public string Author => "Катя";
    public void Initialize() { }
    public void Execute() => Console.WriteLine("Данные успешно обработаны");
    public void Dispose() { }
}

public class LoggingPlugin : IPlugin
{
    public string Name => "Плагин логирования";
    public string Version => "1.0";
    public string Author => "Дима";
    public void Initialize() { }
    public void Execute() => Console.WriteLine("Запись в лог добавлена");
    public void Dispose() { }
}

public class NetworkPlugin : IPlugin
{
    public string Name => "Сетевой плагин";
    public string Version => "1.0";
    public string Author => "Ева";
    public void Initialize() { }
    public void Execute() => Console.WriteLine("Пинг прошёл успешно");
    public void Dispose() { }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Запуск системы плагинов\n");

        var manager = new PluginManager();

        Directory.CreateDirectory("Plugins");
        var currentDll = Assembly.GetExecutingAssembly().Location;
        if (!string.IsNullOrEmpty(currentDll))
        {
            File.Copy(currentDll, Path.Combine("Plugins", "MathPlugin.dll"), true);
            File.Copy(currentDll, Path.Combine("Plugins", "StringPlugin.dll"), true);
            File.Copy(currentDll, Path.Combine("Plugins", "DataPlugin.dll"), true);
            File.Copy(currentDll, Path.Combine("Plugins", "LoggingPlugin.dll"), true);
            File.Copy(currentDll, Path.Combine("Plugins", "NetworkPlugin.dll"), true);
        }

        manager.LoadPluginsFromDirectory();
        manager.GetPluginStats();
        manager.ExecuteAll();

        Console.WriteLine("\nНажмите любую клавишу для выгрузки плагинов...");
        Console.ReadKey();

        manager.UnloadAll();

        Console.WriteLine("Все плагины выгружены. Готово!");
        Console.WriteLine("Нажмите любую клавишу для выхода.");
        Console.ReadKey();
    }
}