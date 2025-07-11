namespace project_graduation.DTOclass
{
    // DTO used to return scan result details
    public class ScanResultDto
    {
        // Unique identifier for the scan result
        public int scanId { get; set; }

        // Date when the scan was performed
        public DateTime scanDate { get; set; }

        // Result content of the scan
        public string scanResult { get; set; }
    }
}
