namespace Skeleton.Core.Enums
{
    public enum ResultStatusType
    {
        Ok = 200,
        Invalid = 400,
        //Unauthorized = 401, Shouldnt get this far
        //Forbidden = 403, I think these are better off as 404s if people are screwing around
        NotFound = 404,
        Conflict = 409,
        Error = 500,
    }
}
