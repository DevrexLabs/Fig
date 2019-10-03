
configuration=$1
version=$2

echo $configuration
echo $version

nuget push Fig.Core/bin/$configuration/Fig.$version.nupkg -Source https://api.nuget.org/v3/index.json
nuget push Fig.AppSettingsxml/bin/$configuration/Fig.AppSettingsXml.$version.nupkg -Source https://api.nuget.org/v3/index.json
nuget push Fig.AppSettingsJson/bin/$configuration/Fig.AppSettingsJson.$version.nupkg -Source https://api.nuget.org/v3/index.json
