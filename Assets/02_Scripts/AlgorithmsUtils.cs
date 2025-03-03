using System.Collections.Generic;
using System;
using UnityEngine;

public class AlgorithmsUtils
{
    /// <summary>
    /// Checks if two rectangles intersect.
    /// </summary>
    /// <param name="a">First rectangle.</param>
    /// <param name="b">Second rectangle.</param>
    /// <returns>True if the rectangles intersect, false otherwise.</returns>
    public static bool Intersects(RectInt a, RectInt b)
    {
        return a.xMin < b.xMax &&
               a.xMax > b.xMin &&
               a.yMin < b.yMax &&
               a.yMax > b.yMin;
    }

    /// <summary>
    /// Returns the intersection of two rectangles.
    /// </summary>
    /// <param name="a">First rectangle.</param>
    /// <param name="b">Second rectangle.</param>
    /// <returns>The intersecting rectangle, or an empty rectangle if there is no intersection.</returns>
    public static RectInt Intersect(RectInt a, RectInt b)
    {
        int x = Mathf.Max(a.xMin, b.xMin);
        int y = Mathf.Max(a.yMin, b.yMin);
        int width = Mathf.Min(a.xMax, b.xMax) - x;
        int height = Mathf.Min(a.yMax, b.yMax) - y;

        if (width <= 0 || height <= 0)
        {
            return new RectInt();
        }
        else
        {
            return new RectInt(x, y, width, height);
        }
    }

    /// <summary>
    /// Fills a rectangular area in a 2D array with a specified value.
    /// </summary>
    /// <param name="array">The 2D array to fill.</param>
    /// <param name="area">The rectangular area to fill.</param>
    /// <param name="value">The value to fill the area with.</param>
    public static void FillRectangle(char[,] array, RectInt area, char value)
    {
        for (int i = area.y; i < area.y + area.height; i++)
        {
            for (int j = area.x; j < area.x + area.width; j++)
            {
                array[i, j] = value;
            }
        }
    }

    /// <summary>
    /// Draws the outline of a rectangular area in a 2D array with a specified value.
    /// </summary>
    /// <param name="array">The 2D array to draw on.</param>
    /// <param name="area">The rectangular area to outline.</param>
    /// <param name="value">The value to use for the outline.</param>
    public static void FillRectangleOutline(char[,] array, RectInt area, char value)
    {
        int endX = area.x + area.width - 1;
        int endY = area.y + area.height - 1;

        // Draw top and bottom borders
        for (int x = area.x; x <= endX; x++)
        {
            array[area.y, x] = value;
            array[endY, x] = value;
        }

        // Draw left and right borders
        for (int y = area.y + 1; y < endY; y++)
        {
            array[y, area.x] = value;
            array[y, endX] = value;
        }
    }

    /// <summary>
    /// Draws a rectangle in the Unity editor for debugging purposes.
    /// </summary>
    /// <param name="rectInt">The rectangle to draw.</param>
    /// <param name="color">The color of the rectangle.</param>
    /// <param name="duration">How long the rectangle should be visible.</param>
    /// <param name="depthTest">Whether the rectangle should be occluded by objects closer to the camera.</param>
    /// <param name="height">The height of the rectangle.</param>
    public static void DebugRectInt(RectInt rectInt, Color color, float duration = 0f, bool depthTest = false, float height = 0.01f)
    {
        DebugExtension.DebugBounds(new Bounds(new Vector3(rectInt.center.x, 0, rectInt.center.y), new Vector3(rectInt.width, height, rectInt.height)), color, duration, depthTest);
    }
}

public static class ListExtensions
{
    public static T Pop<T>(this List<T> list, int index)
    {
        if (list == null || list.Count == 0)
        {
            throw new InvalidOperationException("Cannot pop from an empty list.");
        }

        T item = list[index];
        list.RemoveAt(index);
        return item;
    }
}
