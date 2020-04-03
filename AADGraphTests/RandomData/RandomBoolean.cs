namespace AppRolesTesting
{
    /// <summary>
    /// Generates random Boolean values
    /// </summary>
    public class RandomBoolean : RandomDataBase<bool>
    {
        /// <summary>
        /// Returns True or False randomly
        /// </summary>
        /// <returns>True or False</returns>
        public override bool GetRandom()
        {
            return _random.NextDouble() >= 0.5;
        }
    }
}