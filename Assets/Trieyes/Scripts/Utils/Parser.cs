namespace Utils
{
    public class Parser
    {
        public static int ParseStrToInt(string str)
        {
            int.TryParse(str, out int result);
            return result;
        }
    }
}