# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is Azuro Enterprise Library, a .NET 8 library that provides common utilities and base classes for Azuro Solutions applications. The library is packaged as a NuGet package and includes functionality for Azure services, HTTP operations, security, and web/function application base classes.

## Solution Structure

- **Azuro.CommonCore** - Main library project containing all utility classes and base classes
- **Azuro.EnterpriseLibrary.Tests** - MSTest unit test project  
- **Azuro.EnterpriseLibrary.ConsoleTester** - Console application for testing library functionality

The main library is organized into functional modules:
- `AzureStorage/` - Azure Blob Storage helpers
- `Http/` - HTTP client utilities and result types
- `KeyVault/` - Azure Key Vault integration
- `Security/` - Claims-based security utilities
- `Web/` - Base controller class (`AControllerBase`) for ASP.NET Core APIs
- `Functions/` - Base class (`AFunction`) for Azure Functions
- `Messaging/` - XML serialization utilities
- `Validation/` - Extended validation base classes

## Build and Development Commands

**Note:** All commands should be run from the `Azuro.EnterpriseLibrary/` directory.

### Build
```bash
dotnet build
```

### Run Tests
```bash
dotnet test Azuro.EnterpriseLibrary.Tests/Azuro.EnterpriseLibrary.Tests.csproj
```

### Run Tests with Coverage
```bash
dotnet test Azuro.EnterpriseLibrary.Tests/Azuro.EnterpriseLibrary.Tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=TestResults/Coverage/
```

### Package for NuGet
```bash
dotnet pack Azuro.CommonCore/Azuro.CommonCore.csproj
```

### Console Tester
```bash
dotnet run --project Azuro.EnterpriseLibrary.ConsoleTester
```

## Key Architecture Patterns

### Base Classes
The library provides two main base classes that applications should inherit from:

1. **AControllerBase<TController, TAppSettings>** (`Web/AControllerBase.cs:10`) - Base class for ASP.NET Core controllers
   - Provides dependency injection for logger, configuration, app settings, and HTTP helper
   - Built-in Azure Functions URL generation with security key handling
   - Exception handling and HTTP result processing utilities

2. **AFunction<TFunction>** (`Functions/AFunction.cs:18`) - Base class for Azure Functions  
   - Logging wrapper methods for all log levels
   - JSON serialization/deserialization helpers
   - Validation framework integration
   - Claims principal extraction from HTTP headers

### HTTP Operations
The `HttpInjectableHelper` and `HttpRequestHelper` classes provide standardized HTTP client functionality with:
- Automatic retry logic
- Error handling and exception wrapping
- Claims principal forwarding between services
- Azure Functions security key management

### Configuration Pattern
The library uses a pattern where applications implement `IAppSettings` to define their configuration contract. The base classes automatically bind configuration from both `IConfiguration` and strongly-typed options.

## Dependencies

Key external dependencies:
- Azure.Identity (1.11.3)
- Azure.Security.KeyVault.Secrets (4.6.0) 
- Azure.Storage.Blobs (12.19.1)
- Microsoft.AspNetCore.* (2.2.x) for web functionality
- Microsoft.Extensions.* (8.0.0) for dependency injection and configuration

## Testing Framework

Uses MSTest with the following packages:
- MSTest.TestAdapter and MSTest.TestFramework (2.2.9)
- coverlet.msbuild (3.1.2) for code coverage

## CI/CD Pipeline

The project uses Azure DevOps Pipelines with:
- SonarCloud integration for code quality analysis
- Automated testing with coverage reporting
- NuGet package publishing to both GitHub and Azure DevOps feeds
- Build configuration supports both Debug and Release