using Skeleton.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Skeleton.EntityFrameworkCore
{
    /// <summary>
    /// Helpers for making fields consistantly stored in the database.
    /// </summary>
    public static class PropertyExtensions
	{
        public static PropertyBuilder HasPrecision(this PropertyBuilder propertyBuilder, byte precision, byte scale)
        {
	        return propertyBuilder.HasColumnType($"decimal({precision},{scale})");
        }

		public static PropertyBuilder DefaultPrecision(this PropertyBuilder propertyBuilder)
		{
			return propertyBuilder.HasColumnType("decimal(18,4)");
		}

        public static PropertyBuilder IsMoney(this PropertyBuilder propertyBuilder)
        {
            return propertyBuilder.HasColumnType("money");
        }

        public static PropertyBuilder IsPercentage(this PropertyBuilder propertyBuilder)
        {
	        return propertyBuilder.HasColumnType("decimal(6,4)");
        }

		public static PropertyBuilder IsDescription(this PropertyBuilder propertyBuilder)
		{
			return propertyBuilder.HasMaxLength(ValidationConstants.DescriptionLength);
		}

        public static PropertyBuilder IsName(this PropertyBuilder propertyBuilder)
		{
			return propertyBuilder.HasMaxLength(ValidationConstants.NameLength);
		}

		public static PropertyBuilder IsFirstName(this PropertyBuilder propertyBuilder)
		{
			return propertyBuilder.HasMaxLength(ValidationConstants.FirstNameLength);
		}

		public static PropertyBuilder IsLastName(this PropertyBuilder propertyBuilder)
		{
			return propertyBuilder.HasMaxLength(ValidationConstants.LastNameLength);
		}

		public static PropertyBuilder IsEmailAddress(this PropertyBuilder propertyBuilder)
		{
			return propertyBuilder.HasMaxLength(ValidationConstants.EmailAddressLength);
		}

		public static PropertyBuilder IsIPAddress(this PropertyBuilder propertyBuilder)
		{
			return propertyBuilder.HasMaxLength(ValidationConstants.IPAddressLength);
		}

		public static PropertyBuilder IsPhoneNumber(this PropertyBuilder propertyBuilder)
		{
			return propertyBuilder.HasMaxLength(ValidationConstants.PhoneNumberLength);
		}

		public static PropertyBuilder IsUrl(this PropertyBuilder propertyBuilder)
		{
			return propertyBuilder.HasMaxLength(ValidationConstants.UrlLength);
		}
    }
}
