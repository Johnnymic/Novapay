using FluentValidation;
using Nova.Application.Dto.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Validation
{
   public class TransferWalletRequestValidator
        : AbstractValidator<TransferWalletRequest>
    {
        public TransferWalletRequestValidator()
        {
            RuleFor(x => x.DestinationWalletId)
                .NotEmpty()
                .WithMessage(
                    "Destination wallet ID is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage(
                    "Transfer amount must be greater than zero.")
                .PrecisionScale(
                    precision: 18,
                    scale: 2,
                    ignoreTrailingZeros: true)
                .WithMessage(
                    "Amount must have a maximum of 2 decimal places.");

            RuleFor(x => x.Reference)
                .NotEmpty()
                .WithMessage(
                    "Transfer reference is required.")
                .MaximumLength(100)
                .WithMessage(
                    "Transfer reference cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage(
                    "Description cannot exceed 500 characters.");
        }
    }
}
