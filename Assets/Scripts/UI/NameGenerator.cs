using UnityEngine;
using Math = System.Math;

namespace Utils
{
    public static class NameGenerator
    {
        private static readonly string[] Actions = {
            "Happy", "Swift", "Brave", "Fierce", "Mighty", "Silent", "Wild", "Clever",
            "Quick", "Fast", "Bold", "Smart", "Strong", "Wise", "Calm", "Sneaky",
            "Flying", "Dance", "Steady", "Noble", "Lucky", "Angry", "Nice", "Proud",
            "Sleepy", "Hungry", "Lazy", "Crazy", "Silly", "Soft", "Grump", "Loud",
            "Quiet", "Tiny", "Giant", "Cute", "Scary", "Shy", "Bold", "Fun",
            "Small", "Big", "Tall", "Tiny", "Huge", "Lil", "Long", "Short",
            "Fat", "Thin", "Fluff", "Fuzz", "Fury", "Shag", "Smooth", "Spiky",
            "Red", "Blue", "Green", "Black", "White", "Gold", "Silver", "Purple",
            "Pink", "Yellow", "Brown", "Gray", "Orange", "Dark", "Bright", "Shiny",
            "Super", "Mega", "Ultra", "Power", "Epic", "Cool", "Pro", "Max",
            "Great", "Grand", "Royal", "Ace", "Elite", "Top", "Star", "Prime",
            "Fire", "Ice", "Storm", "Shade", "Frost", "Flame", "Bolt", "Flash", "Sus"
        };

        private static readonly string[] Animals = {
            "Dog", "Cat", "Ham", "Bun", "Fish", "Bird", "Dove", "Mouse",
            "Pony", "Cow", "Pig", "Hen", "Goat", "Ram", "Duck", "Rex",
            "Wolf", "Bear", "Fox", "Deer", "Rat", "Coon", "Ape", "Ele",
            "Zeeb", "Lion", "Tiger", "Panda", "Bear", "Roo", "Peng", "Seal",
            "Bird", "Hawk", "Owl", "Crow", "Jay", "Kite", "Swan", "Dove",
            "Fish", "Pod", "Fin", "Oct", "Shell", "Crab", "Ray", "Star",
            "Drag", "Corn", "Bird", "Gryf", "Maid", "Peg", "Fae", "Titan",
            "Trol", "Gob", "Ghost", "Vamp", "Wolf", "Mage", "Imp", "Angel",
            "Blob", "Orc", "Rock", "Nin", "Pirate", "Bot", "ET", "Rex"
        };

        public static string GenerateGuestName()
        {
            string result;
            do
            {
                string randomAction = Actions[Random.Range(0, Actions.Length)];
                string randomAnimal = Animals[Random.Range(0, Animals.Length)];
                int randomNumber = Random.Range(1, 100);

                // Try different formats until we get one under 14 chars
                result = $"{randomAction}-{randomAnimal}{randomNumber}";
                if (result.Length > 13)
                {
                    // Try shorter format
                    result = $"{randomAction.Substring(0, Math.Min(4, randomAction.Length))}{randomAnimal}{randomNumber}";
                }
                if (result.Length > 13)
                {
                    // Try even shorter format
                    result = $"{randomAction.Substring(0, Math.Min(3, randomAction.Length))}{randomAnimal.Substring(0, Math.Min(3, randomAnimal.Length))}{randomNumber}";
                }
            } while (result.Length > 13);

            return result;
        }
    }
}