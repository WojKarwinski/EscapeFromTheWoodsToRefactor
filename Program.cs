using EscapeFromTheWoods;
using System;
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
        await w1.PlaceMonkeyAsync("Alice", IDgenerator.GetMonkeyID());
        await w1.PlaceMonkeyAsync("Janice", IDgenerator.GetMonkeyID());
        await w1.PlaceMonkeyAsync("Toby", IDgenerator.GetMonkeyID());
        await w1.PlaceMonkeyAsync("Mindy", IDgenerator.GetMonkeyID());
        await w1.PlaceMonkeyAsync("Jos", IDgenerator.GetMonkeyID());

        Map m2 = new Map(0, 200, 0, 400);
        Wood w2 = WoodBuilder.GetWood(2500, m2, path, db);
        await w2.PlaceMonkeyAsync("Tom", IDgenerator.GetMonkeyID());
        await w2.PlaceMonkeyAsync("Jerry", IDgenerator.GetMonkeyID());
        await w2.PlaceMonkeyAsync("Tiffany", IDgenerator.GetMonkeyID());
        await w2.PlaceMonkeyAsync("Mozes", IDgenerator.GetMonkeyID());
        await w2.PlaceMonkeyAsync("Jebus", IDgenerator.GetMonkeyID());

        Map m3 = new Map(0, 400, 0, 400);
        Wood w3 = WoodBuilder.GetWood(2000, m3, path, db);
        await w3.PlaceMonkeyAsync("Kelly", IDgenerator.GetMonkeyID());
        await w3.PlaceMonkeyAsync("Kenji", IDgenerator.GetMonkeyID());
        await w3.PlaceMonkeyAsync("Kobe", IDgenerator.GetMonkeyID());
        await w3.PlaceMonkeyAsync("Kendra", IDgenerator.GetMonkeyID());

        await w1.WriteWoodToDBAsync();
        await w2.WriteWoodToDBAsync();
        await w3.WriteWoodToDBAsync();
        await w1.EscapeAsync();
        await w2.EscapeAsync();
        await w3.EscapeAsync();

        stopwatch.Stop();
        Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
        Console.WriteLine("end");
    }
}