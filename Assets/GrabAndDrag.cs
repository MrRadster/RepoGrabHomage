using UnityEngine;

public class GrabAndDragFPS : MonoBehaviour
{
    [Header("Grab Settings")]
    public float grabDistance = 4.5f;
    public float holdDistance = 2.2f;
    public float dragSpeed = 18f;
    public float maxHoldDistance = 3.8f;
    public LayerMask grabbableLayer;

    [Header("Throw")]
    public bool canThrow = true;
    public float throwForce = 12f;
    public float upwardThrowBonus = 2.5f;

    [Header("References")]
    public Transform holdPoint;          
    public Camera playerCamera;          

    private Rigidbody grabbedRb;
    private bool isGrabbing;
    private float originalDrag;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (holdPoint == null)
        {
            GameObject hp = new GameObject("HoldPoint");
            hp.transform.SetParent(playerCamera.transform);
            hp.transform.localPosition = new Vector3(0f, 0f, holdDistance);
            holdPoint = hp.transform;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isGrabbing)
                TryGrab();
            else
                Release(canThrow);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isGrabbing)
                TryGrab();
            else
                Release(canThrow);
        }
    }

    void FixedUpdate()
    {
        if (isGrabbing && grabbedRb != null)
        {
            DragObject();
        }
    }

    void TryGrab()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabbableLayer))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;

            if (rb != null && !rb.isKinematic)
            {
                grabbedRb = rb;
                isGrabbing = true;

                originalDrag = grabbedRb.linearDamping;   

                grabbedRb.useGravity = false;
                grabbedRb.linearDamping = 12f;            
            }
        }
    }

    void DragObject()
    {
        Vector3 targetPos = holdPoint.position;

        Vector3 toTarget = targetPos - playerCamera.transform.position;
        if (toTarget.magnitude > maxHoldDistance)
        {
            targetPos = playerCamera.transform.position + toTarget.normalized * maxHoldDistance;
        }

        Vector3 newPos = Vector3.Lerp(grabbedRb.position, targetPos, Time.fixedDeltaTime * dragSpeed);
        grabbedRb.MovePosition(newPos);

    }

    void Release(bool throwObject)
    {
        if (grabbedRb == null) return;

        grabbedRb.useGravity = true;
        grabbedRb.linearDamping = originalDrag;

        if (throwObject)
        {
            Vector3 throwDir = playerCamera.transform.forward * throwForce +
                               playerCamera.transform.up * upwardThrowBonus;
            grabbedRb.AddForce(throwDir, ForceMode.Impulse);
        }

        isGrabbing = false;
        grabbedRb = null;
    }

    void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * grabDistance);
    }
}