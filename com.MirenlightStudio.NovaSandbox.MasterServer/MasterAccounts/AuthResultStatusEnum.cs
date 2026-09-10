namespace com.MirenlightStudio.NovaSandbox.MasterServer.MasterAccounts
{
    public enum AuthResultStatusEnum : short
    {
        Success = 1000,
        AccountNotFound,
        AccountBanned,
        InvalidPassword,
        EmailNotAllowed,
        EmailAlreadyExist,
        EmailNotVerified,
        EmailLinkRequired,
        AccountAlreadyExist,
        AccountAlreadyInUse,
        AccountUnderReview,
        InternalServerError = 1999,
    }
}
