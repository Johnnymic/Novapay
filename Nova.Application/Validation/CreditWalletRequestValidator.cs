using FluentValidation;
using Nova.Application.Dto.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Validation
{
    
    public class CreditWalletRequestValidator
        : AbstractValidator<CreditWalletRequest>
    {
        public CreditWalletRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Credit amount must be greater than zero.")
                .PrecisionScale(
                    precision: 18,
                    scale: 2,
                    ignoreTrailingZeros: true)
                .WithMessage(
                    "Amount must have a maximum of 2 decimal places.");

            RuleFor(x => x.Reference)
                .NotEmpty()
                .WithMessage("Transaction reference is required.")
                .MaximumLength(100)
                .WithMessage(
                    "Transaction reference cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage(
                    "Description cannot exceed 500 characters.");
        }
    }
}
