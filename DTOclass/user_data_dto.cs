using System.Collections.Generic;

namespace project_graduation.DTOclass
{
    // DTO used to return user information along with their scan results
    public class user_data_dto
    {
        // User ID
        public int id_user { get; set; }

        // User's name
        public string name_user { get; set; }

        // List of scan results associated with the user
        public List<ScanResultDto> scans { get; set; }
    }
}
