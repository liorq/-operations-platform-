using FluentValidation.Results;
using OperationsPlatform.Core.Models;
using OperationsPlatform.Core.Validators;
using OperationsPlatform.Domain.Enums;
using Xunit;

namespace OperationsPlatform.Tests;

public class ValidatorTests
{
    private static OperationDto CreateDto(string key = "my-op", string name = "שם", string definition = "a + b")
    {
        return new OperationDto(0, key, name, null, OperationKind.Expression, InputType.Number, definition, true);
    }

    [Fact]
    public void CalculateRequest_ReportsEveryProblemAtOnce()
    {
        string tooLong = new string('x', 501);

        ValidationResult result = new CalculateRequestValidator().Validate(new CalculateRequest(tooLong, tooLong, ""));

        Assert.Equal(3, result.Errors.Count);
    }

    [Theory]
    [InlineData("Bad Key")]
    [InlineData("x")]
    [InlineData("UPPER")]
    [InlineData("ab\n")]
    public void OperationDto_BadKey_Fails(string key)
    {
        ValidationResult result = new OperationDtoValidator().Validate(CreateDto(key: key));

        Assert.Contains(result.Errors, error => error.PropertyName.Equals("Key", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OperationDto_EmptyNameAndDefinition_FailOnBoth()
    {
        ValidationResult result = new OperationDtoValidator().Validate(CreateDto(name: "", definition: ""));

        Assert.Contains(result.Errors, error => error.PropertyName.Equals("Name", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, error => error.PropertyName.Equals("Definition", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OperationDto_UnknownKindNumber_Fails()
    {
        OperationDto dto = new OperationDto(0, "my-op", "שם", null, (OperationKind)99, InputType.Number, "a + b", true);

        ValidationResult result = new OperationDtoValidator().Validate(dto);

        Assert.Contains(result.Errors, error => error.PropertyName.Equals("Kind", StringComparison.OrdinalIgnoreCase));
    }
}
