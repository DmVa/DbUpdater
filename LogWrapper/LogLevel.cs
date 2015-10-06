namespace LogWrapper
{
    public enum LogLevel
    {
        /// <summary>
        /// Not visibile messages in production.
        /// </summary>
        Debug, 
        /// <summary>
        /// Visible messages in production, but not errors.
        /// </summary>
        Info,
        /// <summary>
        /// Errors, visible in production.
        /// </summary>
        Error
    }
}
