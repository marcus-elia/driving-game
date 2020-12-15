using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour
{
    private Intersection currentInt;
    private Intersection nextInt;
    private Direction currentDir;
    private Direction nextDir;
    private List<Vector3> points;
    private Vector3 target;
    private Vector3 velocity;
    private float currentAngle;

    public float speed;
    public float centerHeight; // How high above the ground it is

    public static float tolerance = 0.1f;

    private int targetPointsIndex = 0; // Keep track of where we are in the list

    // Start is called before the first frame update
    void Start()
    {
        velocity = new Vector3(0, 0, 0);
        currentAngle = 0;
        points = new List<Vector3>();
    }

    public void setCurrentInt(Intersection inputInt)
    {
        currentInt = inputInt;
    }
    public void setCurrentDir(Direction inputDir)
    {
        currentDir = inputDir;
    }
    public void setNextInt(Intersection inputInt)
    {
        nextInt = inputInt;
    }
    public void setNextDir(Direction inputDir)
    {
        nextDir = inputDir;
    }
    public void setLocation(Vector3 location)
    {
        transform.position.Set(location.x, centerHeight, location.z);
    }
    public void setAngle(float inputAngle)
    {
        currentAngle = inputAngle;
    }
    public void setPoints(List<Vector3> inputPoints)
    {
        points = inputPoints;
        target = points[0];
    }
    public void setVelocity()
    {
        float angleToMove = Mathf.Atan2(target.z - transform.position.z, target.x - transform.position.x);
        velocity.x = Mathf.Cos(angleToMove) * speed;
        velocity.z = Mathf.Sin(angleToMove) * speed;
        transform.Rotate(0f, -Mathf.Rad2Deg*(angleToMove - currentAngle), 0f, Space.World);
        currentAngle = angleToMove;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Mathf.Sqrt((target.x - transform.position.x)* (target.x - transform.position.x) + (target.z - transform.position.z) * (target.z - transform.position.z));
        
        // If the car got to it's target
        if(distance < tolerance)
        {
            targetPointsIndex++;
            if(points.Count == targetPointsIndex) // If we finished the intersection
            {
                currentInt = nextInt;
                currentDir = Intersection.reverseDirection(nextDir);
                nextDir = currentInt.getRandomDirection(currentDir);
                nextInt = currentInt.getNextIntersection(nextDir);
                points = currentInt.getTurnPoints(currentDir, nextDir);
                targetPointsIndex = 0;
            }
            // Now target the next point  
            target = points[targetPointsIndex];

            // Set velocity and angles accordingly
            this.setVelocity();
        }
        // If the car is very close
        else if(distance < speed)
        {
            transform.position.Set(target.x, centerHeight, target.z);
        }

        // Move
        transform.position += velocity;
    }
}
