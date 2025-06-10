public static class GlobalCounter
{
    public static int char1 = 0;
    public static int char2 = 0;
    public static int char3 = 0;
    public static int char4 = 0;
    public static int passengernum = 0;
    public static void Increment(string characterName)
    {
        switch (characterName)
        {
            case "char1":
                char1++;
                break;
            case "char2":
                char2++;
                break;
            case "char3":
                char3++;
                break;
            case "char4":
                char4++;
                break;
        }
    }
}
