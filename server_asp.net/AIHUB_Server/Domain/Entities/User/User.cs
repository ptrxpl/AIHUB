using System.Text.Json.Serialization;

namespace AIHUB_Server.Domain.Entities.Users
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }

        [JsonIgnore] // Prevents the password property from being serialized and returned in api responses.
        public string Password { get; set; }
    }
}
