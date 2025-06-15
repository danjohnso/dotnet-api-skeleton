namespace Skeleton.Core
{
    /// <summary>
    /// Nice list of starter problems
    /// </summary>
    public class Problems
    {
        public static readonly Problem UnexpectedError = new("ERR1000", "An unexpected error occurred, please try again later");
        public static readonly Problem InvalidRequestFormat = new("ERR1001", "The request format is invalid");
        public static readonly Problem Unauthorized = new("ERR1002", "The client has not been authenticated");
        public static readonly Problem Forbidden = new("ERR1003", "The client does not have access rights to the content");
        public static readonly Problem NotFound = new("ERR1004", "The resource was not found");
        public static readonly Problem UnprocessableEntity = new("ERR1006", "Unable to process the request body");
        public static readonly Problem Conflict = new("ERR1007", "There was a conflict while updating the resource");
        public static readonly Problem Deleted = new("ERR1008", "Resource is no longer available");

        public static readonly Problem FieldRequired = new("ERR2000", "'{0}' is required");
        public static readonly Problem FieldMaxLength = new("ERR2001", "'{0}' cannot be longer than '{1}' characters");
        public static readonly Problem FieldEmail = new("ERR2002", "'{0}' does not look like a valid email address");
        public static readonly Problem FieldInEnum = new("ERR2003", "'{0}' is not in the list of a valid values");
        public static readonly Problem FieldInclusiveRange = new("ERR2004", "'{0}' must be greater than or equal to '{1}' and less than or equal to '{2}'");
        public static readonly Problem FieldExclusiveRange = new("ERR2005", "'{0}' must be greater than '{1}' and less than '{2}'");
        public static readonly Problem FieldExactLength = new("ERR2006", "'{0}' must be '{1}' characters long");
        public static readonly Problem FileEmpty = new("ERR2007", "File was empty");
        public static readonly Problem FileTooLarge = new("ERR2008", "File must be less than or equal to {0}mb");
        public static readonly Problem FileTypeInvalid = new("ERR2009", "File type is not allowed");
    }
}
