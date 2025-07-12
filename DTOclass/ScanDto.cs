namespace project_graduation.DTOclass
{
    public class SubmitScanDto
    {
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
    }

    public class ScanDto
    {
        public int UrlId { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string? Result { get; set; }
    }
}