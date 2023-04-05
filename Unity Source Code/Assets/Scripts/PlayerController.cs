using GraphFoundation;
using System;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float Sensitivity
    {
        get { return sensitivity; }
        set { sensitivity = value; }
    }
    public Quaternion xQuat;
    public Quaternion yQuat;
    private float speed = 10.0f;
    private float frontalMovementInput;
    private float sidewayMovementInput;
    private float verticalMovement;

    Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

    [Range(0.1f, 9f)][SerializeField] float sensitivity = 2f;
    [Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
    [Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;

    public Vector2 rotation = Vector2.zero;

    public float MovementSpeed = 0.5F;
    private LineRenderer drawLine;
    private Vector3 CameraStartPosition;
    private Vector3 CameraEndPosition;
    private Quaternion CameraStartRotation;
    private Quaternion CameraEndRotation;
    private bool DoMovement = false;
    private float LerpMovement = 0;
    private GameObject PopUpUI;
    private GameObject Attributes;
    private Collider focusedNode;

    // Start is called before the first frame update
    void Start()
    {
        // set PopUpUI false on awake AFTER we have the reference to it stored.
        PopUpUI = GameObject.FindGameObjectWithTag("PopUp");
        Attributes = GameObject.FindGameObjectWithTag("Attributes");
        PopUpUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Get user movement from keyboard
;       ObtainMovement();
        // Get user mouse movement
        ObtainLook();
    }

    private void ObtainLook()
    {
        // Only rotate if inside playmode screen
        if (!screenRect.Contains(Input.mousePosition))
            return;
        // Camera movement
        rotation.x += Input.GetAxis("Mouse X") * sensitivity;
        rotation.y += Input.GetAxis("Mouse Y") * sensitivity;

        rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
        xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);

        transform.localRotation = xQuat * yQuat;
        // Lock cursor in window and disable presence
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        CheckIfIntersectsWithCamera();
        MoveToPointUpdate();
    }

    private void ObtainMovement()
    {
        // Obtain frontal movement input
        frontalMovementInput = Input.GetAxisRaw("Vertical");
        // Obtain sideways movement input
        sidewayMovementInput = Input.GetAxisRaw("Horizontal");
        // Obtain vertical movement input
        verticalMovement = Input.GetAxis("QandE");
        // Forward movement
        transform.position += transform.forward * Time.deltaTime * speed * frontalMovementInput;
        // Sideways movement
        transform.position += transform.right * Time.deltaTime * speed * sidewayMovementInput;
        // Vertical movement
        transform.Translate(Vector3.up * speed * Time.deltaTime * verticalMovement);
        // Mouse scroll movement
        transform.Translate(Input.mouseScrollDelta.y * speed * transform.forward);
    }

    private void CheckIfIntersectsWithCamera()
    {
        // Cast a ray from the main camera
        Ray ray = new Ray(transform.position,transform.forward);

        // Define a maximum distance for the raycast
        float maxDistance = 120.0f;

        // Create a RaycastHit object to store the result of the raycast
        RaycastHit hit;

        // Check if the ray intersects with an object within the maximum distance
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // If the ray intersects with an object, change its color to red
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            // Change the color of the object to yellow
            renderer.material.color = Color.yellow;
            // show PopUpUI;
            PopUpUI.SetActive(true);
            focusedNode = hit.collider;
            // if camera intersects with THIS object then chasnge text
            PopUpUI.GetComponentInChildren<TextMeshProUGUI>().text = $"Node ID: {hit.collider.GetComponent<NodeBehaviour>().nodeID}";
            Attributes.GetComponentInChildren<TextMeshProUGUI>().text = $"RDFS label: {hit.collider.GetComponent<NodeBehaviour>().properties["rdfs__label"]}\n" +
               $"URI: {hit.collider.GetComponent<NodeBehaviour>().properties["uri"]}";
            
            // is lerping actually needed, more of a gimmick
            // if node is clicked then unfold the node
            if (Input.GetMouseButtonDown(0))
            {
                if (!focusedNode.transform.GetComponent<NodeBehaviour>().expanded) 
                {
                    focusedNode.transform.GetComponent<NodeBehaviour>().DoUnfolding();

                }
                else
                {
                    focusedNode.transform.GetComponent<NodeBehaviour>().UndoUnfolding();
                }
                

                //    CameraStartPosition = transform.position;
                //    CameraStartRotation = transform.rotation;
                //    CameraEndPosition = new Vector3(hit.transform.position.x, hit.transform.position.y, hit.transform.position.z - 70);
                //    CameraEndRotation = Quaternion.identity;
                //    LerpMovement = 0F;
                //    DoMovement = true;
            }
        }
        else
        {
            // Do not show PopUpUI when not in focus
            PopUpUI.SetActive(false);
            if (focusedNode != null)
            {
                focusedNode.GetComponent<Renderer>().material.color = focusedNode.transform.GetComponent<NodeBehaviour>().defaultColor;
                focusedNode = null;
            }
        }
    }
    private void MoveToPointUpdate()
    {
        if (DoMovement)
        {
            transform.position = Vector3.Lerp(CameraStartPosition, CameraEndPosition, LerpMovement);
            transform.rotation = Quaternion.Lerp(CameraStartRotation, CameraEndRotation, LerpMovement);
            LerpMovement += Time.deltaTime * MovementSpeed;
            if (LerpMovement >= 1F)
            {
                LerpMovement = 0F;
                DoMovement = false;
                rotation.x = 0;
                rotation.y = 0;
            }
        }
    }
}
