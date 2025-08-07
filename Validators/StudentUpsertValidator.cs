using FluentValidation;
using SchoolProject.Models.Models.RequestModels;

namespace SchoolProject.Validators
{
    public class StudentUpsertValidator : AbstractValidator<StudentUpsertRequest>
    {
        public StudentUpsertValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full Name is required.")
                .MaximumLength(30).WithMessage("Full Name must not exceed 30 characters.");

            RuleFor(x => x.Age)
                .NotNull().WithMessage("Age is required.")
                .InclusiveBetween(5, 100).WithMessage("Age must be between 6 and 100.");

            RuleFor(x => x.Standard)
                .NotNull().WithMessage("Standard is required.")
                .InclusiveBetween(1, 12).WithMessage("Standard must be between 1 and 12.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be valid.");

            RuleFor(x => x.ContactNumber)
                .Matches(@"^\d{10}$").When(x => !string.IsNullOrEmpty(x.ContactNumber))
                .WithMessage("Contact Number must be a valid 10-digit number.");
        }
    }
}
