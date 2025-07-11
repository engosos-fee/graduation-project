namespace project_graduation.DTOclass
{
    // This DTO is used to receive scanning request data from the frontend
    public class data_from_front
    {
        // The unique identifier for the data entry (can be auto-generated or unused in request)
        public int Id { get; set; }

        // The URL to be scanned
        public string Url { get; set; }

        // The date when the scan was requested or submitted
        public DateTime Date { get; set; }

        // The ID of the user who submitted the scan
        public int UserId { get; set; }
    }
}
