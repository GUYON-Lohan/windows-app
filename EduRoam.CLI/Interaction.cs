using System;

using SharedResources = EduRoam.Localization.Resources;

namespace EduRoam.CLI
{
    public static class Interaction
    {
        public static bool GetConfirmation()
        {
            Console.Write($"{SharedResources.AreYouSure} ({SharedResources.ChoiceKeyYes.ToLower()}/{SharedResources.ChoiceKeyNo.ToUpper()})");

            var choice = Console.ReadLine() ?? SharedResources.ChoiceKeyNo.ToUpper();

            return (choice.Trim().ToString().Equals(SharedResources.ChoiceKeyYes, StringComparison.CurrentCultureIgnoreCase));
        }

        public static string GetYesNoText(bool status)
        {
            return status ? SharedResources.EmojiYes : SharedResources.EmojiNo;
        }
    }
}
