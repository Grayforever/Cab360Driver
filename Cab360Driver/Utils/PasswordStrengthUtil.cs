using Android.Graphics;
using System;

namespace Cab360Driver.Utils
{
    public enum PasswordStrength
    {
        Weak,
        Medium,
        Strong,
        VeryStrong
    }

    public class PasswordStrengthUtil
    {
        private static int MIN_LENGTH = 8;
        private static int MAX_LENGTH = 15;
        private string _password;
        public PasswordStrengthUtil(string password)
        {
            _password = password;
        }

        public PasswordStrength Calculate()
        {
            int score = 0;
            // bool indicating if password has an upper case
            bool upper = false;
            // bool indicating if password has a lower case
            bool lower = false;
            // bool indicating if password has at least one digit
            bool digit = false;
            // bool indicating if password has a leat one special char
            bool specialChar = false;

            for (int i = 0; i < _password.Length; i++)
            {
                char c = _password[i];

                if (!specialChar && !char.IsLetterOrDigit(c))
                {
                    score++;
                    specialChar = true;
                }
                else
                {
                    if (!digit && char.IsDigit(c))
                    {
                        score++;
                        digit = true;
                    }
                    else
                    {
                        if (!upper || !lower)
                        {
                            if (char.IsUpper(c))
                            {
                                upper = true;
                            }
                            else
                            {
                                lower = true;
                            }

                            if (upper && lower)
                            {
                                score++;
                            }
                        }
                    }
                }
            }

            int length = _password.Length;

            if (length > MAX_LENGTH)
            {
                score++;
            }
            else if (length < MIN_LENGTH)
            {
                score = 0;
            }

            // return enum following the score
            switch (score)
            {
                case 0: return PasswordStrength.Weak;
                case 1: return PasswordStrength.Medium;
                case 2: return PasswordStrength.Strong;
                case 3: return PasswordStrength.VeryStrong;
                default:
                    break;
            }

            return PasswordStrength.VeryStrong;
        }


        public Tuple<int, int> GetPassResult()
        {
            switch (Calculate())
            {
                case PasswordStrength.Weak:
                    return new Tuple<int, int>(Resource.String.password_strength_weak, Color.ParseColor("#61ad85"));
                    
                case PasswordStrength.Medium:
                    return new Tuple<int, int>(Resource.String.password_strength_medium, Color.ParseColor("#4d8a6a"));

                case PasswordStrength.Strong:
                    return new Tuple<int, int>(Resource.String.password_strength_strong, Color.ParseColor("#3a674f"));

                case PasswordStrength.VeryStrong:
                    return new Tuple<int, int>(Resource.String.password_strength_very_strong, Color.ParseColor("#61ad85"));
            }
            return new Tuple<int, int>(Resource.String.password_strength_very_strong, Color.ParseColor("#61ad85"));
        }
    }

}