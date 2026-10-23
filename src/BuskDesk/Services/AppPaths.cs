namespace BuskDesk.Services;

public static class AppPaths
{
    public static string Root
    {
        get
        {
            // Prefer repo-adjacent data when running from publish/mock or bin
            var baseDir = AppContext.BaseDirectory;
            var candidate = Path.GetFullPath(Path.Combine(baseDir, "..", ".."));
            if (Directory.Exists(Path.Combine(candidate, "src")))
            {
                return candidate;
            }

            candidate = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            if (Directory.Exists(Path.Combine(candidate, "src")))
            {
                return candidate;
            }

            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BuskDesk");
            Directory.CreateDirectory(appData);
            return appData;
        }
    }

    public static string FixturesDir => Ensure(Path.Combine(Root, "fixtures"));
    public static string ShowsDir => Ensure(Path.Combine(Root, "shows"));
    public static string LibraryDir => Ensure(Path.Combine(Root, "library"));
    public static string SettingsFile => Path.Combine(Root, "buskdesk.settings.json");
    public static string PatchFile => Path.Combine(ShowsDir, "current.patch.json");
    public static string ScenesFile => Path.Combine(ShowsDir, "current.scenes.json");
    public static string FocusFile => Path.Combine(ShowsDir, "current.focus.json");
    public static string PatternsFile => Path.Combine(ShowsDir, "current.patterns.json");

    private static string Ensure(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }
}
