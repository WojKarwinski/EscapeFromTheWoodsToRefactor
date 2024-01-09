using EscapeFromTheWoods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        string connectionString = @"mongodb://localhost:27017";
        DBwriter db = new DBwriter(connectionString);
        string path = @"D:\School 2023-2024\EscapeFromTheWoodsToRefactor\EscapeFromTheWoodsToRefactor\monkeys";

        Map m1 = new Map(0, 500, 0, 500);
        Wood w1 = WoodBuilder.GetWood(500, m1, path, db);

        Map m2 = new Map(0, 200, 0, 400);
        Wood w2 = WoodBuilder.GetWood(2500, m2, path, db);

        Map m3 = new Map(0, 400, 0, 400);
        Wood w3 = WoodBuilder.GetWood(2000, m3, path, db);

        var placementTasks = new List<Task>
            {
        PlaceMonkeysInWood(w1, new string[] { "Alice", "Janice", "Toby", "Mindy", "Jos" }),
        PlaceMonkeysInWood(w2, new string[] { "Tom", "Jerry", "Tiffany", "Mozes", "Jebus" }),
        PlaceMonkeysInWood(w3, new string[] { "Kelly", "Kenji", "Kobe", "Kendra" })
             };
        await Task.WhenAll(placementTasks);

        // Write to DB
        var dbTasks = new Task[] { w1.WriteWoodToDBAsync(), w2.WriteWoodToDBAsync(), w3.WriteWoodToDBAsync() };
        await Task.WhenAll(dbTasks);

        // Escape at the same time
        var escapeTasks = new Task[] { w1.EscapeAsync(), w2.EscapeAsync(), w3.EscapeAsync() };
        await Task.WhenAll(escapeTasks);

        stopwatch.Stop();
        Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
        Console.WriteLine("end");
    }
    static async Task PlaceMonkeysInWood(Wood wood, IEnumerable<string> monkeyNames)
    {
        foreach(var name in monkeyNames)
        {
            await wood.PlaceMonkeyAsync(name, IDgenerator.GetMonkeyID());
        }
    }
}
