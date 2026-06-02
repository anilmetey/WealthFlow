using FluentValidation;
using WealthFlow.Application.DTOs;

namespace WealthFlow.Application.Validation
{
    public class TransactionValidator : AbstractValidator<TransactionDto>
    {
        public TransactionValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama alanı boş olamaz.")
                .MaximumLength(100).WithMessage("Açıklama en fazla 100 karakter olmalıdır.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Tutar 0'dan büyük olmalıdır.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Tarih alanı seçilmelidir.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Lütfen geçerli bir kategori seçin.");
        }
    }
}
