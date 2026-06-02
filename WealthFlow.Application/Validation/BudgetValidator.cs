using FluentValidation;
using WealthFlow.Application.DTOs;

namespace WealthFlow.Application.Validation
{
    public class BudgetValidator : AbstractValidator<BudgetDto>
    {
        public BudgetValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Bütçe miktarı 0'dan büyük olmalıdır.");

            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12).WithMessage("Ay değeri 1 ile 12 arasında olmalıdır.");

            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100).WithMessage("Yıl değeri 2000 ile 2100 arasında olmalıdır.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Lütfen geçerli bir kategori seçin.");
        }
    }
}
