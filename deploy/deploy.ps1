<#
.SYNOPSIS
    构建并打包 LeatherNesting 桌面应用的 Windows 自包含单文件发布包。

.DESCRIPTION
    使用 dotnet publish 将 src/LeatherNesting.Desktop 发布为 win-x64 自包含单文件
    （最终用户无需安装 .NET 运行时），并把产物打包成 zip 放到 deploy/artifacts/ 下。

    发布参数要点：
      - --self-contained      自包含，携带 .NET 运行时
      - PublishSingleFile     单文件 exe
      - IncludeNativeLibrariesForSelfExtract  Avalonia 原生库（Skia 等）运行时解包
      - DebugType=None        不生成 .pdb，缩小交付体积

.EXAMPLE
    .\deploy.ps1
    默认发布 Release / win-x64，产物打包到 deploy/artifacts/。

.EXAMPLE
    .\deploy.ps1 -Version 1.2.0 -SkipZip
    指定版本号并跳过 zip 打包，仅保留 publish 目录。

.NOTES
    需要 .NET 10 SDK（见仓库根 global.json）。
    在 Windows 上 PowerShell 运行：  .\deploy\deploy.ps1
#>
param(
    # 构建配置，默认 Release
    [string]$Configuration = "Release",

    # 目标运行时标识（RID），本软件仅支持 win-x64
    [string]$Runtime = "win-x64",

    # 版本号（用于 zip 文件名，不改变程序集版本）。默认取当天日期。
    [string]$Version,

    # 发布输出根目录（相对本脚本所在 deploy 目录）
    [string]$OutputDir = "artifacts",

    # 是否跳过 zip 打包，仅保留 publish 目录
    [switch]$SkipZip,

    # 是否关闭自包含（框架依赖）。默认自包含。
    [switch]$NoSelfContained
)

$ErrorActionPreference = "Stop"

# ---------- 路径解析 ----------
$DeployDir  = $PSScriptRoot                        # deploy/ 目录
$RepoRoot   = Split-Path -Parent $DeployDir         # 仓库根目录
$Project    = Join-Path $RepoRoot "src/LeatherNesting.Desktop/LeatherNesting.Desktop.csproj"
$Artifacts  = Join-Path $DeployDir $OutputDir
$PublishDir = Join-Path $Artifacts "publish"

if (-not (Test-Path $Project)) {
    throw "未找到项目文件：$Project"
}

if (-not $Version) {
    $Version = Get-Date -Format "yyyyMMdd"
}

# ---------- 检查 dotnet ----------
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    throw "未找到 dotnet。请安装 .NET 10 SDK（https://dotnet.microsoft.com/download）。"
}

Write-Host "== 开始发布 LeatherNesting.Desktop ==" -ForegroundColor Cyan
Write-Host "  项目        : $Project"
Write-Host "  配置/运行时 : $Configuration / $Runtime"
Write-Host "  自包含      : $(-not $NoSelfContained)"
Write-Host "  输出目录    : $PublishDir"
Write-Host ""

# ---------- 清理旧产物 ----------
if (Test-Path $PublishDir) {
    Write-Host "清理旧产物：$PublishDir"
    Remove-Item $PublishDir -Recurse -Force
}

# ---------- 组装发布参数 ----------
$publishArgs = @(
    "publish", $Project,
    "-c", $Configuration,
    "-r", $Runtime,
    "-o", $PublishDir,
    "--nologo"
)

if ($NoSelfContained) {
    $publishArgs += "--self-contained", "false"
} else {
    $publishArgs += "--self-contained", "true"
    $publishArgs += "-p:PublishSingleFile=true"
    $publishArgs += "-p:IncludeNativeLibrariesForSelfExtract=true"
    $publishArgs += "-p:EnableCompressionInSingleFile=true"
}

# 发布产物不生成 .pdb（缩体积）。如需调试符号，删除下面这行。
$publishArgs += "-p:DebugType=None"

# ---------- 执行发布 ----------
Write-Host "执行：dotnet $($publishArgs -join ' ')" -ForegroundColor DarkGray
Write-Host ""
& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish 失败，退出码 $LASTEXITCODE"
}

# ---------- 校验产物 ----------
$exe = Join-Path $PublishDir "LeatherNesting.Desktop.exe"
if (-not (Test-Path $exe)) {
    throw "发布完成但未找到可执行文件：$exe"
}

$size = [math]::Round((Get-Item $exe).Length / 1MB, 2)
Write-Host ""
Write-Host "发布完成：$exe ($size MB)" -ForegroundColor Green

# ---------- 打包 zip ----------
if (-not $SkipZip) {
    $zipName = "LeatherNesting.Desktop-$Runtime-$Version.zip"
    $zipPath = Join-Path $Artifacts $zipName

    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }

    Write-Host "打包：$zipPath"
    Compress-Archive -Path (Join-Path $PublishDir "*") -DestinationPath $zipPath -Force

    $zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
    Write-Host "打包完成：$zipPath ($zipSize MB)" -ForegroundColor Green
    Write-Host ""
    Write-Host "== 部署包已就绪 ==" -ForegroundColor Cyan
    Write-Host "  目标机器直接双击解压出的 LeatherNesting.Desktop.exe 即可运行（无需安装 .NET）。"
} else {
    Write-Host ""
    Write-Host "已跳过 zip 打包。产物目录：$PublishDir"
}
