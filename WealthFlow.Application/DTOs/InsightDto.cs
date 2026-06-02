namespace WealthFlow.Application.DTOs
{
    public class InsightDto
    {
        public string Type { get; set; } = "info"; // info, success, warning
        public string Message { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-circle-info";
    }
}
