using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleRenderer : MonoBehaviour
{
    public LineRenderer circleRenderer;
    [SerializeField] private int steps = 100;
    [SerializeField] private float radius = 1f;

    private void Start()
    {
        DrawCircle(steps, radius);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            DrawCircle(steps, radius);
        }
    }

    private void DrawCircle(int steps, float radius)
    {
        circleRenderer.positionCount = 0;
        circleRenderer.positionCount = steps;

        for (int i = 0; i < steps; i++)
        {
            float circumferenceProgress = (float)i / steps;

            float currentRadian = circumferenceProgress * 2 * Mathf.PI;

            float xScaled = Mathf.Cos(currentRadian);
            float yScaled = Mathf.Sin(currentRadian);

            float x = xScaled * radius;
            float y = yScaled * radius;

            Vector3 currentPosition = new Vector3(x, y, 0);

            circleRenderer.SetPosition(i, currentPosition);
        }
    }
}
