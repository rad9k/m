using m0;
using m0.Store.FileSystem;
using m0.Store.Json;

string previousApplicationPath = MinusZero.Instance.ApplicationPath;
string testRoot = Path.Combine(
    Path.GetTempPath(),
    $"m0-start-path-smoke-{Guid.NewGuid():N}");
string applicationPath = Path.Combine(testRoot, "application");
string nestedPhysicalPath = Path.Combine(
    applicationPath,
    "KOTEK",
    "podkatalog",
    "plik.m0j");

Directory.CreateDirectory(
    Path.GetDirectoryName(nestedPhysicalPath)!);

try
{
    MinusZero.Instance.ApplicationPath = applicationPath;

    AssertEqual(
        "Start:/KOTEK/podkatalog/plik.m0j",
        FileSystemUtil.GetCanonicalFileIdentifier(
            nestedPhysicalPath));

    AssertEqual(
        FileSystemUtil.NormalizeFileSystemIdentifier(
            nestedPhysicalPath),
        FileSystemUtil.GetPhysicalFileName(
            "Start:/KOTEK/podkatalog/plik.m0j"));

    AssertEqual(
        FileSystemUtil.NormalizeFileSystemIdentifier(
            nestedPhysicalPath),
        FileSystemUtil.GetPhysicalFileName(
            @"Start:\KOTEK\podkatalog\plik.m0j"));

    FileSystemUtil.GetCanonicalStoreReference(
        typeof(JsonSerializationStore).AssemblyQualifiedName!,
        nestedPhysicalPath,
        10L,
        out string canonicalStoreIdentifier,
        out object canonicalVertexIdentifier);

    AssertEqual(
        "Start:/KOTEK/podkatalog/plik.m0j",
        canonicalStoreIdentifier);
    AssertEqual(10L, canonicalVertexIdentifier);

    FileSystemUtil.GetCanonicalStoreReference(
        typeof(FileSystemStore).AssemblyQualifiedName!,
        Path.GetPathRoot(applicationPath)!,
        nestedPhysicalPath,
        out canonicalStoreIdentifier,
        out canonicalVertexIdentifier);

    AssertEqual(
        FileSystemUtil.StartIdentifierPrefix,
        canonicalStoreIdentifier);
    AssertEqual(
        "Start:/KOTEK/podkatalog/plik.m0j",
        canonicalVertexIdentifier);

    bool traversalRejected = false;

    try
    {
        FileSystemUtil.GetPhysicalFileName(
            "Start:/../outside.txt");
    }
    catch (ArgumentException)
    {
        traversalRejected = true;
    }

    if (!traversalRejected)
        throw new InvalidOperationException(
            "Start:/ parent traversal was not rejected.");
}
finally
{
    MinusZero.Instance.ApplicationPath =
        previousApplicationPath;

    if (Directory.Exists(testRoot))
        Directory.Delete(testRoot, true);
}

static void AssertEqual(
    object expected,
    object actual)
{
    if (!Equals(expected, actual))
    {
        throw new InvalidOperationException(
            $"Expected '{expected}', actual '{actual}'.");
    }
}
