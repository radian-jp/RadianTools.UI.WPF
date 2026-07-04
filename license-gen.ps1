<#
================================================================================
 Third Party License Collector (NuGet → GitHub LICENSE extractor)
--------------------------------------------------------------------------------
機能:
- csprojからNuGet依存関係を取得
- NuGet metadataからGitHub repositoryを特定
- GitHubからLICENSEファイルを収集
- Markdown or 個別ファイルに出力
- System / Microsoft系はデフォルト除外

出力モード:
- single: 1つのMarkdownにまとめる
- split : パッケージごとにLICENSEファイルを出力
================================================================================ #>

param(
    # 対象csproj（未指定ならカレントから自動検出）
    [string]$CsprojPath = "./src/RadianTools.UI.WPF/RadianTools.UI.WPF.csproj",
    
    # singleモード時の出力ファイル
    [string]$OutputFile = ".\THIRD_PARTY_LICENSES.md",
    
    # splitモード時の出力ディレクトリ
    [string]$OutputDir = ".\third-party-licenses",

    # GitHub APIトークン（レート制限対策）
    [string]$GitHubToken = $env:GITHUB_TOKEN,

    # 除外するパッケージプレフィックス
    [string[]]$ExcludePrefixes = @(
        "System.",
        "Microsoft.",
        "runtime.",
        "NETStandard.",
        "Microsoft.NETCore.",
        "Microsoft.AspNetCore."
    )
)

# ================================
# 出力モード
# ================================
# $OutputMode = "single"
$OutputMode = "split"

# ================================
# csproj resolve
# ================================
function Resolve-CsprojPath {
    param([string]$path)

    if ($path -and $path.Trim() -ne "") {
        return $path
    }

    $csprojFiles = Get-ChildItem -Path . -Filter *.csproj -File

    if ($csprojFiles.Count -eq 0) {
        throw "No .csproj file found."
    }

    return $csprojFiles[0].FullName
}

# ================================
# Package refs
# ================================
function Get-PackageRefs {
    param([string]$csproj)

    [xml]$xml = Get-Content $csproj
    $items = $xml.SelectNodes("//PackageReference")

    foreach ($i in $items) {
        [PSCustomObject]@{
            Id = $i.Include
            Version = $i.Version
        }
    }
}

# ================================
# exclude
# ================================
function Should-ExcludePackage {
    param([string]$id)

    foreach ($p in $ExcludePrefixes) {
        if ($id.StartsWith($p)) {
            return $true
        }
    }
    return $false
}

# ================================
# NuGet metadata
# ================================
function Get-NuGetMetadata {
    param($id, $version)

    $nuspecUrl = "https://api.nuget.org/v3-flatcontainer/$id/$version/$id.nuspec"

    try {
        Invoke-RestMethod $nuspecUrl -ErrorAction Stop
    } catch {
        $null
    }
}

# ================================
# GitHub repo
# ================================
function Get-GitHubRepoFromNuSpec {
    param($nuspec)

    if (-not $nuspec) { return $null }

    try {
        $repo = $nuspec.package.metadata.repository.url
        if ($repo -and $repo -match "github.com") {
            return $repo -replace "\.git$",""
        }
    } catch {}

    return $null
}

# ================================
# LICENSE fetch
# ================================
function Get-GitHubLicense {
    param([string]$repoUrl)

    if (-not $repoUrl) { return $null }

    $repoPath = $repoUrl -replace "https://github.com/", ""

    $candidates = @(
        "LICENSE",
        "LICENSE.txt",
        "LICENSE.md",
        "COPYING"
    )

    foreach ($file in $candidates) {

        $url = "https://raw.githubusercontent.com/$repoPath/master/$file"

        try {
            return Invoke-RestMethod $url -ErrorAction Stop
        } catch {}
    }

    return $null
}

# ================================
# main
# ================================
$CsprojPath = Resolve-CsprojPath $CsprojPath
Write-Host "Using csproj: $CsprojPath"

$packages = Get-PackageRefs $CsprojPath | Sort-Object Id -Unique

$result = @()

foreach ($pkg in $packages) {

    if (Should-ExcludePackage $pkg.Id) {
        Write-Host "Skipping $($pkg.Id)"
        continue
    }

    Write-Host "Processing $($pkg.Id) $($pkg.Version)..."

    $nuspec = Get-NuGetMetadata $pkg.Id $pkg.Version
    $repo = Get-GitHubRepoFromNuSpec $nuspec
    $license = Get-GitHubLicense $repo

    $result += [PSCustomObject]@{
        Package = $pkg.Id
        Version = $pkg.Version
        Repo    = $repo
        License = $license
    }
}

# ================================
# OUTPUT
# ================================
if ($OutputMode -eq "single") {

    # ★ StringBuilder化
    $sb = New-Object System.Text.StringBuilder

    [void]$sb.AppendLine("# Third Party Licenses")
    [void]$sb.AppendLine("")

    foreach ($r in $result) {

        [void]$sb.AppendLine("## $($r.Package) $($r.Version)")
        [void]$sb.AppendLine("")
        [void]$sb.AppendLine("Repo: $($r.Repo)")
        [void]$sb.AppendLine("")

        # code block（安全）
        [void]$sb.AppendLine('```text')

        if ($r.License) {
            [void]$sb.AppendLine($r.License)
        } else {
            [void]$sb.AppendLine("LICENSE NOT FOUND")
        }

        [void]$sb.AppendLine('```')
        [void]$sb.AppendLine("")
    }

    Set-Content -Path $OutputFile -Value $sb.ToString() -Encoding UTF8
    Write-Host "Output -> $OutputFile"
}

elseif ($OutputMode -eq "split") {

    if (-not (Test-Path $OutputDir)) {
        New-Item -ItemType Directory -Path $OutputDir | Out-Null
    }

    foreach ($r in $result) {

        $safeName = ($r.Package -replace "[^a-zA-Z0-9_\-\.]", "_")
        $filePath = Join-Path $OutputDir "$safeName`_license.txt"

        $content = if ($r.License) { $r.License } else { "LICENSE NOT FOUND" }

        Set-Content $filePath $content -Encoding UTF8

        Write-Host "Wrote $filePath"
    }

    Write-Host "Output dir -> $OutputDir"
}