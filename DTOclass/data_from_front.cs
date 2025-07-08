namespace project_graduation.DTOclass
{
    // DTO used to receive URL data from the frontend
    public class data_from_front
    {
        // Identifier (optional, may be unused during creation)
        public int Id { get; set; }

        // The URL to be scanned or saved
        public string Url { get; set; }

        // The date the URL was submitted
        public DateTime Date { get; set; }

        // The ID of the user submitting the URL
        public int UserId { get; set; }
    }
}
