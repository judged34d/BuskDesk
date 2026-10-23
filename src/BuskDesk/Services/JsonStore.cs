using System.Text.Json;
using System.Text.Json.Serialization;
using BuskDesk.Model;
using BuskDesk.Output;

namespace BuskDesk.Services;

public static class JsonStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public static OutputSettings LoadSettings()
    {
        try
        {
            if (File.Exists(AppPaths.SettingsFile))
            {
                return JsonSerializer.Deserialize<OutputSettings>(
                           File.ReadAllText(AppPaths.SettingsFile), Options)
                       ?? new OutputSettings();
            }
        }
        catch
        {
            // fall through
        }

        return new OutputSettings();
    }

    public static void SaveSettings(OutputSettings settings)
    {
        File.WriteAllText(AppPaths.SettingsFile, JsonSerializer.Serialize(settings, Options));
    }

    public static void SaveFixture(FixtureDefinition definition)
    {
        var safe = Sanitize(definition.DisplayName);
        var path = Path.Combine(AppPaths.FixturesDir, $"{safe}-{definition.Id[..8]}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(definition, Options));
    }

    public static IReadOnlyList<FixtureDefinition> LoadFixtures()
    {
        var list = new List<FixtureDefinition>();
        foreach (var file in Directory.EnumerateFiles(AppPaths.FixturesDir, "*.json"))
        {
            try
            {
                var def = JsonSerializer.Deserialize<FixtureDefinition>(File.ReadAllText(file), Options);
                if (def is not null)
                {
                    list.Add(def);
                }
            }
            catch
            {
                // skip broken
            }
        }

        return list.OrderBy(f => f.DisplayName).ToList();
    }

    public static void SavePatch(IEnumerable<PatchedFixture> patch)
    {
        var payload = patch.Select(p => new PatchedFixture
        {
            InstanceId = p.InstanceId,
            DefinitionId = p.DefinitionId,
            Name = p.Name,
            StartAddress = p.StartAddress,
            Group = p.Group
        }).ToList();
        File.WriteAllText(AppPaths.PatchFile, JsonSerializer.Serialize(payload, Options));
    }

    public static List<PatchedFixture> LoadPatch(IReadOnlyDictionary<string, FixtureDefinition> definitions)
    {
        if (!File.Exists(AppPaths.PatchFile))
        {
            return [];
        }

        try
        {
            var list = JsonSerializer.Deserialize<List<PatchedFixture>>(
                           File.ReadAllText(AppPaths.PatchFile), Options)
                       ?? [];
            foreach (var item in list)
            {
                if (definitions.TryGetValue(item.DefinitionId, out var def))
                {
                    item.Definition = def;
                }
            }

            return list;
        }
        catch
        {
            return [];
        }
    }

    public static void SaveScenes(IEnumerable<SceneLook> scenes)
    {
        File.WriteAllText(AppPaths.ScenesFile, JsonSerializer.Serialize(scenes.ToList(), Options));
    }

    public static List<SceneLook> LoadScenes()
    {
        if (!File.Exists(AppPaths.ScenesFile))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<SceneLook>>(
                       File.ReadAllText(AppPaths.ScenesFile), Options)
                   ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static void SaveFocusPoints(IEnumerable<FocusPoint> points) =>
        File.WriteAllText(AppPaths.FocusFile, JsonSerializer.Serialize(points.ToList(), Options));

    public static List<FocusPoint> LoadFocusPoints()
    {
        if (!File.Exists(AppPaths.FocusFile)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<FocusPoint>>(
                       File.ReadAllText(AppPaths.FocusFile), Options)
                   ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static void SavePatterns(IEnumerable<MovePattern> patterns) =>
        File.WriteAllText(AppPaths.PatternsFile, JsonSerializer.Serialize(patterns.ToList(), Options));

    public static List<MovePattern> LoadPatterns()
    {
        if (!File.Exists(AppPaths.PatternsFile)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<MovePattern>>(
                       File.ReadAllText(AppPaths.PatternsFile), Options)
                   ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return string.IsNullOrWhiteSpace(name) ? "fixture" : name.Trim();
    }
}
