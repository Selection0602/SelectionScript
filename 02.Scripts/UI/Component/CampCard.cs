using System.Collections;
using UnityEngine;

public class CampCard : MonoBehaviour   
{
    public GameObject front;
    public GameObject back; 
    [SerializeField] private Animator animator;
    private bool isFlipped = false;
    public string factionName; 
    
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
    
    public IEnumerator ReverseFlip()
    {
        float time = 0.5f;
        float elapsed = 0f;
        
        front.SetActive(true);
        back.SetActive(false);
        
        Quaternion startRot = Quaternion.Euler(0, 0, 0);
        Quaternion endRot = Quaternion.Euler(0, 180, 0);
        bool half = false;
        
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);

            front.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            if (!half && t >= 0.5f)
            {
                front.SetActive(false);
                back.SetActive(true);
                half = true;
            }
            yield return null;
        }
        front.transform.rotation = endRot;
        isFlipped = true;
    }

    public IEnumerator Flip()
    {
        float time = 0.5f;
        float elapsed = 0f;
        back.SetActive(true);
        front.SetActive(false);
        
        Quaternion startRot = Quaternion.Euler(0, 180, 0);
        Quaternion endRot = Quaternion.Euler(0, 0, 0);
        bool half = false;
        
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);

            back.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            if (!half && t >= 0.5f)
            {
                back.SetActive(false);
                front.SetActive(true);
                half = true;
            }
            yield return null;
        }
        back.transform.rotation = endRot;
        isFlipped = false;
    }

    public void ResetCard()
    {
        front.SetActive(false);
        back.SetActive(true);
        front.transform.rotation = Quaternion.identity;
        back.transform.rotation = Quaternion.identity;
        isFlipped = false;
    }
    
    private void OnEnable()
    {
        ResetCard();
    }
}