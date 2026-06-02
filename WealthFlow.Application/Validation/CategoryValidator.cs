using FluentValidation;
using WealthFlow.Application.DTOs;

namespace WealthFlow.Application.Validation
{
    public class CategoryValidator : AbstractValidator<CategoryDto>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Kategori adı boş olamaz.")
                .MaximumLength(50).WithMessage("Kategori adı en fazla 50 karakter olmalıdır.");

            RuleFor(x => x.Icon)
                .NotEmpty().WithMessage("İkon seçimi zorunludur.")
                .MaximumLength(50).WithMessage("İkon adı en fazla 50 karakter olmalıdır.");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Renk seçimi zorunludur.")
                .MaximumLength(20).WithMessage("Renk kodu en fazla 20 karakter olmalıdır.");
        }
    }
}
