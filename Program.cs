using Newtonsoft.Json;
using System.Net.Http;

public static class PokeAPI
{
    private static readonly HttpClient client = new HttpClient { BaseAddress = new Uri("https://pokeapi.co/api/v2/") };

    public static async Task<Pokemon> GetPokemonData(string name)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync($"pokemon/{name.ToLower()}");
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var apiData = JsonConvert.DeserializeObject<PokeResponse>(json);

                return new Pokemon
                {
                    name = apiData?.name,
                    type = string.Join(", ", apiData?.types?.Select(t => t.type?.name) ?? new string[0]),
                    hp = apiData?.stats?.FirstOrDefault(s => s.stat?.name == "hp")?.base_stat ?? 0,
                    attack = apiData?.stats?.FirstOrDefault(s => s.stat?.name == "attack")?.base_stat ?? 0,
                    defense = apiData?.stats?.FirstOrDefault(s => s.stat?.name == "defense")?.base_stat ?? 0
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"error fetching pokemon data: {ex.Message}");
        }

        return new Pokemon { name = "unknown", hp = 0, attack = 0, defense = 0 };
    }
}

public class Pokemon
{
    public string? name { get; set; }
    public string? type { get; set; }
    public int hp { get; set; }
    public int attack { get; set; }
    public int defense { get; set; }

    public override string ToString()
    {
        return $"{name} - type: {type} - hp: {hp}, attack: {attack}, defense: {defense}";
    }
}

public class PokeResponse
{
    public string? name { get; set; }
    public List<TypeData>? types { get; set; }
    public List<Stat>? stats { get; set; }
}

public class TypeData
{
    public TypeInfo? type { get; set; }
}

public class TypeInfo
{
    public string? name { get; set; }
}

public class Stat
{
    public int base_stat { get; set; }
    public StatInfo? stat { get; set; }
}

public class StatInfo
{
    public string? name { get; set; }
}

class Program
{
    public static async Task Battle(Pokemon pokemon1, Pokemon pokemon2)
    {
        Console.WriteLine($"\nstarting battle: {pokemon1.name} vs {pokemon2.name}");
        await Task.Delay(1000);

        while (pokemon1.hp > 0 && pokemon2.hp > 0)
        {
            int damageTo2 = Math.Max(1, (new Random().Next(pokemon1.attack / 2, pokemon1.attack + 1)) - pokemon2.defense);
            pokemon2.hp -= damageTo2;
            Console.WriteLine($"{pokemon1.name} attacks {pokemon2.name} for {damageTo2} damage. {pokemon2.name} has {Math.Max(0, pokemon2.hp)} hp left.");
            await Task.Delay(1000);

            if (pokemon2.hp <= 0)
            {
                Console.WriteLine($"\n{pokemon2.name} fainted. {pokemon1.name} wins!");
                return;
            }

            int damageTo1 = Math.Max(1, (new Random().Next(pokemon2.attack / 2, pokemon2.attack + 1)) - pokemon1.defense);
            pokemon1.hp -= damageTo1;
            Console.WriteLine($"{pokemon2.name} attacks {pokemon1.name} for {damageTo1} damage. {pokemon1.name} has {Math.Max(0, pokemon1.hp)} hp left.");
            await Task.Delay(1000);

            if (pokemon1.hp <= 0)
            {
                Console.WriteLine($"\n{pokemon1.name} fainted. {pokemon2.name} wins!");
                return;
            }
        }
    }

    static async Task Main()
    {
        Console.WriteLine("poke-search");

        while (true)
        {
            Console.WriteLine("\n1: search");
            Console.WriteLine("2: battle");
            Console.WriteLine("3: exit");

            Console.Write("choose: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("pokemon name: ");
                    string name = Console.ReadLine() ?? "";
                    Pokemon pokemon = await PokeAPI.GetPokemonData(name);
                    Console.WriteLine(pokemon != null && pokemon.name != "unknown" ? pokemon.ToString() : "pokemon not found");
                    break;

                case "2":
                    Console.Write("pokemon 1 name: ");
                    string name1 = Console.ReadLine() ?? "";
                    Pokemon pokemon1 = await PokeAPI.GetPokemonData(name1);

                    Console.Write("pokemon 2 name: ");
                    string name2 = Console.ReadLine() ?? "";
                    Pokemon pokemon2 = await PokeAPI.GetPokemonData(name2);

                    if (pokemon1.name != "unknown" && pokemon2.name != "unknown")
                        await Battle(pokemon1, pokemon2);
                    else
                        Console.WriteLine("one or both pokemon not found");
                    break;

                case "3":
                    return;

                default:
                    Console.WriteLine("invalid choice");
                    break;
            }
        }
    }
}
