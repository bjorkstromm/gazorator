namespace AndroidBinderator
{
    public class NuGetDependencyModel
    {
        public bool IsProjectReference { get; set; }

        public string NuGetPackageId { get; set; } = string.Empty;
        public string NuGetVersion { get; set; } = string.Empty;

        public MavenArtifactModel MavenArtifact { get; set; }
    }
}