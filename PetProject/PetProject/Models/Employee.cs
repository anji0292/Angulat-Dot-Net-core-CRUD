using Newtonsoft.Json;

namespace PetProject.Models
{
    public class Employee
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("phone")]
        public string PhoneNumber { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("gender")]
        public string Gender { get; set; }
        [JsonProperty("dob")]
        public string DOB { get; set; }
        [JsonProperty("age")]
        public int ?Age { get; set; }

    }
}