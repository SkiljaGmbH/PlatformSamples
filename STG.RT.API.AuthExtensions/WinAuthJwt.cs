using System;

namespace STG.RT.API.AuthExtensions
{
    /// <summary>
    /// Windows authentication token data
    /// </summary>
    public class WinAuthJwt
    {
        public string access_token { get; set; }

        /// <summary>
        /// In seconds when this token expires
        /// </summary>
        public int expires_in { get; set; }

        public DateTime ExpiresUtc { get; set; }
    }
}
