    using UnityEngine;
    using TMPro;

    public class bootyPanel : MonoBehaviour
    {
        private TextMeshProUGUI nametext;
        private TextMeshProUGUI desctext;

        private void Awake()
        {
            FindTextComponents();
        }
        
        private void FindTextComponents()
        {
            if (nametext == null || desctext == null)
            {
                // 모든 자식에서 텍스트 컴포넌트 검색 (비활성 포함)
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
                
                foreach (var text in texts)
                {
                    if (text == null) continue;
                    
                    string objName = text.gameObject.name;
                    if (objName == "Name") 
                        nametext = text;
                    else if (objName == "Desc")
                        desctext = text;
                }
            }
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public void SetInfo(string itemName, string itemDesc)
        {
            // 혹시 컴포넌트가 없을 경우 다시 찾기 시도
            if (nametext == null || desctext == null)
            {
                FindTextComponents();
            }
            
            // null 체크 후 적용
            if (nametext != null) 
                nametext.text = itemName;
                
            if (desctext != null) 
                desctext.text = itemDesc;
        }
    }