using Stasistium.Stages;

namespace Stasistium
{
    public class GenerationOptions
    {
        public bool Refresh { set; get; } = true;

        public bool CompressCache { get; set; } = true;
        public OptionToken Token
        {
            get
            {
                var token = new OptionToken(this.Refresh);
                return token;

            }
        }
    }

}