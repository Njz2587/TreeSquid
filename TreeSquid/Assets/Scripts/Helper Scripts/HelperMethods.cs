using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    public static float QuickDistance(Vector3 firstPosition, Vector3 secondPosition)
    {
        // Create the vector for heading
        Vector3 heading;

        // Calculate the heading values
        heading.x = firstPosition.x - secondPosition.x;
        heading.y = firstPosition.y - secondPosition.y;
        heading.z = firstPosition.z - secondPosition.z;

        // Calculate the distance squared
        float distanceSquared = heading.x * heading.x + heading.y * heading.y + heading.z * heading.z;
        // Calculate and return the actual distance by finding the square root of that
        return  Mathf.Sqrt(distanceSquared);
    }
}
