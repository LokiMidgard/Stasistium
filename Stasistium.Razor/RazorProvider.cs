using System;

namespace Stasistium.Razor
{

    public class RazorProvider
    {
        internal RazorProvider(RazorViewToStringRenderer renderer)
        {
            this.Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        internal RazorViewToStringRenderer Renderer { get; }
    }
}
