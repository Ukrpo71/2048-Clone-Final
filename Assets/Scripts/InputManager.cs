using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;

    private bool stopTouch = false;

    [SerializeField] private float swipeRange;

    void Update()
    {
        if (_gameManager.IsInputEnabled)
        {
            //Управление для компа
            if (Input.GetKey(KeyCode.A)) _gameManager.Shift(Vector2.left);
            if (Input.GetKey(KeyCode.W)) _gameManager.Shift(Vector2.up);
            if (Input.GetKey(KeyCode.S)) _gameManager.Shift(Vector2.down);
            if (Input.GetKey(KeyCode.D)) _gameManager.Shift(Vector2.right);


            //Управление для телефона
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    startTouchPosition = touch.position;
                    endTouchPosition = touch.position;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    endTouchPosition = touch.position;
                    DetectSwipe();

                    stopTouch = false;
                }
            }
        }
    }


    void DetectSwipe()
    {
        if (!stopTouch)
        {
            stopTouch = true;

            if (SwipeDistanceMet)
            {
                if (IsVerticalSwipe)
                {
                    var direction = startTouchPosition.y - endTouchPosition.y > 0 ? Vector2.down : Vector2.up;
                    _gameManager.Shift(direction);
                }
                else
                {
                    var direction = startTouchPosition.x - endTouchPosition.x > 0 ? Vector2.left : Vector2.right;
                    _gameManager.Shift(direction);
                }
            }
        }
    }


    public bool SwipeDistanceMet => VerticalMovementDistance >= swipeRange || HorizontalMovementDistance >= swipeRange;
    public float VerticalMovementDistance => Mathf.Abs(startTouchPosition.y - endTouchPosition.y);
    public float HorizontalMovementDistance => Mathf.Abs(startTouchPosition.x - endTouchPosition.x);

    public bool IsVerticalSwipe => VerticalMovementDistance > HorizontalMovementDistance;
    public bool isHorizontalSwipe => HorizontalMovementDistance > VerticalMovementDistance;
    
}
