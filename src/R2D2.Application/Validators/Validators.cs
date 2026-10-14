using FluentValidation;
using R2D2.Application.DTOs;

namespace R2D2.Application.Validators;

public class CreateDynamicFieldValidator : AbstractValidator<CreateDynamicFieldDto>
{
    public CreateDynamicFieldValidator()
    {
        RuleFor(x => x.TechnicalName)
            .NotEmpty().WithMessage("Technical name is required")
            .MaximumLength(100).WithMessage("Technical name must not exceed 100 characters")
            .Matches("^[a-zA-Z_][a-zA-Z0-9_]*$").WithMessage("Technical name must be a valid identifier");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required")
            .MaximumLength(200).WithMessage("Label must not exceed 200 characters");

        RuleFor(x => x.FieldType)
            .IsInEnum().WithMessage("Invalid field type");

        RuleFor(x => x.MaxLength)
            .GreaterThan(0).When(x => x.MaxLength.HasValue)
            .WithMessage("Max length must be greater than 0");
    }
}

public class CreateDynamicListValidator : AbstractValidator<CreateDynamicListDto>
{
    public CreateDynamicListValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleForEach(x => x.Fields)
            .SetValidator(new CreateDynamicFieldValidator())
            .When(x => x.Fields != null);
    }
}

public class CreateDynamicFormValidator : AbstractValidator<CreateDynamicFormDto>
{
    public CreateDynamicFormValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}

public class CreateBusinessRuleValidator : AbstractValidator<CreateBusinessRuleDto>
{
    public CreateBusinessRuleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid rule type");

        RuleFor(x => x.ConfigurationJson)
            .NotEmpty().WithMessage("Configuration is required");
    }
}

public class CreateWorkflowValidator : AbstractValidator<CreateWorkflowDto>
{
    public CreateWorkflowValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.FlowJson)
            .NotEmpty().WithMessage("Flow configuration is required");
    }
}

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(100).WithMessage("Username must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}
