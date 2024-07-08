using g4;
using System.Collections.Generic;
using Microsoft.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using Newtonsoft.Json;
using System.Diagnostics.Tracing;

public class ImplicitUnitCell : BoundedImplicitFunction3d {
    private FieldInfo latticeIndexField;
    Type unitCellType;
    MethodInfo valueMethod;

    public Dictionary<string, ImplicitParameter> Parameters { get; private set; }

    public ImplicitUnitCell(string codeRepPath, int latticeIndex) {
        Console.WriteLine($"Using unit cell {codeRepPath}.");

        var manifestContent = ReadText(Path.Combine(codeRepPath, "manifest.json"));
        var manifest = JsonConvert.DeserializeObject<ImplicitManifest>(manifestContent);

        if (manifest is null)
            throw new Exception("Failed to parse manifest JSON.");

        Parameters = manifest.parameters.ToDictionary(p => p.name, p => p);

        // HACK: Assume that common sources directory is relative to the program executable
        const string ImplicitCS = "Implicit.cs";
        string commonSourcesDirPath = PathUtil.FindDirectoryContaining(ImplicitCS);

        var sources = new string[] {
            Path.Combine(commonSourcesDirPath, ImplicitCS),
            Path.Combine(commonSourcesDirPath, "ImplicitParameter.cs"),
            Path.Combine(codeRepPath, manifest.cSharpLibrary),
            Path.Combine(codeRepPath, manifest.cSharpCode)
        };
        PathUtil.AssertExists(sources);

        var codeList = sources.Select(s => ReadText(s));

        unitCellType = BuildImplicit(codeList);
        if (unitCellType == null)
            throw new Exception("Error compiling unit cell.");

        var latticeIndexField = unitCellType.GetField("VariantIndex");
        if (latticeIndexField is null)
            throw new Exception("Error getting VariantIndex field.");

        foreach (var p in Parameters.Values) {
            SetParameter(p.name, p.defaultValue);
        }

        valueMethod = unitCellType.GetMethod("Value");
        if (valueMethod is null)
            throw new Exception("Error getting value callback.");

    }

    public int VariantIndex {
        get => (int)latticeIndexField.GetValue(null);
        set => latticeIndexField.SetValue(null, value);
    }

    public void SetParameter(string name, double value) {
        Parameters[name].value = value;
        var param = unitCellType.GetField(name);
        if (param is FieldInfo)
            param.SetValue(null, value);
    }

    public double Value(ref Vector3d p) {
        return (double)valueMethod.Invoke(null, new object[] { p });
    }

    public AxisAlignedBox3d Bounds() {
        var halfsize = new Vector3d(Parameters["size_x"].defaultValue, Parameters["size_y"].defaultValue, Parameters["size_z"].defaultValue) * 0.5;
        return new AxisAlignedBox3d(-halfsize, halfsize);
    }

    private static Type BuildImplicit(IEnumerable<string> sources) {
        // based on
        // https://stackoverflow.com/questions/32769630/how-to-compile-a-c-sharp-file-with-roslyn-programmatically
        // https://weblog.west-wind.com/posts/2022/Jun/07/Runtime-CSharp-Code-Compilation-Revisited-for-Roslyn

        var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray();
        var rtPath = Path.GetDirectoryName(typeof(object).Assembly.Location) + Path.DirectorySeparatorChar;
        CSharpCompilation compilation = CSharpCompilation.Create(
            "assemblyName",
            syntaxTrees,
            new[] { 
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(rtPath + "System.Runtime.dll"),
                MetadataReference.CreateFromFile(rtPath + "System.Collections.dll"),
                MetadataReference.CreateFromFile("geometry4Sharp.dll")
                },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assembly? assembly = null;
        using (var dllStream = new MemoryStream())
        using (var pdbStream = new MemoryStream())
        {
            var emitResult = compilation.Emit(dllStream, pdbStream);
            if (!emitResult.Success) {
                emitResult.Diagnostics.ToList().ForEach(error => Console.WriteLine(error.ToString()));
                return null;
            }

            assembly = Assembly.Load(((MemoryStream)dllStream).ToArray());

            var module = assembly.GetModules()[0];
            var lrType = module.GetType("LRImplicit");
            return lrType;
        }
    }

    private static string ReadText(string path) {
        string op = "";
        try {
            StreamReader stream = new(path);
            op = stream.ReadToEnd();
            stream.Close();
        }
        catch (Exception e) {
            Console.WriteLine("Exception: " + e.Message);
        }
        return op;
    }
}

