using com.MirenlightStudio.NovaSandbox.MasterServer.MySql;
using com.MirenlightStudio.NovaSandbox.Networking;
using com.MirenlightStudio.NovaSandbox.Networking.Packets;
using LiteNetLib;
using MySqlConnector;
using Serilog;
using System.Net.Mail;

namespace com.MirenlightStudio.NovaSandbox.MasterServer.MasterAccounts
{
    public class AccountManager : IDisposable
    {
        [Serializable]
        private class EmailDomain
        {
            public int id;
            public string? domain;
            public DateTime added_at;
        }

        private Database? _db;
        private bool _canUseDatabase;
        private HashSet<string>? _emailDomainsWhitelist;

        public AccountManager(Database database)
        {
            _emailDomainsWhitelist = null;
            _canUseDatabase = false;
            _db = database;
            _db.onDatabaseConnected += OnDatabaseConnected;
            _db.Connect();
            NetworkPacketHandlerRegistry.Register(NetworkPacketEnum.Auth, HandleAuthPacket);
        }

        private void HandleAuthPacket(NetworkPacketBase packet, NetPeer peer)
        {
            if (!_canUseDatabase)
            {
                peer.Disconnect();
                return;
            }

            if (packet is not AuthPacket auth || string.IsNullOrEmpty(auth.Password))
            {
                peer.Disconnect();
                return;
            }

            bool hasUsername = !string.IsNullOrEmpty(auth.Username);
            bool hasEmail = !string.IsNullOrEmpty(auth.Email);

            ValueTuple<AuthResult, AccountBan?>? resultTuple = null;

            if (hasUsername && hasEmail)
            {
                resultTuple = Register(auth.Username!, auth.Email!, auth.Password!);
            }
            else if (hasUsername && !hasEmail)
            {
                resultTuple = Login(auth.Username!, auth.Password);
            }
            else if (!hasUsername && hasEmail)
            {
                resultTuple = Login(auth.Email!, auth.Password);
            }
            else
            {
                AuthResponsePacket ise = new()
                {
                    Status = (short)AuthResultStatusEnum.InternalServerError
                };
                peer.Disconnect(ise.Serialize());
                return;
            }

            AuthResult result = resultTuple.Value.Item1;
            AccountBan? ban = resultTuple.Value.Item2;

            AuthResponsePacket response = new()
            {
                Status = (short)result.ResultStatus
            };

            if (ban != null)
            {
                BannedResponsePacket banned = new()
                {
                    Reason = ban.reason,
                    BannedBy = ban.banned_by,
                    BannedAt = new DateTimeOffset(ban.banned_at.ToUniversalTime()).ToUnixTimeSeconds(),
                    ExpiresAt = ban.expires_at.HasValue ? new DateTimeOffset(ban.expires_at.Value.ToUniversalTime()).ToUnixTimeSeconds() : -1
                };
                peer.Send(response.Serialize(), DeliveryMethod.ReliableOrdered);
                peer.Disconnect(banned.Serialize());
                return;
            }

            bool success = result.ResultStatus == AuthResultStatusEnum.Success && result.Account != null;
            if (success)
            {
                Account a = result.Account!;
                response.Id = a.id;
                response.Username = a.username;
                response.Nickname = a.nickname;
                response.Role = (byte)a.role;
                response.CreatedAt = new DateTimeOffset(a.created_at.ToUniversalTime()).ToUnixTimeSeconds();

                peer.Tag = a;
                peer.Send(response.Serialize(), DeliveryMethod.ReliableOrdered);
            }
            else
            {
                peer.Disconnect(response.Serialize());
            }
        }

        private ValueTuple<AuthResult, AccountBan?> Login(string input, string password)
        {
            if (!_canUseDatabase)
            {
                throw new InvalidOperationException("Unable to process login while database is disconnected.");
            }

            List<ValueTuple<Account, AccountBan?>> accounts = _db!.ExecuteQuery(
                @"
SELECT a.id, a.uname, a.pwd, a.nick, a.role, a.created_at, 
e.email, e.verified,
b.id AS ban_id, b.reason, b.banned_by, b.banned_at, b.expires_at
FROM accounts a 
LEFT JOIN emails e ON a.id = e.account_id 
LEFT JOIN bans b ON a.id = b.account_id AND (b.expires_at IS NULL OR b.expires_at > CURRENT_TIMESTAMP)
WHERE a.uname = @login OR e.email = @login
ORDER BY b.id DESC LIMIT 1",
                r =>
                {
                    var account = new Account()
                    {
                        id = r.GetInt64("id"),
                        username = r.GetString("uname"),
                        password = r.GetString("pwd"),
                        nickname = r.GetString("nick"),
                        email = r.IsDBNull(r.GetOrdinal("email")) ? null : r.GetString("email"),
                        emailVerified = !r.IsDBNull(r.GetOrdinal("verified")) && r.GetBoolean("verified"),
                        role = (AccountRoleEnum)Enum.Parse(typeof(AccountRoleEnum), r.GetString("role")),
                        created_at = r.GetDateTime("created_at")
                    };

                    AccountBan? banned = null;
                    if (!r.IsDBNull(r.GetOrdinal("ban_id")))
                    {
                        banned = new AccountBan()
                        {
                            reason = r.GetString("reason"),
                            banned_by = r.GetInt64("banned_by"),
                            banned_at = r.GetDateTime("banned_at"),
                            expires_at = r.IsDBNull(r.GetOrdinal("expires_at")) ? null : r.GetDateTime("expires_at")
                        };
                    }

                    return new ValueTuple<Account, AccountBan?>(account, banned);
                },
                new MySqlParameter("@login", input)
            );

            if (accounts.Count == 0)
            {
                return new ValueTuple<AuthResult, AccountBan?>(new(AuthResultStatusEnum.AccountNotFound), null);
            }

            Account account = accounts[0].Item1;
            AccountBan? ban = accounts[0].Item2;

            if (ban != null)
            {
                return new ValueTuple<AuthResult, AccountBan>(new(AuthResultStatusEnum.AccountBanned), ban!);
            }

            if (string.IsNullOrEmpty(account!.email))
            {
                return new ValueTuple<AuthResult, AccountBan>(new(AuthResultStatusEnum.EmailLinkRequired), ban!);
            }
            if (!ValidateEmail(account!.email))
            {
                return new ValueTuple<AuthResult, AccountBan>(new(AuthResultStatusEnum.EmailNotAllowed), ban!);
            }
            if (account!.emailVerified == false)
            {
                return new ValueTuple<AuthResult, AccountBan>(new(AuthResultStatusEnum.EmailNotVerified), ban!);
            }
            if (account!.password != password)
            {
                return new ValueTuple<AuthResult, AccountBan>(new(AuthResultStatusEnum.InvalidPassword), ban!);
            }
            return new ValueTuple<AuthResult, AccountBan>(new(AuthResultStatusEnum.Success, account), ban!);
        }

        private ValueTuple<AuthResult, AccountBan?> Register(string username, string email, string password)
        {
            if (!_canUseDatabase)
            {
                throw new InvalidOperationException("Unable to process registration while database is disconnected.");
            }

            if (!ValidateEmail(email))
            {
                return new ValueTuple<AuthResult, AccountBan?>(new(AuthResultStatusEnum.EmailNotAllowed), null);
            }

            var existingAccounts = _db!.ExecuteQuery(
                @"
SELECT a.id FROM accounts a WHERE a.uname = @uname 
UNION 
SELECT e.account_id FROM emails e WHERE e.email = @email",
                r => r.GetInt64(0),
                new MySqlParameter("@uname", username),
                new MySqlParameter("@email", email)
            );

            if (existingAccounts.Count > 0)
            {
                return new ValueTuple<AuthResult, AccountBan?>(new(AuthResultStatusEnum.AccountAlreadyExist), null);
            }

            try
            {
                _db.ExecuteNonQuery(
                    @"
INSERT INTO accounts (uname, pwd, nick, role, created_at) 
VALUES (@uname, @pwd, @nick, 'user', CURRENT_TIMESTAMP);

INSERT INTO emails (account_id, email, verified) 
VALUES (LAST_INSERT_ID(), @email, 0);",
                    new MySqlParameter("@uname", username),
                    new MySqlParameter("@pwd", password),
                    new MySqlParameter("@nick", username),
                    new MySqlParameter("@email", email)
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database error during registration");
                return new ValueTuple<AuthResult, AccountBan?>(new(AuthResultStatusEnum.InternalServerError), null);
            }

            return Login(username, password);
        }

        private ValueTuple<string, string> ParseEmail(string mail)
        {
            try
            {
                MailAddress mailAddress = new(mail);
                return new ValueTuple<string, string>(mailAddress.User, mailAddress.Host);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while parsing email: ");
                return new ValueTuple<string, string>();
            }
        }

        private bool ValidateEmail(string mail)
        {
            ValueTuple<string, string> mailTuple = ParseEmail(mail);
            string domain = mailTuple.Item2;
            return _emailDomainsWhitelist!.Contains(domain);
        }

        private void OnDatabaseConnected()
        {
            _canUseDatabase = true;
            List<EmailDomain> emailDomains = _db!.ExecuteQuery(
                @"SELECT * FROM valid_domains WHERE 1",
                r => new EmailDomain()
                {
                    id = r.GetInt32("id"),
                    domain = r.GetString("domain"),
                    added_at = r.GetDateTime("added_at")
                }
                );
            _emailDomainsWhitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (EmailDomain domain in emailDomains)
            {
                _emailDomainsWhitelist.Add(domain.domain!);
            }
        }

        public void Dispose()
        {
            if (_db != null) _db.onDatabaseConnected -= OnDatabaseConnected;
            NetworkPacketHandlerRegistry.Unregister(NetworkPacketEnum.Auth);
            _db = null;
        }
    }
}
