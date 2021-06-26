using MudBlazor;
using Realmar.Jobbernetes.AdminWeb.Shared.Primitives;

namespace Realmar.Jobbernetes.AdminWeb.Client.Formatters
{
    internal class ColorProvider
    {
        private readonly MudTheme _theme;

        public ColorProvider(MudTheme theme) => _theme = theme;

        public string Get(Percentage? percentage)
        {
            if (percentage == null)
            {
                return _theme.Palette.ActionDefault;
            }

            // ReSharper disable once RedundantCast
            return (int) percentage.Value switch
            {
                (< 20) => _theme.Palette.Error,
                (< 80) => _theme.Palette.Warning,
                _      => _theme.Palette.Success
            };
        }
    }
}
