using EscapeFromTheWoods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        string connectionString = @"mongodb://localhost:27017";
        DBwriter db = new(connectionString);
        string path = @"C:\School 2023-2024\Programmeren Specialisatie\EscapeFromTheWoodsToRefactor\monkeys";

        Task t1 = Task.Run(async () =>
        {
            Map m1 = new(0, 500, 0, 500);
            Wood w1 = WoodBuilder.GetWood(30000, m1, path, db);
            PlaceMonkeysInWood(w1, new string[] { "Alice", "Janice", "Toby", "Mindy", "Jos" });
            await Console.Out.WriteLineAsync("GAME 1 START");
            await w1.WriteWoodToDBAsync();
            await w1.EscapeAsync();
        });

        Task t2 = Task.Run(async () =>
        {
            Map m2 = new(0, 200, 0, 400);
            Wood w2 = WoodBuilder.GetWood(20000, m2, path, db);
            PlaceMonkeysInWood(w2, new string[] { "Tom", "Jerry", "Tiffany", "Mozes", "Jebus" });
            await Console.Out.WriteLineAsync("GAME 2 START");
            await w2.WriteWoodToDBAsync();
            await w2.EscapeAsync();
        });

        Task t3 = Task.Run(async () =>
        {
            Map m3 = new(0, 400, 0, 400);
            Wood w3 = WoodBuilder.GetWood(10000, m3, path, db);
            PlaceMonkeysInWood(w3, new string[] { "Kelly", "Kenji", "Kobe", "Kendra" });
            await Console.Out.WriteLineAsync("GAME 3 START");
            await w3.WriteWoodToDBAsync();
            await w3.EscapeAsync();
        });

        await Task.WhenAll(t1, t2, t3);

        stopwatch.Stop();
        Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
        Console.WriteLine("end");
    }

    static async Task PlaceMonkeysInWood(Wood wood, IEnumerable<string> monkeyNames)
    {
        var tasks = new List<Task>();
        foreach(var name in monkeyNames)
        {
            int monkeyID = IDgenerator.GetMonkeyID();
            tasks.Add(wood.PlaceMonkeyAsync(name, monkeyID));
        }
        await Task.WhenAll(tasks);
    }
}
