# Haitch.Unions

Result and Optional union types for .NET with fluent, async-friendly pipelines and typed errors. Part of the [Haitch](https://haitch.dev) suite of foundational libraries.

## Install

```shell
dotnet add package Haitch.Unions
dotnet add package Haitch.Unions.AspNetCore        # Result → HTTP results and ProblemDetails
dotnet add package Haitch.Unions.TUnit.Assertions  # TUnit assertions
```

## Quick start

```csharp
using Haitch.Unions;

Result<User> GetUser(int id) =>
    repository.Find(id) is { } user
        ? user
        : new NotFoundError("User.NotFound", $"User {id} not found");

var name = GetUser(42)
    .Ensure(user => user.IsActive, new ValidationError("User.Inactive", "User is inactive"))
    .Map(user => user.DisplayName)
    .Match(
        onOk: displayName => displayName,
        onError: _ => "Anonymous");
```

Both are C# unions, so they work with `switch` expressions.

## Documentation

Full documentation is at [haitch.dev/libraries/unions](https://haitch.dev/libraries/unions/).
