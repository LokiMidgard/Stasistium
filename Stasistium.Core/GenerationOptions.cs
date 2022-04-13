using Stasistium.Stages;

namespace Stasistium
{
    public class GenerationOptions
    {
        public bool Refresh { set; get; } = true;

        public bool BreakOnError { set; get; }

        public bool CompressCache { get; set; } = true;
        public bool CheckUniqueID { get; set; }
        public OptionToken Token
        {
            get
            {
                OptionToken? token = new(this);
                return token;

            }
        }

    }

}