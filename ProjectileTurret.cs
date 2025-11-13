using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTurret : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 1;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle;

    public LineRenderer lineRenderer;

    public int maxIterations = 10000;
    public int maxSegmentCount = 300;
    public float segmentStepModulo = 10f;

    private Vector3[] segments;
    private int numSegments = 0;

    List<Vector3> points = new List<Vector3>();

    // Start is called before the first frame update


    void Start()
    {
        Enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();
        RotateGun();

        if (Input.GetButtonDown("Fire1"))
            Fire();
    }

    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        projectile.GetComponent<Rigidbody>().linearVelocity = projectileSpeed * barrelEnd.transform.forward;
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
            //Debug.Log("hit ground");
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void RotateGun()
    {
        float? angle = CalculateTrajectory(crosshair.transform.position, useLowAngle);
        if (angle != null)
            gun.transform.localEulerAngles = new Vector3(360f - (float)angle, 0, 0);
    }

    float? CalculateTrajectory(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;
        
        float y = targetDir.y;
        targetDir.y = 0;

        float x = targetDir.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);
        float g = gravity.y;
        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - g * ((g * x2) + (2 * y * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = v2 + root;
            float lowAngle = v2 - root;

            if (useLow)
                return (Mathf.Atan2(lowAngle, g * x) * Mathf.Rad2Deg);
            else
                return (Mathf.Atan2(highAngle, g * x) * Mathf.Rad2Deg);
        }
        else
            return null;
    }
    public bool Enabled
    {
        get
        {
            return lineRenderer.enabled;
        }
        set
        {
            lineRenderer.enabled = value;
        }
    }



    public void SimulatePath(GameObject gameObject, Vector3 forceDirection, float mass, float drag)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

        float timestep = Time.fixedDeltaTime;

        float stepDrag = 1 - drag * timestep;
        Vector3 velocity = forceDirection / mass * timestep;
        Vector3 gravity = Physics.gravity * timestep * timestep;
        Vector3 position = gameObject.transform.position + rigidbody.centerOfMass;

        if (segments == null || segments.Length != maxSegmentCount)
        {
            segments = new Vector3[maxSegmentCount];
        }

        segments[0] = position;
        numSegments = 1;

        for (int i = 0; i < maxIterations && numSegments < maxSegmentCount && position.y > 0f; i++)
        {
            velocity += gravity;
            velocity *= stepDrag;

            position += velocity;

            if (i % segmentStepModulo == 0)
            {
                segments[numSegments] = position;
                numSegments++;
            }
        }

        Draw();
    }

    private void Draw()
    {
        Color startColor = Color.magenta;
        Color endColor = Color.magenta;
        startColor.a = 1f;
        endColor.a = 1f;

        lineRenderer.transform.position = segments[0];

        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;

        lineRenderer.positionCount = numSegments;
        for (int i = 0; i < numSegments; i++)
        {
            lineRenderer.SetPosition(i, segments[i]);
        }
    }
}
