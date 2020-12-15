using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    public CarAI car1;

    List<Intersection> intersections;

    // Start is called before the first frame update
    void Start()
    {
        /*
         * 0 - 3   7 - 11
         * |   |   |   |
         * 1 - 4 - 8 - 12
         * |   |   |   |
         * |   |   |   13-15
         * 2 - 5 - 9   |   |
         *     |   |   |   |
         *     6 - 10 -14- 16
         * 
         */
        intersections = new List<Intersection>();
        intersections.Add(new Intersection(new Vector3(-88, 0, 81), null, null, null, null));
        intersections.Add(new Intersection(new Vector3(-88, 0, 37), null, null, intersections[0], null));
        intersections[0].addDown(intersections[1]);
        intersections.Add(new Intersection(new Vector3(-88, 0, -37), null, null, intersections[1], null));
        intersections[1].addDown(intersections[2]);
        intersections.Add(new Intersection(new Vector3(-44, 0, 81), intersections[0], null, null, null));
        intersections[0].addRight(intersections[3]);
        intersections.Add(new Intersection(new Vector3(-44, 0, 37), intersections[1], null, intersections[3], null));
        intersections[1].addRight(intersections[4]);
        intersections[3].addDown(intersections[4]);
        intersections.Add(new Intersection(new Vector3(-44, 0, -37), intersections[2], null, intersections[4], null));
        intersections[2].addRight(intersections[5]);
        intersections[4].addDown(intersections[5]);
        intersections.Add(new Intersection(new Vector3(-44, 0, -81), null, null, intersections[5], null));
        intersections[5].addDown(intersections[6]);
        intersections.Add(new Intersection(new Vector3(0, 0, 81), null, null, null, null));
        intersections.Add(new Intersection(new Vector3(0, 0, 37), intersections[4], null, intersections[7], null));
        intersections[4].addRight(intersections[8]);
        intersections[7].addDown(intersections[8]);
        intersections.Add(new Intersection(new Vector3(0, 0, -37), intersections[5], null, intersections[8], null));
        intersections[5].addRight(intersections[9]);
        intersections[8].addDown(intersections[9]);
        intersections.Add(new Intersection(new Vector3(0, 0, -81), intersections[6], null, intersections[9], null));
        intersections[6].addRight(intersections[10]);
        intersections[9].addDown(intersections[10]);
        intersections.Add(new Intersection(new Vector3(44, 0, 81), intersections[7], null, null, null));
        intersections[7].addRight(intersections[11]);
        intersections.Add(new Intersection(new Vector3(44, 0, 37), intersections[8], null, intersections[11], null));
        intersections[8].addRight(intersections[12]);
        intersections[11].addDown(intersections[12]);
        intersections.Add(new Intersection(new Vector3(44, 0, -7), null, null, intersections[12], null));
        intersections[12].addDown(intersections[13]);
        intersections.Add(new Intersection(new Vector3(44, 0, -81), intersections[10], null, intersections[13], null));
        intersections[10].addRight(intersections[14]);
        intersections[13].addDown(intersections[14]);
        intersections.Add(new Intersection(new Vector3(58, 0, -7), intersections[13], null, null, null));
        intersections[13].addRight(intersections[15]);
        intersections.Add(new Intersection(new Vector3(58, 0, -81), intersections[14], null, intersections[15], null));
        intersections[14].addRight(intersections[16]);
        intersections[15].addDown(intersections[16]);

        car1.setCurrentInt(intersections[8]);
        car1.setCurrentDir(Direction.Down);
        car1.setNextInt(intersections[7]);
        car1.setNextDir(Direction.Up);
        car1.setLocation(Intersection.downInOffset + intersections[8].getCenter());
        car1.setAngle(Mathf.PI / 2);
        car1.setPoints(intersections[8].getTurnPoints(Direction.Down, Direction.Up));
        car1.setVelocity();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
