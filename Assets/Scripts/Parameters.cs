using UnityEngine;
using System.Collections.Generic;
// this static class stores all the information required to simulation Astrobee
public static class Parameters
{
    // the vents are numbered properly and the parameters are defined here: 
    // https://www.notion.so/Astrobee-Dynamics-1ba5e61c54338086a670dc6c97f83654
    // unit in meters (m)
    public static float a = ((94.794f + 94.769f) / 2) / 1000;
    public static float b = 69.71f / 1000;
    public static float c = 139.70f / 1000;
    public static float d = 50.91f / 1000;
    public static float e = 56.982f / 1000;
    public static float f = 139.657f / 1000;
    // the position of the vents
    public static Vector3[] r = new Vector3[]
        {
            new Vector3(0,0,0),  // 0
            new Vector3(a, c, d),  // 1
            new Vector3(a, d, c),  // 2
            new Vector3(a,-c,-d), // 3
            new Vector3(a, -d,-c), // 4
            new Vector3(f, b,-e),  // 5
            new Vector3(f, -b, e), // 6
            new Vector3(-a, c,-d), // 7
            new Vector3(-a, d,-c), // 8
            new Vector3(-a, -c, d),// 9
            new Vector3(-a, -d, c), // 10
            new Vector3(-f, b, e), // 11
            new Vector3(-f, -b,-e), // 12
        };

    // The forces, unit in Newtons (N)
    public static float F1 = 1.5f;
    public static float F2 = 1.3f;

    public static Vector3[] F = new Vector3[]
        {
            new Vector3(0,0,0),  // 0
            new Vector3(0,-F2,0),  // 1
            new Vector3(0,0,-F2),  // 2
            new Vector3(0,F2,0), // 3
            new Vector3(0,0,F2), // 4
            new Vector3(-F1,0,0),  // 5
            new Vector3(-F1,0,0), // 6
            new Vector3(0,-F2,0), // 7
            new Vector3(0,0,F2), // 8
            new Vector3(0,F2,0),// 9
            new Vector3(0,0,-F2), // 10
            new Vector3(F1,0,0), // 11
            new Vector3(F1,0,0), // 12
        };

    // Mass unit, in kilogram (kg)
    public static float M = 9.12f;
    // Moment of inertia, unit in kg*m^2
    public static Vector3 I_value = new Vector3(1, 1, 1);
    public static Quaternion I_rotation = new Quaternion(1, 0, 0, 0);

    // Center of mass, unit in m
    public static Vector3 R = new Vector3(0, 0, 0);

    //opening the vent number corresponding to direction
    public static Dictionary<string, int[]> CONTROL = new Dictionary<string, int[]>
    {
        { "+x", new int[] { 11, 12 } },
        { "-x", new int[] { 5, 6 } },
        { "+y", new int[] { 3, 9 } },
        { "-y", new int[] { 1, 7 } },
        { "+z", new int[] { 4, 8 } },
        { "-z", new int[] { 2, 10 } },

        { "+Rx1", new int[] { 1, 3 } },   // same torque
        { "+Rx2", new int[] { 8, 10 } },
        { "-Rx1", new int[] { 2, 4 } },   // same torque
        { "-Rx2", new int[] { 7, 9 } },

        { "+Ry1", new int[] { 2, 8 } },   // bigger torque
        { "+Ry2", new int[] { 5, 11 } },
        { "-Ry1", new int[] { 4, 10 } },  // bigger torque
        { "-Ry2", new int[] { 6, 12 } },

        { "+Rz1", new int[] { 3, 7 } },   // bigger torque
        { "+Rz2", new int[] { 5, 12 } },
        { "-Rz1", new int[] { 1, 9 } },   // bigger torque
        { "-Rz2", new int[] { 6, 11 } },
    };

    // this section relates to motor controlling the vent
    // how fast each motor rotates, the value is 1 over the seconds it takes from fully open to 
    // fully closed
    public static float[] motorVelocity = new float[]
    {
        0f, //0
        10f,// 1
        10f,// 2
        10f,// 3
        10f,// 4
        10f,// 5
        10f,// 6
        10f,// 7
        10f,// 8
        10f,// 9
        10f,// 10
        10f,// 11
        10f,// 12
    };

    // how is the force related to the angle relative to its maximum magnitude
    public static float AngleToForce(float angle)
    {
        // This assumes the force is related to area(which is 1-cos angle) it exposes
        return 1 - Mathf.Cos(angle * Mathf.PI / 2);
    }


}