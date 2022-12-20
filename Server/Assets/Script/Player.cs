using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    private float moveSpeed = 5f / (1000f / 15f);
    private bool[] inputs;

    public void Initialize(int newId, string newUsername, Vector3 spawnPosition)
    {
        id = newId;
        username = newUsername;

        inputs = new bool[4];
    }

    public void Update()
    {

        Vector2 inputDirection = Vector2.zero;
        if (inputs[0])
        {
            inputDirection.y += 1;
        }
        if (inputs[1])
        {
            inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            inputDirection.x += 1;
        }
        if (inputs[3])
        {
            inputDirection.x -= 1;
        }
        Move(inputDirection);
    }

    private void Move(Vector2 inputDirection)
    {
        Vector3 direction = transform.right * inputDirection.x + transform.forward * inputDirection.y;
        transform.position += direction * moveSpeed;

        PacketSender.PlayerPosition(this);
        PacketSender.PlayerRotation(this);
    }

    public void SetInput(bool[] newInputs, Quaternion newRotation)
    {
        inputs = newInputs;
        transform.rotation = newRotation;
    }
}
