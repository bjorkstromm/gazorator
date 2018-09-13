using System;
using System.Collections.Generic;

namespace AndroidBinderator
{
	public class BindingProjectModel
	{
		public string Id { get; private set; } = Guid.NewGuid().ToString().ToUpperInvariant();

        public string Name { get; set; } = string.Empty;

		public string MavenGroupId { get; set; } = string.Empty;

        public List<MavenArtifactModel> MavenArtifacts { get; set; } = new List<MavenArtifactModel>();

		public string NuGetPackageId { get; set; } = string.Empty;
        public string NuGetVersion { get; set; } = string.Empty;

        public string AssemblyName { get; set; } = string.Empty;

        public List<NuGetDependencyModel> NuGetDependencies { get; set; } = new List<NuGetDependencyModel>();

		public List<string> ProjectReferences { get; set; } = new List<string>();
	}
}