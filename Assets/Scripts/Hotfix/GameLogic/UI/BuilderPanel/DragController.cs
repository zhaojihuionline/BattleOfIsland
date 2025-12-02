using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using QFramework;

public class DragOutEvent
{
    public readonly GameObject DraggedObject;
    public readonly float DragDistance;

    public DragOutEvent(GameObject draggedObject, float dragDistance)
    {
        DraggedObject = draggedObject;
        DragDistance = dragDistance;


    }
}

public class DragController : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IController
{
    [SerializeField] private RectTransform boundaryRectTransform;
    [SerializeField] private float boundaryThreshold = 0f;

    [SerializeField] private float returnDuration = 0.5f;
    [SerializeField] private Ease returnEase = Ease.OutBack;

    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float scaleDuration = 0.2f;

    private Vector2 initialPosition;
    private RectTransform rectTransform;
    private bool isDragging = false;
    private Vector2 dragStartPosition;
    private Vector3 originalScale;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        originalScale = transform.localScale;

        if (GetComponent<CanvasGroup>() == null)
            gameObject.AddComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out dragStartPosition);

        transform.DOScale(originalScale * selectedScale, scaleDuration).SetEase(Ease.OutBack);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition))
        {
            float yOffset = localPointerPosition.y - dragStartPosition.y;

            if (yOffset > 0)
            {
                rectTransform.anchoredPosition = new Vector2(
                    initialPosition.x,
                    initialPosition.y + yOffset);

                CheckBoundary();
            }
            else
            {
                rectTransform.anchoredPosition = initialPosition;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        transform.DOKill();

        if (rectTransform.anchoredPosition.y > initialPosition.y)
        {
            ReturnToInitialPosition();
        }
        else
        {
            transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutBack);
        }
    }

    private void CheckBoundary()
    {
        if (boundaryRectTransform == null || !isDragging) return;

        Vector2 currentLocalPosInBoundary = boundaryRectTransform.InverseTransformPoint(rectTransform.position);
        float currentHeight = rectTransform.rect.height * rectTransform.lossyScale.y;
        float currentTopYInBoundary = currentLocalPosInBoundary.y + (currentHeight / 2f);
        float boundaryTopY = boundaryRectTransform.rect.height / 2f;

        if (currentTopYInBoundary > boundaryTopY + boundaryThreshold)
        {
            isDragging = false;

            float dragDistance = rectTransform.anchoredPosition.y - initialPosition.y;
            TypeEventSystem.Global.Send(new DragOutEvent(gameObject, dragDistance));
            ReturnToInitialPosition();
        }
    }

    private void ReturnToInitialPosition()
    {
        rectTransform.DOKill();
        transform.DOKill();
        rectTransform.DOAnchorPos(initialPosition, returnDuration)
            .SetEase(returnEase)
            .OnStart(() =>
            {
                var canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
            })
            .OnComplete(() =>
            {
                var canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
            });

        transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutBack);
    }

    private void OnDisable()
    {
        rectTransform.DOKill();
        transform.DOKill();
    }

    private void OnDestroy()
    {
        rectTransform.DOKill();
        transform.DOKill();
    }

    public void ResetPosition()
    {
        ReturnToInitialPosition();
    }

    public IArchitecture GetArchitecture() => GridTrackerApp.Interface;
}