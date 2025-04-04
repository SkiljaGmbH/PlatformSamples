namespace STG.RT.API.AuthExtensions
{
    /// <summary>
    /// Error content response for token fetch
    /// </summary>
    public class ErrorContent
    {
        public string error { get; set; }
        public string error_description { get; set; }
    }
}
