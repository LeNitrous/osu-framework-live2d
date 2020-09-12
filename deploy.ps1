$request = "https://api.nuget.org/v3/registration5-gz-semver2/osu.framework.live2d/index.json"
$entries = Invoke-WebRequest $request | ConvertFrom-Json | Select-Object -expand items

$year = (Get-Date).Year
$monthDay = [int]([String](Get-Date).Month + [String](Get-Date).Day)
$current = $entries[0].items.catalogEntry.version.Split(".")
$revision = 0

if ( ([int]($current[0]) -eq $year) -and ([int]($current[1]) -eq $monthDay) ) {
    $revision = [int]($current[2]) + 1
}

if ($monthDay.length -eq 2) {
    $monthDay = "0" + $monthDay
}

$version = [string]::Format("{0}.{1}.{2}", [string]$year, [string]$monthDay, [string]$revision)
& dotnet pack ./osu.Framework.Live2D/osu.Framework.Live2D.csproj -c Release -o output /p:Version=$version