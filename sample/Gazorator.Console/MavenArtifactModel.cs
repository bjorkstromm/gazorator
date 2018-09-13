namespace AndroidBinderator
{
	public class MavenArtifactModel
    {
		public string MavenGroupId { get; set; } = string.Empty;
        public string MavenArtifactId { get; set; } = string.Empty;
        public string MavenArtifactVersion { get; set; } = string.Empty;
        public string MavenArtifactPackaging { get; set; } = string.Empty;

        public string DownloadedArtifact { get; set; } = string.Empty;
        public string ProguardFile { get; set; } = string.Empty;
    }
}