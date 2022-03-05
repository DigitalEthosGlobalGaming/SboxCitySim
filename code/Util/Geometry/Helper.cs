﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Maker")]

namespace GridSystem.Geometry
{
    internal static class Helper
    {
        /**
 * Check for line overlapping another line.
 * 
 * @param lineA The line to test.
 * @param lineB The line to check for overlap.
 * @param excludeTangents Boolean to exclude exact endpoints and only look for deep overlaps.
 * @returns Boolean true if lineA is overlapped with lineB.
 */
        internal static bool IsLineOverlapping(Line lineA, Line lineB, bool excludeTangents)
        {
            var pointsOfIntersection = new List<Vector2>();

            bool checkPoints(int index, Line a, Line b)
            {

                bool checkPoint(Vector2 p)
                {
                    return IsBetweenPoints(p, a, excludeTangents);
                }

                return checkPoint(b.P1) || checkPoint(b.P2);
            }

            return checkPoints(0, lineA, lineB) || checkPoints(1, lineB, lineA);
        }

        /// <summary>
        /// Check if a given point is between a line's end points.
        /// </summary>
        /// <param name="pointInQuestion">The point to test.</param>
        /// <param name="line">Line to test against.</param>
        /// <param name="exclusive">Flag to exclude equaling the origin or end points.</param>
        /// <returns>Boolean true if point is between (or equal to) the line's origin and end points.</returns>
        internal static bool IsBetweenPoints(Vector2 pointInQuestion, Line line, bool exclusive)
        {
            bool oneDimension = false;
            {
                if (line.P1.x == line.P2.x)
                {
                    if (oneDimension)
                    {
                        return false;
                    }

                    oneDimension = true;
                    goto partY;
                }
                var origin_value = line.P1.x;
                var end_value = line.P2.x;
                if (!IsBetween(pointInQuestion.x, origin_value, end_value, exclusive))
                {
                    return false;
                }
            }

            partY:
            {
                if (line.P1.y == line.P2.y)
                {
                    if (oneDimension)
                    {
                        return false;
                    }

                    oneDimension = true;
                    goto partEnd;
                }

                var origin_value = line.P1.y;
                var end_value = line.P2.y;
                if (!IsBetween(pointInQuestion.y, origin_value, end_value, exclusive))
                {
                    return false;
                }
            }

            partEnd:
            return true;
        }

        /// <summary>
        /// Check if a given angle is between an arc's start and end angles.
        /// </summary>
        /// <param name="arc">Arc to test against.</param>
        /// <param name="angleInQuestion">The angle to test.</param>
        /// <param name="exclusive">Flag to exclude equaling the start or end angles.</param>
        /// <returns>Boolean true if angle is between (or equal to) the arc's start and end angles.</returns>
        internal static bool IsBetweenArcAngles(this Arc arc, float angleInQuestion, bool exclusive)
        {
            return arc.IsAngleOnArc(angleInQuestion, exclusive);
        }

        /// <summary>
        /// Check if a given number is between two given limits.
        /// </summary>
        /// <param name="valueInQuestion">The number to test.</param>
        /// <param name="limitA">First limit.</param>
        /// <param name="limitB">Second limit.</param>
        /// <param name="exclusive">Flag to exclude equaling the limits.</param>
        /// <returns>Boolean true if value is between (or equal to) the limits.</returns>
        internal static bool IsBetween(float valueInQuestion, float limitA, float limitB, bool exclusive)
        {
            if (exclusive)
            {
                return Math.Min(limitA, limitB) < valueInQuestion && valueInQuestion < Math.Max(limitA, limitB);
            }
            else
            {
                return Math.Min(limitA, limitB) <= valueInQuestion && valueInQuestion <= Math.Max(limitA, limitB);
            }
        }
        /**
 * Find out if two angles are equal.
 * 
 * @param angleA First angle.
 * @param angleB Second angle.
 * @returns true if angles are the same, false if they are not
 */
        public static bool IsAngleEqual(float angleA, float angleB, int accuracy = 6)
        {
            var a = Angle.NoRevolutions(angleA);
            var b = Angle.NoRevolutions(angleB);
            var d = Angle.NoRevolutions(Round(b - a, accuracy));
            return d == 0;
        }

        public static float Round(float n, int accuracy)
        {
            return (float)Math.Round(n, accuracy);
        }
    }
}
