using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class TableConfigurationProvider: ITableConfigurationProvider
    {
        private readonly int[] _default;
        private readonly Dictionary<int, TableConfiguration> _config;

        public TableConfigurationProvider(Dictionary<int, TableConfiguration> config,
            IConfigurationProvider configurationProvider)
        {
            var depth = configurationProvider
                .GeneralConfiguration.GameDepth;
            _default = new int[depth];
            for (var i = 0; i < _default.Length; i++)
            {
                _default[i] = 2;
            }
            _config = config;
        }

        #region Implementation of ITableConfigurationProvider

        public int[] GetValues(int depth)
        {
            return _config.TryGetValue(depth, out var table) ? table.Values : _default;
        }

        #endregion
    }
}