using UnityEngine;

namespace Utils
{
    public class CharacterSorter : MonoBehaviour
    {
        [SerializeField] private GameObject yOffsetMarker;
        [SerializeField] private SpriteRenderer spriteRenderer;
        void Update()
        {
            spriteRenderer.sortingOrder = (int)(yOffsetMarker.transform.position.y * -100);
        }
    }
}
