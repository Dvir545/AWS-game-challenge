using System.Collections;
using UnityEngine;

namespace AWSUtils
{
    public class NPCSpeech: MonoBehaviour
    {
        [SerializeField] private SpeechBubbleBehaviour speechBubbleBehaviour;
        
        public void Speak(string text) // DVIR - use this method to make the NPC speak
        {
            speechBubbleBehaviour.SetText(text);
        }

        private void Start()  // DVIR - this is just an example for you to experiment with. Delete this when you get the idea.
        {
            Speak("Hello there! Nice to meet you! My name is glorbo and I am the npc!");
        }
    }
}