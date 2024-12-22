using UnityEngine;

namespace Utils
{
    public static class NameGenerator
    {
        private static readonly string[] Actions = {
            "Happy", "Swift", "Brave", "Fierce", "Mighty", "Silent", "Wild", "Clever",
            "Quick", "Agile", "Bold", "Rapid", "Sleek", "Wise", "Calm", "Mystic",
            "Flying", "Dancing", "Steady", "Noble", "Lucky", "Shadow", "Thunder", "Crystal"
        };

        private static readonly string[] Animals = {
            "Wolf", "Eagle", "Lion", "Tiger", "Dragon", "Phoenix", "Falcon", "Bear",
            "Hawk", "Panther", "Dolphin", "Fox", "Owl", "Penguin", "Lynx", "Raven",
            "Shark", "Jaguar", "Cobra", "Leopard", "Viper", "Cheetah", "Crane", "Whale",
            "Slime", "Ork", "Skeleton", "Goblin", "Chicken", "Demon"
        };

        public static string GenerateGuestName()
        {
            string randomAction = Actions[Random.Range(0, Actions.Length)];
            string randomAnimal = Animals[Random.Range(0, Animals.Length)];
            int randomNumber = Random.Range(1, 101);

            return $"{randomAction}{randomAnimal}{randomNumber}";
        }
    }
}