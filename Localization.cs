using Sandbox;

namespace AutoEatPulsar
{
    internal static class AutoEatLocalization
    {
        public static bool IsPortuguese
        {
            get
            {
                if (MySandboxGame.Config == null)
                    return false;

                return MySandboxGame.Config.Language.ToString().StartsWith("Portuguese");
            }
        }

        public static string Text(string portuguese, string english)
        {
            return IsPortuguese ? portuguese : english;
        }
    }
}
