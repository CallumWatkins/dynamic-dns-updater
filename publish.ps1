$dockerHubUsername = "callumwatkins"
$imageName = "dynamicdnsupdater"
$architectures = @("amd64", "arm32v7", "arm64v8")
$versionTag = "1.0.1"
$includeLatestTag = $true

$tags = @($versionTag)
if ($includeLatestTag) {
    $tags += "latest"
}

Write-Host "The following tags will be published to ${dockerHubUsername}/${imageName}: {$($architectures -join ', ')}-{$($tags -join ', ')}"
$response = Read-Host "Do you wish to proceed? (yes/no)"

if ($response -ne "yes") {
    Write-Host "Operation cancelled by user."
    exit
}

foreach ($architecture in $architectures) {
    foreach ($tag in $tags) {
        $fullTag = "${dockerHubUsername}/${imageName}:${architecture}-${tag}"
        Write-Host "Building image for $architecture, version $tag..."
        docker build -t $fullTag -f "${architecture}.Dockerfile" .
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully built $fullTag"
            Write-Host "Pushing $fullTag..."
            docker push $fullTag
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Successfully pushed $fullTag"
            } else {
                Write-Host "Failed to push $fullTag"
            }
        } else {
            Write-Host "Failed to build $fullTag"
        }
    }
}

Write-Host "Complete."
