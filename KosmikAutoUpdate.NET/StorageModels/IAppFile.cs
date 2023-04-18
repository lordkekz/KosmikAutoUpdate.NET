namespace KosmikAutoUpdate.NET.StorageModels;

internal interface IAppFile {
    string RelativePath { get; }
    string FileHash { get; }
}