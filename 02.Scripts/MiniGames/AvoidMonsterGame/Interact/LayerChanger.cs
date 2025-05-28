using UnityEngine;

public class LayerChanger : MonoBehaviour
{
    [SerializeField] private int _layerBeforeEnter;
    [SerializeField] private int _layerAfterEnter;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<AMMonster>() != null)
        {
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = _layerAfterEnter;
            Debug.Log(gameObject.GetComponent<SpriteRenderer>().sortingOrder);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<AMMonster>() != null)
        {
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = _layerBeforeEnter;
            Debug.Log(gameObject.GetComponent<SpriteRenderer>().sortingOrder);
        }
    }

}
