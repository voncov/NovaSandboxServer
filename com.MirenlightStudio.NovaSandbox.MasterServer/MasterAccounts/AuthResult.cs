namespace com.MirenlightStudio.NovaSandbox.MasterServer.MasterAccounts
{
    public class AuthResult
    {
        public AuthResultStatusEnum ResultStatus { get; }
        public Account? Account { get; }

        public AuthResult(AuthResultStatusEnum resultStatus)
        {
            ResultStatus = resultStatus;
        }

        public AuthResult(AuthResultStatusEnum resultStatus, Account account)
        {
            ResultStatus = resultStatus;
            Account = account;
        }
    }
}
