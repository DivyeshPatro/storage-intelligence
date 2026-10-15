using System.Text.Json;
using System.Text.Json.Serialization;
using StorageIntelligence.Bench;
using StorageIntelligence.Scanning.Windows;

// si-bench: M0c measurement harness. Every result records where it was measured (BenchEnvironment.Kind, set with
// SI_BENCH_KIND, defaults to "CI (GitHub-hosted VM)" under GitHub Actions).
//   core       --entries N --store full|parents --temp DIR --out FILE        any OS: accounting core only, no disk I/O
//   fixture    --root DIR --files N --out FILE                              guarded write: synthetic tree
//   probe-file --root DIR --size-mb N                                       guarded write: probe data file
//   scan       --root DIR --mode low-impact|fast --links none|retained|all --cache cold|warm --temp DIR --out FILE
//   cancel     --root DIR --mode low-impact|fast --trials N --temp DIR --out FILE
//   probe      --file FILE --label TEXT (--seconds S | --until-pid PID) --out FILE
//   startup    --out FILE
DateTime mainEntered = DateTime.Now;
var json = new JsonSerializerOptions
{
    WriteIndented = true,
    IncludeFields = true,
    Converters = { new JsonStringEnumConverter() },
};

if (args.Length == 0)
{
    Console.Error.WriteLine("usage: si-bench core|fixture|probe-file|scan|cancel|probe|startup [options]");
    return 2;
}

object result = args[0] switch
{
    "core" => CoreBench.Run(long.Parse(Arg("entries")), Arg("store", "full"), Arg("temp", Path.GetTempPath())),
    "fixture" => WindowsBench.Fixture(Arg("root"), long.Parse(Arg("files"))),
    "probe-file" => ProbeFile(),
    "scan" => WindowsBench.Scan(Arg("root"), Mode(Arg("mode")), Links(Arg("links", "retained")), Arg("cache", "unknown"), Arg("temp", Path.GetTempPath())),
    "cancel" => WindowsBench.Cancel(Arg("root"), Mode(Arg("mode")), int.Parse(Arg("trials", "10")), Arg("temp", Path.GetTempPath()), int.Parse(Arg("seed", "1"))),
    "probe" => WindowsBench.Probe(Arg("file"), Arg("label"), double.Parse(Arg("seconds", "0"), System.Globalization.CultureInfo.InvariantCulture), int.Parse(Arg("until-pid", "0"))),
    "startup" => WindowsBench.Startup(mainEntered),
    _ => throw new ArgumentException($"unknown command {args[0]}"),
};

string text = JsonSerializer.Serialize(result, result.GetType(), json);
if (Array.IndexOf(args, "--out") >= 0)
{
    File.WriteAllText(Arg("out"), text);
}

Console.WriteLine(text);
return 0;

string Arg(string name, string? fallback = null)
{
    int i = Array.IndexOf(args, "--" + name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback ?? throw new ArgumentException($"missing --{name}");
}

object ProbeFile()
{
    WindowsBench.ProbeFile(Arg("root"), long.Parse(Arg("size-mb")));
    return new { created = Path.Combine(Arg("root"), "probe.bin") };
}

static ScanMode Mode(string s) => s switch
{
    "low-impact" => ScanMode.LowImpact,
    "fast" => ScanMode.Fast,
    _ => throw new ArgumentException($"unknown mode {s}"),
};

static LinkCountMeasurement Links(string s) => s switch
{
    "none" => LinkCountMeasurement.None,
    "retained" => LinkCountMeasurement.RetainedFiles,
    "all" => LinkCountMeasurement.All,
    _ => throw new ArgumentException($"unknown link mode {s}"),
};
