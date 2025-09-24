namespace TitleVerification.Api.Models
{
    public class LandRecord
    {
        // Composite unique constraint on LandID, Latitude, Longitude will be enforced in DbContext/migration
        public int Id { get; set; }
        public string LandID { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AadhaarNumber { get; set; } = string.Empty;
        public string OwnershipType { get; set; } = string.Empty; // Self, Inherited
        public bool SiblingApproval { get; set; }
        public bool LoanDisputeStatus { get; set; } // true => either loan or dispute exists
        public string LandType { get; set; } = string.Empty; // Forest, Military, Government, Other
    }
}
