// SrmProbe - independent .NET metadata reader used to validate the published
// assembly-reference inventory, the per-type method/field/property census and
// the settings-key list in docs/ASSEMBLY_REFERENCES.md and
// docs/CONFIGURATION_AND_STATE.md.
//
// It uses .NET's own ECMA-335 reader (System.Reflection.Metadata) and shares no
// code with any other tool used in this research. It is deliberately written to
// REPORT problems rather than suppress them.
//
// The file is opened read-only and is NEVER executed. Nothing here modifies the
// input, contacts a network endpoint, or loads the target as an assembly.
//
//   dotnet run -c Release -- <path-to-NetMod.exe> [output.txt]
//
// .NET SDK 9.0.319 / runtime 9.0.21 were used to produce the published numbers.

using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;

string path = args.Length > 0 ? args[0] : "NetMod.exe";
string outPath = args.Length > 1 ? args[1] : "srm-probe-output.txt";

using var fs = File.OpenRead(path);
using var pe = new PEReader(fs);
var md = pe.GetMetadataReader();

var w = new StreamWriter(outPath, false, new UTF8Encoding(false));
void L(string s = "") => w.WriteLine(s);
string Safe(string s)
{
    // Type names may be obfuscated with zero-width characters; keep the report
    // printable and record the length instead of emitting control characters.
    if (s.Length > 0 && (char.IsControl(s[0]) || s[0] > 0x7E))
        return "<non-printable name, length=" + s.Length + ">";
    return s;
}

L("SrmProbe - independent .NET metadata reader");
L("runtime : " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
L("file    : " + path);
L();

var asm = md.GetAssemblyDefinition();
L("=== ASSEMBLY ===");
L($"  Name          : {md.GetString(asm.Name)}");
L($"  Version       : {asm.Version}");
L($"  Culture       : '{md.GetString(asm.Culture)}'");
L($"  Flags         : 0x{(uint)asm.Flags:X8}");
L($"  HashAlgorithm : 0x{(uint)asm.HashAlgorithm:X8}");
L($"  HasPublicKey  : {!asm.PublicKey.IsNil}");
L();

L("=== ROW COUNTS ===");
L($"  TypeDef   {md.TypeDefinitions.Count}");
L($"  MethodDef {md.MethodDefinitions.Count}");
L($"  TypeRef   {md.GetTableRowCount(TableIndex.TypeRef)}");
L($"  MemberRef {md.GetTableRowCount(TableIndex.MemberRef)}");
L($"  AssemblyRef {md.GetTableRowCount(TableIndex.AssemblyRef)}");
L($"  Property  {md.PropertyDefinitions.Count}");
L();

L("=== ASSEMBLYREF INVENTORY ===");
L("row | name | version | flags | pktLen | pktHex | culture | hashLen");
foreach (var h in md.AssemblyReferences)
{
    var a = md.GetAssemblyReference(h);
    var pkt = md.GetBlobBytes(a.PublicKeyOrToken);
    var hv = md.GetBlobBytes(a.HashValue);
    L($"{MetadataTokens.GetRowNumber(h),3} | {md.GetString(a.Name)} | {a.Version} | " +
      $"0x{(uint)a.Flags:X8} | {pkt.Length} | {(pkt.Length > 0 ? Convert.ToHexString(pkt) : "-")} | " +
      $"'{md.GetString(a.Culture)}' | {hv.Length}");
}
L();

L("=== TYPEREF RESOLUTION SCOPES ===");
var hist = new SortedDictionary<string, int>();
var perAsm = new SortedDictionary<string, int>();
foreach (var h in md.TypeReferences)
{
    var tr = md.GetTypeReference(h);
    string scope;
    try
    {
        var rs = tr.ResolutionScope;
        if (rs.Kind == HandleKind.AssemblyReference)
        {
            scope = md.GetString(md.GetAssemblyReference((AssemblyReferenceHandle)rs).Name);
            perAsm.TryGetValue(scope, out var n); perAsm[scope] = n + 1;
        }
        else scope = rs.Kind.ToString();
    }
    catch (Exception ex) { scope = "EXCEPTION:" + ex.GetType().Name; }
    hist.TryGetValue(scope, out var c); hist[scope] = c + 1;
}
foreach (var kv in hist) L($"  {kv.Key,-46} {kv.Value}");
L();

L("=== PER-TYPE CENSUS (types of interest) ===");
L("type | methods | fields | properties | property names");
string[] want = {
  "Settings","ConfigSettings","Resources","ConfigPreferences","DefaultPreferences",
  "CustomSettingsProvider","CustomConfigSettingsProvider","DbManager","MainJSON","Encryption",
  "ProcessRunner","ProcessWatcher","ChildProcessTracker","NetworkManagement","NetworkStatusReceiver",
  "UpdateApi","V2RayConfig","V2RayService","ProxyServer","ProxyHandler","SshService","VpnService",
  "TetheringManager","App","MainWindow","MetroWindowCustom","Utility","Constants"
};
foreach (var th in md.TypeDefinitions)
{
    var t = md.GetTypeDefinition(th);
    string ns = md.GetString(t.Namespace), nm = md.GetString(t.Name);
    if (Array.IndexOf(want, nm) < 0 || !ns.StartsWith("NetModMaterial")) continue;
    string full = ns + "." + nm;
    var props = new List<string>();
    foreach (var ph in t.GetProperties()) props.Add(md.GetString(md.GetPropertyDefinition(ph).Name));
    L($"{full} | {t.GetMethods().Count} | {t.GetFields().Count} | {props.Count} | {string.Join(", ", props)}");
}
L();

L("=== LARGEST TYPES BY METHOD COUNT (top 25) ===");
var sizes = new List<(string, int)>();
foreach (var th in md.TypeDefinitions)
{
    var t = md.GetTypeDefinition(th);
    string ns = md.GetString(t.Namespace), nm = Safe(md.GetString(t.Name));
    sizes.Add(((ns.Length > 0 ? ns + "." : "") + nm, t.GetMethods().Count));
}
foreach (var x in sizes.OrderByDescending(z => z.Item2).Take(25)) L($"  {x.Item2,5}  {x.Item1}");
L();

L("=== TOTAL METHODS OWNED BY TYPEDEFS ===");
L("sum = " + sizes.Sum(z => z.Item2) + "  (must equal the MethodDef row count)");
L("END");
w.Flush();

Console.WriteLine("TypeDef=" + md.TypeDefinitions.Count + "  MethodDef=" + md.MethodDefinitions.Count +
                  "  AssemblyRef=" + md.GetTableRowCount(TableIndex.AssemblyRef));
Console.WriteLine("per-type method sum = " + sizes.Sum(z => z.Item2));
Console.WriteLine("wrote " + outPath);
