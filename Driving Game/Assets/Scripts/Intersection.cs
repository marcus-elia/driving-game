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
    private static Vector3 leftInOffset = new Vector3(-sideLength/2 - buffer, 0, -sideLength/2);
    private static Vector3 leftOutOffset = new Vector3(-sideLength/2 - buffer, 0, sideLength/2);
    private static Vector3 rightInOffset = new Vector3(sideLength/2 + buffer, 0, sideLength/2);
    private static Vector3 rightOutOffset = new Vector3(sideLength/2 + buffer, 0, -sideLength/2);
    private static Vector3 upInOffset = new Vector3(-sideLength/2, 0, sideLength/2 + buffer);
    private static Vector3 upOutOffset = new Vector3(sideLength/2, 0, sideLength/2 + buffer);
    private static Vector3 downInOffset = new Vector3(sideLength/2, 0, -sideLength/2 - buffer);
    private static Vector3 downOutOffset = new Vector3(-sideLength/2, 0, -sideLength/2 - buffer);

    // It's also good to have the 4 corners for calculating curves
    private static Vector3 bottomRightOffset = new Vector3(sideLength/2 + buffer, 0, -sideLength/2 - buffer);
    private static Vector3 topRightOffset = new Vector3(sideLength/2 + buffer, 0, sideLength/2 + buffer);
    private static Vector3 topLeftOffset = new Vector3(-sideLength/2 - buffer, 0, sideLength/2 + buffer);
    private static Vector3 bottomLeftOffset = new Vector3(-sideLength/2 - buffer, 0, -sideLength/2 - buffer);

    private static int turnSmoothness = 5;
    private static float rightRadius = 7.5f;
    private static float leftRadius = 12.5f;

    // Private variables
    private Vector3 center;

    private List<Direction> directions;

    // For each choice a car can make, here are the points it needs to follow
    private Dictionary<TurnChoice, List<Vector3>> choiceToPoints;

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
        for(int i = 0; i < directions.Count; i++)
        {
            for(int j = 0; j < directions.Count; j++)
            {
                if(i == j) // Can't leave in the direction you came from
                {
                    continue;
                }
                choiceToPoints[new TurnChoice(directions[i], directions[j])] = calculateTurnPoints(directions[i], directions[j]);
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
            float theta = Mathf.PI / 2;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta += Mathf.PI / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * rightRadius, 0, corner.z + Mathf.Sin(theta) * rightRadius));
            }
        }
        else if(dirIn == Direction.Right && dirOut == Direction.Up)
        {
            Vector3 corner = center + topRightOffset;
            float theta = Mathf.PI;
            for (int i = 0; i < turnSmoothness; i++)
            {
                theta += Mathf.PI / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * rightRadius, 0, corner.z + Mathf.Sin(theta) * rightRadius));
            }
        }
        else if(dirIn == Direction.Up && dirOut == Direction.Left)
        {
            Vector3 corner = center + topLeftOffset;
            float theta = 3*Mathf.PI/2;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta += Mathf.PI / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * rightRadius, 0, corner.z + Mathf.Sin(theta) * rightRadius));
            }
        }
        else if(dirIn == Direction.Left && dirOut == Direction.Down)
        {
            Vector3 corner = center + bottomLeftOffset;
            float theta = 0f;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta += Mathf.PI / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * rightRadius, 0, corner.z + Mathf.Sin(theta) * rightRadius));
            }
        }
        // Turning left
        else if(dirIn == Direction.Down && dirOut == Direction.Left)
        {
            Vector3 corner = center + bottomLeftOffset;
            float theta = Mathf.PI / 2;
            for (int i = 0; i < turnSmoothness; i++)
            {
                theta += Mathf.PI / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * leftRadius, 0, corner.z + Mathf.Sin(theta) * leftRadius));
            }
        }
        else if(dirIn == Direction.Right && dirOut == Direction.Down)
        {
            Vector3 corner = center + bottomRightOffset;
            float theta = Mathf.PI;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta += Mathf.PI / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * leftRadius, 0, corner.z + Mathf.Sin(theta) * leftRadius));
            }
        }
        else if(dirIn == Direction.Up && dirOut == Direction.Right)
        {
            Vector3 corner = center + topRightOffset;
            float theta = 3*Mathf.PI/2;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta += Mathf.PI / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * leftRadius, 0, corner.z + Mathf.Sin(theta) * leftRadius));
            }
        }
        else if(dirIn == Direction.Left && dirOut == Direction.Up)
        {
            Vector3 corner = center + topLeftOffset;
            float theta = 0;
            for(int i = 0; i < turnSmoothness; i++)
            {
                theta += Mathf.PI / turnSmoothness;
                output.Add(new Vector3(corner.x + Mathf.Cos(theta) * leftRadius, 0, corner.z + Mathf.Sin(theta) * leftRadius));
            }
        }
        else
        {
            Debug.LogError("Can't leave an intersection from the same direction you entered.");
        }
        return output;
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
        return choiceToPoints[new TurnChoice(dirIn, dirOut)];
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
}
