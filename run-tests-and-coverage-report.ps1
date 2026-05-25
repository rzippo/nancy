#!/bin/pwsh

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

Set-Location $PSScriptRoot

# Release/net10.0 currently produces zero-hit Coverlet data for some instrumented assemblies.
$coverageConfiguration = if ($env:NANCY_COVERAGE_CONFIGURATION) {
    $env:NANCY_COVERAGE_CONFIGURATION
} else {
    "Debug"
}

$testProjects = @(
    @{
        Path = "./Nancy/Nancy.Tests/Nancy.Tests.csproj"
        CoverletIncludes = @("[Unipi.Nancy]*", "[Unipi.Nancy.UncheckedInternals]*")
    },
    @{
        Path = "./Nancy/Nancy.Tests/Nancy.Tests.LongRational.csproj"
        CoverletIncludes = @("[Unipi.Nancy.LongRational]*", "[Unipi.Nancy.UncheckedInternals]*")
    },
    @{
        Path = "./Nancy/Nancy.Expressions.Tests/Nancy.Expressions.Tests.csproj"
        CoverletIncludes = @("[Unipi.Nancy.Expressions]*")
    },
    @{
        Path = "./Nancy/Nancy.Expressions.Tests/Nancy.Expressions.Tests.Local.csproj"
        CoverletIncludes = @("[Unipi.Nancy.Expressions.Local]*")
    },
    @{
        Path = "./Nancy/Nancy.Plots/Nancy.Plots.Tikz.Tests/Nancy.Plots.Tikz.Tests.csproj"
        CoverletIncludes = @("[Unipi.Nancy.Plots.Tikz]*")
    },
    @{
        Path = "./Nancy/Nancy.Plots/Nancy.Plots.ScottPlot.Tests/Nancy.Plots.ScottPlot.Tests.csproj"
        CoverletIncludes = @("[Unipi.Nancy.Plots.ScottPlot]*")
    },
    @{
        Path = "./Nancy/Nancy.Plots/Nancy.Plots.XPlot.Plotly.Tests/Nancy.Plots.XPlot.Plotly.Tests.csproj"
        CoverletIncludes = @("[Unipi.Nancy.Plots.XPlot.Plotly]*")
    }
)

function Invoke-Checked {
    param(
        [string] $Command,
        [string[]] $Arguments
    )

    $processStartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $processStartInfo.FileName = $Command

    foreach ($argument in $Arguments) {
        [void] $processStartInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::Start($processStartInfo)
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        throw "Command failed with exit code $($process.ExitCode): $Command $($Arguments -join ' ')"
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
Write-Host "Using $coverageConfiguration configuration for coverage collection." -ForegroundColor Cyan
foreach ($testProject in $testProjects) {
    Write-Host "Testing $($testProject.Path)" -ForegroundColor Cyan

    $testArguments = @(
        "test",
        "--project",
        $testProject.Path,
        "--configuration",
        $coverageConfiguration,
        "--framework",
        "net10.0",
        "--coverlet",
        "--coverlet-output-format",
        "cobertura"
    )

    foreach ($coverletInclude in $testProject.CoverletIncludes) {
        $testArguments += @("--coverlet-include", $coverletInclude)
    }

    Invoke-Checked "dotnet" $testArguments
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
