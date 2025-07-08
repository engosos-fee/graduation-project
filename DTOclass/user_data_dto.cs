namespace project_graduation.DTOclass
{
    // DTO used to send user data and scan results to the frontend
    public class user_data_dto
    {
        // The user's unique ID
        public int id_user { get; set; }

        // The user's name
        public string name_user { get; set; }

        // List of scan result strings associated with the user
        public List<string> result { get; set; }
    }
}
