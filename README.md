# Azuro Enterprise Library

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![NuGet](https://img.shields.io/badge/NuGet-Available-green.svg)](https://www.nuget.org/)

A comprehensive .NET 8 enterprise library providing common utilities, base classes, and Azure integrations for building robust web applications and Azure Functions. Released as open source for educational purposes.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Getting Started](#getting-started)
- [Architecture Overview](#architecture-overview)
- [Core Components](#core-components)
- [Usage Examples](#usage-examples)
- [Build and Test](#build-and-test)
- [Contributing](#contributing)
- [Educational Purpose](#educational-purpose)
- [License](#license)

## Overview

The Azuro Enterprise Library was developed to standardize common patterns and utilities across enterprise applications. It provides:

- **Base Classes**: Standardized controllers and Azure Functions with built-in logging, validation, and configuration
- **Azure Integration**: Seamless integration with Azure Key Vault, Blob Storage, and other Azure services
- **HTTP Utilities**: Robust HTTP client helpers with retry logic and claims forwarding
- **Security**: Claims-based authentication utilities and secure configuration management
- **Validation**: Extended validation framework with service provider integration
- **Utilities**: Common type conversion, password generation, and data manipulation utilities

## Features

### 🏗️ **Base Classes**
- `AControllerBase<TController, TAppSettings>` - ASP.NET Core controller base class
- `AFunction<TFunction>` - Azure Functions base class with logging and validation

### ☁️ **Azure Services Integration**
- Azure Key Vault secret management
- Azure Blob Storage operations
- Azure Functions security key handling

### 🌐 **HTTP & Web Utilities**
- Injectable HTTP client factory pattern
- Automatic retry logic and error handling
- Claims principal forwarding between services
- Request/response serialization helpers

### 🔒 **Security Features**
- JSON-based claims principal serialization
- Secure configuration management
- Password generation with customizable attributes

### 🛠️ **Development Utilities**
- Type-safe conversion methods
- Data validation framework
- Exception unpacking and logging
- XML serialization helpers

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Azure subscription (for Azure services)

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Azuro-Solutions/EnterpriseLibrary.git
   cd EnterpriseLibrary
   ```

2. **Build the solution:**
   ```bash
   cd Azuro.EnterpriseLibrary
   dotnet build
   ```

3. **Run tests:**
   ```bash
   dotnet test Azuro.EnterpriseLibrary.Tests/
   ```

4. **Package for NuGet (optional):**
   ```bash
   dotnet pack Azuro.CommonCore/Azuro.CommonCore.csproj
   ```

### NuGet Package Installation

```xml
<PackageReference Include="Azuro.CommonCore" Version="2.0.7-prerelease-5" />
```

## Architecture Overview

The library follows a modular architecture organized by functional domains:

```
Azuro.CommonCore/
├── AzureStorage/          # Azure Blob Storage utilities
├── Extensions/            # Extension methods for common types
├── Functions/             # Azure Functions base classes
├── Http/                  # HTTP client utilities and result types
├── KeyVault/              # Azure Key Vault integration
├── Messaging/             # XML serialization and messaging
├── Security/              # Claims-based security utilities
├── Text/                  # Text processing utilities
├── Validation/            # Extended validation framework
├── Web/                   # ASP.NET Core base classes
└── Utility.cs             # Common utility methods
```

## Core Components

### Base Controller Class

The `AControllerBase<TController, TAppSettings>` provides a foundation for ASP.NET Core controllers:

```csharp
public class MyController : AControllerBase<MyController, MyAppSettings>
{
    public MyController(ILogger<MyController> logger,
                       IConfiguration configuration,
                       IOptions<MyAppSettings> appSettings,
                       HttpInjectableHelper http)
        : base(logger, configuration, appSettings, http)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        // Access to _logger, _config, _appSettings, _httpHelper
        var functionUrl = CreateFunctionUrl("api/data");
        var result = await _httpHelper.GetAsync<DataModel>(functionUrl);
        return HandleApiResult(result);
    }
}
```

### Azure Functions Base Class

The `AFunction<TFunction>` provides standardized logging and validation for Azure Functions:

```csharp
public class MyFunction : AFunction<MyFunction>
{
    public MyFunction(ILogger<MyFunction> logger, IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    [FunctionName("ProcessData")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var model = await GetPostBodyAsync<DataModel>(req);
            var (isValid, errors) = ValidateObject(model);
            
            if (!isValid)
                return new BadRequestObjectResult(errors);

            // Process data
            LogInfo("Data processed successfully");
            return new OkObjectResult("Success");
        }
        catch (Exception ex)
        {
            return PackageErrorResult(ex);
        }
    }
}
```

### Azure Key Vault Integration

```csharp
// Configuration
services.Configure<KeyVaultConfig>(options =>
{
    options.VaultName = "my-keyvault";
    options.AllowInteractiveLogin = false;
});

services.AddTransient<ISecretHelper, KeyVaultHelper>();

// Usage
public class MyService
{
    private readonly ISecretHelper _secretHelper;

    public MyService(ISecretHelper secretHelper)
    {
        _secretHelper = secretHelper;
    }

    public async Task<string> GetConnectionString()
    {
        return await _secretHelper.GetSecretAsync("DatabaseConnection");
    }
}
```

### HTTP Client Utilities

```csharp
public class ApiService
{
    private readonly HttpInjectableHelper _httpHelper;

    public ApiService(HttpInjectableHelper httpHelper)
    {
        _httpHelper = httpHelper;
    }

    public async Task<TResult> CallApi<TResult>(string endpoint, object payload)
    {
        var result = await _httpHelper.PostAsync<TResult>(endpoint, payload);
        return result.Item2; // HttpResult contains status and data
    }
}
```

## Usage Examples

### Password Generation

```csharp
// Generate a 12-character password with all character types
string password = Utility.GeneratePassword(12, PasswordAttributes.All);

// Generate letters and digits only
string simplePassword = Utility.GeneratePassword(8, PasswordAttributes.LettersAndDigits);
```

### Type-Safe Conversions

```csharp
// Safe type conversions from database fields
int userId = Utility.FieldValueToInt32(dataRow["UserId"], -1);
DateTime? lastLogin = Utility.FieldValueToDateTime(dataRow["LastLogin"], null);
decimal amount = Utility.FieldValueToDecimal(dataRow["Amount"], 0.0m);
```

### Exception Handling

```csharp
try
{
    // Some operation
}
catch (Exception ex)
{
    string detailedError = Utility.UnpackException(ex);
    _logger.LogError(detailedError);
}
```

## Build and Test

### Development Commands

All commands should be run from the `Azuro.EnterpriseLibrary/` directory:

```bash
# Build the solution
dotnet build

# Run all tests
dotnet test Azuro.EnterpriseLibrary.Tests/

# Run tests with coverage
dotnet test Azuro.EnterpriseLibrary.Tests/ --configuration Release \
  /p:CollectCoverage=true /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=TestResults/Coverage/

# Package for NuGet
dotnet pack Azuro.CommonCore/Azuro.CommonCore.csproj

# Run console tester
dotnet run --project Azuro.EnterpriseLibrary.ConsoleTester
```

### Testing Framework

The library uses MSTest with code coverage:
- **MSTest** for unit testing
- **Coverlet** for code coverage analysis
- **SonarCloud** for code quality analysis (in CI/CD)

Example test structure:
```csharp
[TestClass]
public class UtilityTests
{
    [TestMethod]
    public void GeneratePassword_WithValidLength_ReturnsCorrectLength()
    {
        var password = Utility.GeneratePassword(10);
        Assert.AreEqual(10, password.Length);
    }
}
```

## Contributing

We welcome contributions to improve this library! Please follow these guidelines:

1. **Fork the repository** and create a feature branch
2. **Write tests** for new functionality
3. **Follow C# coding conventions** and existing patterns
4. **Update documentation** for new features
5. **Run all tests** before submitting
6. **Submit a pull request** with a clear description

### Development Environment Setup

1. Install .NET 8.0 SDK
2. Clone the repository
3. Open in Visual Studio 2022 or VS Code
4. Install required extensions (C# Dev Kit for VS Code)
5. Run `dotnet restore` to restore packages

## Educational Purpose

This library is released as open source for educational purposes to demonstrate:

- **Enterprise .NET Architecture**: How to structure reusable enterprise libraries
- **Azure Integration Patterns**: Best practices for Azure service integration
- **Dependency Injection**: Proper DI patterns in .NET applications
- **Logging and Monitoring**: Structured logging throughout applications
- **Security Patterns**: Claims-based authentication and secure configuration
- **Testing Strategies**: Unit testing with mocking and code coverage
- **CI/CD Practices**: Automated builds, testing, and package deployment

### Learning Outcomes

Developers can learn from this codebase:
- How to create flexible base classes for common patterns
- Azure service integration techniques
- HTTP client factory patterns and retry logic
- Configuration management best practices
- Validation framework integration
- Error handling and logging strategies
- NuGet package creation and distribution

## Dependencies

Key external dependencies:
- **Azure.Identity** (1.11.3) - Azure authentication
- **Azure.Security.KeyVault.Secrets** (4.6.0) - Key Vault integration
- **Azure.Storage.Blobs** (12.19.1) - Blob storage operations
- **Microsoft.AspNetCore.*** (2.2.x) - Web functionality
- **Microsoft.Extensions.*** (8.0.0) - Dependency injection and configuration

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For questions about this educational resource:
- Open an issue in this repository
- Review the code examples and documentation
- Check the unit tests for usage patterns

---

**Note**: This library was developed by Azuro Solutions and is shared for educational purposes. While functional, it should be adapted and tested thoroughly before use in production environments.