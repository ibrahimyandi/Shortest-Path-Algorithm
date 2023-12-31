using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float moveSpeed = 50f; // Kamera hareket hızı
    private float rotationSpeed = 300f; // Kamera dönme hızı
    public bool cameraControl = false;

    void Update()
    {
        if (cameraControl == true)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

            // Sağ tık basılı tutularak kamera dönüş kontrolü
            if (Input.GetMouseButton(1)) // 1 sağ tık, 0 sol tık
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                // Y ekseninde dönme
                transform.Rotate(Vector3.up * mouseX * rotationSpeed * Time.deltaTime);

                // X ekseninde dönme
                transform.Rotate(Vector3.left * mouseY * rotationSpeed * Time.deltaTime);
            }
        }
    }
}
