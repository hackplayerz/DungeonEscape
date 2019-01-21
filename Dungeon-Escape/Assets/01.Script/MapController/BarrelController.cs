using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelController : MonoBehaviour
{
    [Header("Moving Transform")]
    public Transform StartPosition;
    public Transform EndPosition;
    [Header("Moving Control")] 
    public float StopTime = 0.7f;
    public float Speed = .7f;

    private bool _isMovingToEnd = true;

    private void OnEnable()
    {
        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        while (enabled)
        {
            if (transform.localPosition.x > EndPosition.localPosition.x)
            {
                _isMovingToEnd = false;
                yield return new WaitForSeconds(StopTime);

            }
            else if (transform.localPosition.x < StartPosition.localPosition.x)
            {
                _isMovingToEnd = true;
                yield return new WaitForSeconds(StopTime);

            }
            if (_isMovingToEnd)
            {
                transform.Translate(transform.right * Speed * Time.fixedDeltaTime);
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                transform.Translate(-transform.right * Speed * Time.fixedDeltaTime);
                yield return new WaitForSeconds(0.01f);
            }
        }
    }
}
