#!/bin/pwsh

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

Set-Location $PSScriptRoot

$testProjects = @(
    "./Nancy/Nancy.Tests/Nancy.Tests.csproj",
    "./Nancy/Nancy.Tests/Nancy.Tests.LongRational.csproj",
    "./Nancy/Nancy.Expressions.Tests/Nancy.Expressions.Tests.csproj",
    "./Nancy/Nancy.Expressions.Tests/Nancy.Expressions.Tests.Local.csproj",
    "./Nancy/Nancy.Plots/Nancy.Plots.Tikz.Tests/Nancy.Plots.Tikz.Tests.csproj",
    "./Nancy/Nancy.Plots/Nancy.Plots.ScottPlot.Tests/Nancy.Plots.ScottPlot.Tests.csproj",
    "./Nancy/Nancy.Plots/Nancy.Plots.XPlot.Plotly.Tests/Nancy.Plots.XPlot.Plotly.Tests.csproj"
)

function Invoke-Checked {
    param(
        [string] $Command,
        [string[]] $Arguments
    )

    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code ${LASTEXITCODE}: $Command $($Arguments -join ' ')"
    }
}

function Get-ReportGenerator {
    $globalTool = Get-Command "reportgenerator" -ErrorAction SilentlyContinue
    if ($globalTool) {
        return $globalTool.Source
    }

    $toolPath = Join-Path $PSScriptRoot ".tools"
    $toolExecutable = if ($IsWindows) { "reportgenerator.exe" } else { "reportgenerator" }
    $localTool = Join-Path $toolPath $toolExecutable

    if (-not (Test-Path $localTool)) {
        Write-Host "Installing ReportGenerator locally..." -ForegroundColor Yellow
        Invoke-Checked "dotnet" @(
            "tool",
            "install",
            "dotnet-reportgenerator-globaltool",
            "--tool-path",
            $toolPath,
            "--version",
            "5.5.9"
        )
    }

    return $localTool
}

Write-Host "Cleaning previous test results and coverage reports..." -ForegroundColor Yellow
Get-ChildItem -Recurse -Directory -Filter "TestResults" | Remove-Item -Recurse -Force
if (Test-Path "coveragereport") {
    Remove-Item "coveragereport" -Recurse -Force
}

Write-Host "Running tests with coverage collection..." -ForegroundColor Yellow
foreach ($testProject in $testProjects) {
    Write-Host "Testing $testProject" -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "test",
        "--project",
        $testProject,
        "--configuration",
        "Release",
        "--framework",
        "net10.0",
        "--coverlet",
        "--coverlet-output-format",
        "cobertura"
    )
}

Write-Host "Generating coverage report..." -ForegroundColor Yellow
$reportGenerator = Get-ReportGenerator
Invoke-Checked $reportGenerator @(
    "-reports:**/coverage.cobertura*.xml",
    "-targetdir:coveragereport",
    "-assemblyfilters:+Unipi.Nancy*;-Unipi.Nancy*.Tests*;-Nancy.Plots.*.Tests*",
    "-reporttypes:Html;TextSummary"
)

Write-Host "Coverage report generated in 'coveragereport'." -ForegroundColor Green
Write-Host "Summary:" -ForegroundColor Green
Get-Content -Path "coveragereport/Summary.txt"
