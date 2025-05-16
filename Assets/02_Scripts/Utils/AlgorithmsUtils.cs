using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    using Dungeon.Data;
    using Dungeon.DataStructures;
    using System.Collections;

    /// <summary>
    /// Utility class for various algorithms and operations related to dungeon generation.
    /// </summary>
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
        /// <summary>
        /// Calculates the overlay position of a given room.
        /// </summary>
        /// <param name="room">The room for which to calculate the overlay position.</param>
        /// <returns>A RectInt representing the overlay position of the room.</returns>
        public static RectInt CalculateOverlayPosition(Room room)
        {
            RectInt position = room.roomDimensions;
            position.x += position.width / 2;
            position.y += position.height / 2;
            position.width = 1;
            position.height = 1;
            return position;
        }

        /// <summary>
        /// Delays the execution based on the specified delay type.
        /// </summary>
        /// <param name="delayType">The type of delay to apply (Instant, Delayed, KeyPress).</param>
        /// <returns>An IEnumerator for the coroutine.</returns>
        public static IEnumerator Delay(DelayType delayType)
        {
            DelaySettings delaySettings = DungeonDataGenerator.Instance.GenerationSettings.delaySettings;

            if (!delaySettings.UseDefaultDelayType)
            {
                switch (delayType)
                {
                    case DelayType.Instant:
                        break;
                    case DelayType.Delayed:
                        yield return new WaitForSeconds(delaySettings.actionDelay);
                        break;
                    case DelayType.KeyPress:
                        yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));
                        break;
                }
            }
            else
            {
                switch (delaySettings.defaultDelayType)
                {
                    case DelayType.Instant:
                        break;
                    case DelayType.Delayed:
                        yield return new WaitForSeconds(delaySettings.actionDelay);
                        break;
                    case DelayType.KeyPress:
                        yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));
                        break;
                }
            }
        }

        /// <summary>
        /// Determines whether the dungeon generation should proceed instantly based on the provided delay type
        /// and the current delay settings in the generation configuration.
        /// </summary>
        /// <param name="passedDelay">
        /// The delay type to check against the current settings. If set to <see cref="DelayType.Instant"/>,
        /// the method will return true regardless of the default delay type.
        /// </param>
        /// <returns>
        /// True if the generation should proceed instantly (either the default delay type is set to Instant
        /// and is being used, or the passed delay type is Instant); otherwise, false.
        /// </returns>
        public static bool DoInstantPass(DelayType passedDelay)
        {
            return DungeonDataGenerator.Instance.GenerationSettings.delaySettings.UseDefaultDelayType && DungeonDataGenerator.Instance.GenerationSettings.delaySettings.defaultDelayType == DelayType.Instant || passedDelay == DelayType.Instant;
        }
    }


    public static class ListExtensions
    {
        /// <summary>
        /// Removes and returns the element at the specified index from the list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to remove the element from.</param>
        /// <param name="index">The zero-based index of the element to remove and return.</param>
        /// <returns>The element that was removed from the list.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the list is null or empty.</exception>
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
}