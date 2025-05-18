using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); // 'A' and 'D' keys
        float moveZ = Input.GetAxisRaw("Vertical");   // 'W' and 'S' keys

        Quaternion rotation = Quaternion.Euler(0f, 45f, 0f);

        Vector3 move = (rotation * new Vector3(moveX, 0f, moveZ)).normalized;
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}
