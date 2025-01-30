namespace Assets.Scripts.Platform.Adapter
{
    public class SocialProfile
    {
        public string Uid { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Avatar { get; set; } = "";
        public string Email { get; set; } = "";
		public string Currency { get; set; } = "USD";

        public override string ToString()
        {
            return $"{Uid}; fn:{FirstName}; ln:{LastName}; avatar:{Avatar}; email:{Email}";
        }
    }
}