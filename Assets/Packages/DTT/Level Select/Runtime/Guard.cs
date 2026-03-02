namespace DTT.LevelSelect
{
    /// <summary>
    /// A utility class that can be used to validate that all fields given are not null.
    /// </summary>
    internal class Guard
    {
        /// <summary>
        /// The objects that should be guarded against.
        /// </summary>
        private object[] _guardables;
        
        /// <summary>
        /// Creates a new guard instance.
        /// </summary>
        /// <param name="guardables">The objects that should be guarded against.</param>
        public Guard(params object[] guardables) => _guardables = guardables;

        /// <summary>
        /// Validates whether all guardables are not null.
        /// </summary>
        /// <returns>Whether all guardables are not null.</returns>
        public bool Validate()
        {
            for (int i = 0; i < _guardables.Length; i++)
            {
                if (_guardables[i] == null)
                    return false;
            }

            return true;
        }
    }
}