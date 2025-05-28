using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ImagePanel : MonoBehaviour
{
    [SerializeField] private Image _currentImage;
    [SerializeField] private Image _nextImage;
    [SerializeField] private float _fadeTransitionTime = 1.0f;
    
    private Sequence _fadeOutSequence;
    
    public event Action OnAnimationStarted;
    public event Action OnAnimationFinished;
    
    private void Awake()
    {
        _currentImage.enabled = false;
        _nextImage.enabled = false;
    }

    public void OnImagePanel(Sprite sprite)
    {
        if (_fadeOutSequence != null && _fadeOutSequence.IsActive())
            _fadeOutSequence.Kill();
        
        if (!sprite || _currentImage.enabled)
        {
            CloseImage();
            return;
        }
        
        ShowImage(sprite);
    }
    
    private void ShowImage(Sprite sprite)
    {
        _currentImage.enabled = true;
        _currentImage.sprite = sprite;
        _currentImage.SetNativeSize();
    }
    
    private void CloseImage()
    {
        _currentImage.enabled = false;
        _currentImage.sprite = null;
    }
    
    public void OnImageTransition(Sprite fromSprite, Sprite toSprite)
    {
        if (_fadeOutSequence != null && _fadeOutSequence.IsActive())
            _fadeOutSequence.Kill();
        
        TransitionImages(fromSprite, toSprite);

    }
    
    private void TransitionImages(Sprite fromSprite, Sprite toSprite)
    {
        _currentImage = SetTransitionImage(_currentImage, fromSprite);
        _nextImage = SetTransitionImage(_nextImage, toSprite);
        
        _fadeOutSequence = DOTween.Sequence();
    
        OnAnimationStarted?.Invoke();
        
        _fadeOutSequence.Join
        (
            _currentImage.DOFade(0f, _fadeTransitionTime)
                .SetUpdate(UpdateType.Normal, true)
                .OnComplete(() => 
                {
                    _currentImage.sprite = _nextImage.sprite;
                    _currentImage.color = Color.white;
                    _nextImage.enabled = false;
                    _nextImage.sprite = null;
                    
                    OnAnimationFinished?.Invoke();
                })
        );
    
        _fadeOutSequence.SetUpdate(UpdateType.Normal, true);
    }
    
    private Image SetTransitionImage(Image image ,Sprite sprite)
    {
        image.sprite = sprite;
        image.enabled = true;
        image.color = Color.white;
        image.SetNativeSize();
        
        return image;
    }
}
