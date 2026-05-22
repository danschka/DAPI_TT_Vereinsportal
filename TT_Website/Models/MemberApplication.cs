namespace TT_Website.Models
{
    public class MemberApplication
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";

        public DateTime BirthDate { get; set; }
        public string Address { get; set; } = "";

        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
