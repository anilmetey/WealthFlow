namespace WealthFlow.Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-tag";
        public string Color { get; set; } = "#6366F1";
    }
}
