// planet names taken from http://www.minorplanetcenter.net/

public class PlanetNames
{
    // private static HashSet<string> usedPlanetNames = new HashSet<string>();

    public static string GetRandom()
    {
        return GenerateName(3 + MyRandom.Instance.Next(4));
    }

    public static string GenerateName(int len)
    {
        string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "z" };
        string[] double_consonants = { "sh", "zh", "cc", "nd", "ch", "ll", "lv", "rr", "tt", "nn", "nl", "ff", "ss", "rg", "gr", "gl" };
        string[] vowels = { "a", "e", "i", "o", "u", "y" };
        string[] double_vowels = { "ae", "aa", "ao", "ea", "ey", "io", "ia" };
        string Name = "";
        Name += consonants[MyRandom.Instance.Next(consonants.Length)];
        int b = 1; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
        while (b < len)
        {
            if (MyRandom.Instance.Next(5) == 0)
                Name += double_vowels[MyRandom.Instance.Next(double_vowels.Length)];
            else
                Name += vowels[MyRandom.Instance.Next(vowels.Length)];
            b++;
            if (b >= len)
                break;
            if (MyRandom.Instance.Next(5) == 0)
                Name += double_consonants[MyRandom.Instance.Next(double_consonants.Length)];
            else
                Name += consonants[MyRandom.Instance.Next(consonants.Length)];
            b++;
        }
        /* Add Number
        if (MyRandom.Instance.Next(20) == 0)
        {
            Name += " " + (MyRandom.Instance.Next(8) + 1).ToString();
            if (MyRandom.Instance.Next(10) == 0)
                Name += (MyRandom.Instance.Next(10)).ToString();
        }
        */
        return Name[0].ToString().ToUpper() + Name.Substring(1);
    }
}