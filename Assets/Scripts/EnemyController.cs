using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}
