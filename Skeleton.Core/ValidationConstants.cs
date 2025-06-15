namespace Skeleton.Core
{
    //these are linked to both fluent validation rules and ef constraints so be careful changing
    public static class ValidationConstants
    {
        public const int DescriptionLength = 1000;
        public const int NameLength = 100;
        public const int FirstNameLength = 50;
        public const int LastNameLength = 50;
        public const int NameMinimumLength = 1;
        public const int CountryCodeLength = 2;
        public const int PostalCodeLength = 20;
        public const int EmailAddressLength = 254; //based on RFCs
        public const int IPAddressLength = 45;  //based on RFCs for all legal ip addresses
        public const int PhoneNumberLength = 31; //Should be max 31 = 5 + 15 + 11 for international standards
        public const int UrlLength = 2047; //IE11 is the lowest common denominator right now
    }
}
