namespace com.MirenlightStudio.NovaSandbox.MasterServer.MasterAccounts
{
    [Serializable]
    public class Account
    {
        public long id;
        public string? username;
        public string? password;
        public string? nickname;
        public string? email;
        public bool? emailVerified;
        public AccountRoleEnum role;
        public DateTime created_at;
    }
}
