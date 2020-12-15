using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { Left, Right, Up, Down }

public struct TurnChoice
{
    public Direction inDirection;
    public Direction outDirection;
    public TurnChoice(Direction inDirection, Direction outDirection)
    {
        this.inDirection = inDirection;
        this.outDirection = outDirection;
    }

}

public class Intersection
{
    private static float sideLength = 10f;  // combined width of lanes/center line
    private static float buffer = 2f;       // sidewalk width

    // There are 8 main target points for cars to drive through. This is the offset from the center
    // of the intersection.
    public static Vector3 leftInOffset = new Vector3(-sideLength/2 - buffer, 0, -sideLength/4);
    public static Vector3 leftOutOffset = new Vector3(-sideLength/2 - buffer, 0, sideLength/4);
    public static Vector3 rightInOffset = new Vector3(sideLength/2 + buffer, 0, sideLength/4);
    public static Vector3 rightOutOffset = new Vector3(sideLength/2 + buffer, 0, -sideLength/4);
    public static Vector3 upInOffset = new Vector3(-sideLength/4, 0, sideLength/2 + buffer);
    public static Vector3 upOutOffset = new Vector3(sideLength/4, 0, sideLength/2 + buffer);
    public static Vector3 downInOffset = new Vector3(sideLength/4, 0, -sideLength/2 - buffer);
    public static Vector3 downOutOffset = new Vector3(-sideLength/4, 0, -sideLength/2 - buffer);

    // It's also good to have the 4 corners for calculating curves
    private static Vector3 bottomRightOffset = new Vector3(sideLength/2 + buffer, 0, -sideLength/2 - buffer);
    private static Vector3 topRightOffset = new Vector3(sideLength/2 + buffer, 0, sideLength/2 + buffer);
    private static Vector3 topLeftOffset = new Vector3(-sideLength/2 - buffer, 0, sideLength/2 + buffer);
    private static Vector3 bottomLeftOffset = new Vector3(-sideLength/2 - buffer, 0, -sideLength/2 - buffer);

    private static int turnSmoothness = 12;
    private static float rightRadius = 4.5f;
    private static float leftRadius = 9.5f;

    // Private variables
    private Vector3 center;

    private List<Direction> directions;

    // For each choice a car can make, here are the points it needs to follow
    private Dictionary<int, List<Vector3>> intToPoints;

    // Pointers to neighboring intersections
    private Intersection left, right, up, down;


    public Intersection(Vector3 inputCenter, Intersection inputLeft, Intersection inputRight, Intersection inputUp, Intersection inputDown)
    {
        center = inputCenter;
        left = inputLeft;
        right = inputRight;
        up = inputUp;
        down = inputDown;

        // Add the directions for when the given intersections are not null
        this.initializeDirections();

        // For each possible turn choice, make the list of points for cars to follow
        this.createChoiceToPoints();
    }

    private void initializeDirections()
    {
        directions = new List<Direction>();
        if(left != null)
        {
            directions.Add(Direction.Left);
        }
        if(right != null)
        {
            directions.Add(Direction.Right);
        }
        if(up != null)
        {
            directions.Add(Direction.Up);
        }
        if(down != null)
        {
            directions.Add(Direction.Down);
        }
    }

    private void createChoiceToPoints()
    {
        intToPoints = new Dictionary<int, List<Vector3>>();
        for (int i = 0; i < directions.Count; i++)
        {
            for(int j = 0; j < directions.Count; j++)
            {
                if(i == j) // Can't leave in the direction you came from
                {
                    continue;
                }
                List<Vector3> points = calculateTurnPoints(directions[i], directions[j]);
                if(points.Count == 0)
                {
                    Debug.LogError("No");
                }
                intToPoints[directionsToInt(directions[i], directions[j])] = points;
            }
        }
    }

    private List<Vector3> calculateTurnPoints(Direction dirIn, Direction dirOut)
    {
        List<Vector3> output = new List<Vector3>();

        // Going straight
        if(dirIn == Direction.Down && dirOut == Direction.Up)
        {
            output.Add(center + upOutOffset);
        }
        else if(dirIn == Direction.Up && dirOut == Direction.Down)
        {
            output.Add(center + downOutOffset);
        }
        else if(dirIn == Direction.Left && dirOut == Direction.Right)
        {
            output.Add(center + rightOutOffset);
        }
        else if(dirIn == Direction.Right && dirOut == Direction.Left)
        {
            output.Add(center + leftOutOffset);
        }
        // Turning right
        else if(dirIn == Direction.Down && dirOut == Direction.Right)
        {
            Vector3 corner = center + bottomRightOffset;
            float theta = Mathf.PI;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta -= (Mathf.PI/2) / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * rightRadius, 0, corner.z + Mathf.Sin(theta) * rightRadius));
            }
        }
        else if(dirIn == Direction.Right && dirOut == Direction.Up)
        {
            Vector3 corner = center + topRightOffset;
            float theta = 3*Mathf.PI/2;
            for (int i = 0; i < turnSmoothness; i++)
            {
                theta -= (Mathf.PI/2) / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * rightRadius, 0, corner.z + Mathf.Sin(theta) * rightRadius));
            }
        }
        else if(dirIn == Direction.Up && dirOut == Direction.Left)
        {
            Vector3 corner = center + topLeftOffset;
            float theta = 0;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta -= (Mathf.PI / 2) / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * rightRadius, 0, corner.z + Mathf.Sin(theta) * rightRadius));
            }
        }
        else if(dirIn == Direction.Left && dirOut == Direction.Down)
        {
            Vector3 corner = center + bottomLeftOffset;
            float theta = Mathf.PI/2;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta -= (Mathf.PI / 2) / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * rightRadius, 0, corner.z + Mathf.Sin(theta) * rightRadius));
            }
        }
        // Turning left
        else if(dirIn == Direction.Down && dirOut == Direction.Left)
        {
            Vector3 corner = center + bottomLeftOffset;
            float theta = 0;
            for (int i = 0; i < turnSmoothness; i++)
            {
                theta += (Mathf.PI/2) / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * leftRadius, 0, corner.z + Mathf.Sin(theta) * leftRadius));
            }
        }
        else if(dirIn == Direction.Right && dirOut == Direction.Down)
        {
            Vector3 corner = center + bottomRightOffset;
            float theta = Mathf.PI/2;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta += (Mathf.PI / 2) / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * leftRadius, 0, corner.z + Mathf.Sin(theta) * leftRadius));
            }
        }
        else if(dirIn == Direction.Up && dirOut == Direction.Right)
        {
            Vector3 corner = center + topRightOffset;
            float theta = Mathf.PI;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta += (Mathf.PI / 2) / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * leftRadius, 0, corner.z + Mathf.Sin(theta) * leftRadius));
            }
        }
        else if(dirIn == Direction.Left && dirOut == Direction.Up)
        {
            Vector3 corner = center + topLeftOffset;
            float theta = 3*Mathf.PI/2;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta += (Mathf.PI / 2) / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * leftRadius, 0, corner.z + Mathf.Sin(theta) * leftRadius));
            }
        }
        else
        {
            Debug.LogError("Can't leave an intersection from the same direction you entered.");
        }

        return output;
    }

    // Adding in new neighbors
    public void addLeft(Intersection inputLeft)
    {
        left = inputLeft;
        // Re-initialize directions and points
        this.initializeDirections();
        this.createChoiceToPoints();
    }
    public void addRight(Intersection inputRight)
    {
        right = inputRight;
        // Re-initialize directions and points
        this.initializeDirections();
        this.createChoiceToPoints();
    }
    public void addUp(Intersection inputUp)
    {
        up = inputUp;
        // Re-initialize directions and points
        this.initializeDirections();
        this.createChoiceToPoints();
    }
    public void addDown(Intersection inputDown)
    {
        down = inputDown;
        // Re-initialize directions and points
        this.initializeDirections();
        this.createChoiceToPoints();
    }

    // Functions for cars to use
    public Direction getRandomDirection(Direction dirIn)
    {
        int randIndex = (int)Random.Range(0, directions.Count);
        Direction dirOut = directions[randIndex];
        while(dirOut == dirIn)
        {
            randIndex = (int)Random.Range(0, directions.Count);
            dirOut = directions[randIndex];
        }
        return dirOut;
    }
    public List<Vector3> getTurnPoints(Direction dirIn, Direction dirOut)
    {
        return intToPoints[directionsToInt(dirIn, dirOut)];
    }
    public Intersection getNextIntersection(Direction dirOut)
    {
        if(dirOut == Direction.Left)
        {
            return left;
        }
        else if(dirOut == Direction.Right)
        {
            return right;
        }
        else if(dirOut == Direction.Up)
        {
            return up;
        }
        else
        {
            return down;
        }
    }
    public Vector3 getCenter()
    {
        return center;
    }

    // Needed when going through intersections
    public static Direction reverseDirection(Direction dir)
    {
        if (dir == Direction.Left)
        {
            return Direction.Right;
        }
        if (dir == Direction.Right)
        {
            return Direction.Left;
        }
        if (dir == Direction.Up)
        {
            return Direction.Down;
        }
        return Direction.Up;
    }
    // Converts pairs of directions to ints
    // LL = 0, LR = 1, LU = 2, LD = 3, RL = 4, RR = 5, etc
    public static int directionsToInt(Direction dir1, Direction dir2)
    {
        int a, b;
        if(dir1 == Direction.Left)
        {
            a = 0;
        }
        else if(dir1 == Direction.Right)
        {
            a = 1;
        }
        else if(dir1 == Direction.Up)
        {
            a = 2;
        }
        else
        {
            a = 3;
        }
        if (dir2 == Direction.Left)
        {
            b = 0;
        }
        else if (dir2 == Direction.Right)
        {
            b = 1;
        }
        else if (dir2 == Direction.Up)
        {
            b = 2;
        }
        else
        {
            b = 3;
        }
        return 4 * a + b;
    }
}
