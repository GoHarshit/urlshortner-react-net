namespace UrlShortner.Helpers
{
    public static class Base62Encoder
    {
        private const string Characters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string Encode(
            long number)
        {
            if (number == 0)
            {
                return Characters[0]
                    .ToString();
            }

            var result = "";

            while (number > 0)
            {
                result =
                    Characters[
                        (int)(number % 62)
                    ] + result;

                number /= 62;
            }

            return result;
        }
    }
}