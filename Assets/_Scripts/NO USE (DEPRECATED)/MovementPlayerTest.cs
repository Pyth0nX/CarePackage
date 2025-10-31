using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementPlayerTest : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    private CharacterController controller;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // Oculta el cursor
    }

    void Update()
    {
        // Movimiento horizontal (WASD)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.SimpleMove(move * speed);

        // Rotación con el mouse (mirar alrededor)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotar el cuerpo del jugador en el eje Y
        transform.Rotate(Vector3.up * mouseX);

        // Rotar la cámara verticalmente (eje X)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}