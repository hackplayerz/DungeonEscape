using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    [Header("Moving Area")]
    [Header("You Must Move To Z")]
    public Transform StartPosition = null;
    public Transform EndPosition = null;
    [Header("Moving Control")] 
    public float StopTime = 0.8f;
    public float Speed = 1f;

    private bool _isMovingToEnd = true;

    private void OnEnable()
    {
        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        while (enabled)
        {
            if (transform.localPosition.z > EndPosition.localPosition.z)
            {
                _isMovingToEnd = false;
                yield return new WaitForSeconds(StopTime);

            }
            else if (transform.localPosition.z < StartPosition.localPosition.z)
            {
                _isMovingToEnd = true;
                yield return new WaitForSeconds(StopTime);

            }
            if (_isMovingToEnd)
            {
                transform.Translate(transform.forward * Speed * Time.fixedDeltaTime);
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                transform.Translate(-transform.forward * Speed * Time.fixedDeltaTime);
                yield return new WaitForSeconds(0.01f);
            }
        }
    }
}
