#if UNITY_2021_3_OR_NEWER
using System;
namespace com.MirenlightStudio.NovaSandbox.Credentials
#nullable enable
#else
namespace com.MirenlightStudio.NovaSandbox.MasterServer.MasterAccounts
#endif
{
    [Serializable]
    public class AccountBan
    {
        public string? reason;
        public long banned_by;
        public DateTime banned_at;
        public DateTime? expires_at;
    }
}