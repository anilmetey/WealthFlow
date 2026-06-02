using System;
using FluentValidation;
using WealthFlow.Application.DTOs;

namespace WealthFlow.Application.Validation
{
    public class FinancialGoalValidator : AbstractValidator<FinancialGoalDto>
    {
        public FinancialGoalValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Hedef başlığı boş bırakılamaz.")
                .MaximumLength(100).WithMessage("Hedef başlığı en fazla 100 karakter olmalıdır.");

            RuleFor(x => x.TargetAmount)
                .GreaterThan(0).WithMessage("Hedef miktarı 0'dan büyük olmalıdır.");

            RuleFor(x => x.CurrentAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Biriken miktar negatif olamaz.");

            RuleFor(x => x.TargetDate)
                .GreaterThan(DateTime.Today).WithMessage("Hedef tarihi bugünden ileriki bir tarih olmalıdır.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Lütfen geçerli bir kategori seçin.");
        }
    }
}
