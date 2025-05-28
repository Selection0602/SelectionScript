using UnityEngine.UI;

public interface IPanelButton 
{
    public Button DefaultButton { get; set; }   //기본 버튼 프러퍼티
    public void ChangeButton();
}

