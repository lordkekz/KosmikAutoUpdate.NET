namespace KosmikAutoUpdate.NET.StorageModels;

internal record LocalAppFile(string RelativePath, string FileHash, long SizeBytes) : IAppFile { }