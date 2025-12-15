// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.
//

// Requires System.Numerics.Vectors (SUPPORTS_VECTOR or OFFICIAL_VECTOR) for base types
#if !SUPPORTS_MATRIX4X4_LEFTHANDED

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Polyfills for Matrix4x4 left-handed coordinate system methods added in .NET 8.0.
/// </summary>
public static partial class Matrix4x4Polyfills {
  extension(Matrix4x4) {
    /// <summary>
    /// Creates a left-handed view matrix.
    /// </summary>
    /// <param name="cameraPosition">The position of the camera.</param>
    /// <param name="cameraTarget">The target towards which the camera is pointing.</param>
    /// <param name="cameraUpVector">The direction that is "up" from the camera's point of view.</param>
    /// <returns>The left-handed view matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 CreateLookAtLeftHanded(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
      => CreateLookToLeftHanded(cameraPosition, cameraTarget - cameraPosition, cameraUpVector);

    /// <summary>
    /// Creates a right-handed view matrix from a camera position and direction.
    /// </summary>
    /// <param name="cameraPosition">The position of the camera.</param>
    /// <param name="cameraDirection">The direction the camera is pointing.</param>
    /// <param name="cameraUpVector">The direction that is "up" from the camera's point of view.</param>
    /// <returns>The right-handed view matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 CreateLookTo(Vector3 cameraPosition, Vector3 cameraDirection, Vector3 cameraUpVector)
      => Matrix4x4.CreateLookAt(cameraPosition, cameraPosition + cameraDirection, cameraUpVector);

    /// <summary>
    /// Creates a left-handed view matrix from a camera position and direction.
    /// </summary>
    /// <param name="cameraPosition">The position of the camera.</param>
    /// <param name="cameraDirection">The direction the camera is pointing.</param>
    /// <param name="cameraUpVector">The direction that is "up" from the camera's point of view.</param>
    /// <returns>The left-handed view matrix.</returns>
    public static Matrix4x4 CreateLookToLeftHanded(Vector3 cameraPosition, Vector3 cameraDirection, Vector3 cameraUpVector) {
      var axisZ = Vector3.Normalize(cameraDirection);
      var axisX = Vector3.Normalize(Vector3.Cross(cameraUpVector, axisZ));
      var axisY = Vector3.Cross(axisZ, axisX);

      return new(
        axisX.X,
        axisY.X,
        axisZ.X,
        0,
        axisX.Y,
        axisY.Y,
        axisZ.Y,
        0,
        axisX.Z,
        axisY.Z,
        axisZ.Z,
        0,
        -Vector3.Dot(axisX, cameraPosition),
        -Vector3.Dot(axisY, cameraPosition),
        -Vector3.Dot(axisZ, cameraPosition),
        1
      );
    }

    /// <summary>
    /// Creates a left-handed orthographic projection matrix.
    /// </summary>
    /// <param name="width">The width of the view volume.</param>
    /// <param name="height">The height of the view volume.</param>
    /// <param name="zNearPlane">The minimum Z-value of the view volume.</param>
    /// <param name="zFarPlane">The maximum Z-value of the view volume.</param>
    /// <returns>The left-handed orthographic projection matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 CreateOrthographicLeftHanded(float width, float height, float zNearPlane, float zFarPlane) {
      var range = 1.0f / (zFarPlane - zNearPlane);
      return new(
        2.0f / width,
        0,
        0,
        0,
        0,
        2.0f / height,
        0,
        0,
        0,
        0,
        range,
        0,
        0,
        0,
        -range * zNearPlane,
        1
      );
    }

    /// <summary>
    /// Creates a left-handed customized orthographic projection matrix.
    /// </summary>
    /// <param name="left">The minimum X-value of the view volume.</param>
    /// <param name="right">The maximum X-value of the view volume.</param>
    /// <param name="bottom">The minimum Y-value of the view volume.</param>
    /// <param name="top">The maximum Y-value of the view volume.</param>
    /// <param name="zNearPlane">The minimum Z-value of the view volume.</param>
    /// <param name="zFarPlane">The maximum Z-value of the view volume.</param>
    /// <returns>The left-handed orthographic projection matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 CreateOrthographicOffCenterLeftHanded(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane) {
      var rangeX = 1.0f / (right - left);
      var rangeY = 1.0f / (top - bottom);
      var rangeZ = 1.0f / (zFarPlane - zNearPlane);
      return new(
        2.0f * rangeX,
        0,
        0,
        0,
        0,
        2.0f * rangeY,
        0,
        0,
        0,
        0,
        rangeZ,
        0,
        -(left + right) * rangeX,
        -(top + bottom) * rangeY,
        -rangeZ * zNearPlane,
        1
      );
    }

    /// <summary>
    /// Creates a left-handed perspective projection matrix based on a field of view.
    /// </summary>
    /// <param name="fieldOfView">The field of view in the y direction, in radians.</param>
    /// <param name="aspectRatio">The aspect ratio (width / height).</param>
    /// <param name="nearPlaneDistance">The distance to the near view plane.</param>
    /// <param name="farPlaneDistance">The distance to the far view plane.</param>
    /// <returns>The left-handed perspective projection matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 CreatePerspectiveFieldOfViewLeftHanded(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance) {
      ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(fieldOfView, 0);
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(fieldOfView, MathF.PI);
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nearPlaneDistance);
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(farPlaneDistance);
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(nearPlaneDistance, farPlaneDistance);

      var yScale = 1.0f / MathF.Tan(fieldOfView * 0.5f);
      var xScale = yScale / aspectRatio;
      var range = farPlaneDistance / (farPlaneDistance - nearPlaneDistance);

      return new(
        xScale,
        0,
        0,
        0,
        0,
        yScale,
        0,
        0,
        0,
        0,
        range,
        1,
        0,
        0,
        -range * nearPlaneDistance,
        0
      );
    }

    /// <summary>
    /// Creates a left-handed perspective projection matrix.
    /// </summary>
    /// <param name="width">The width of the view volume at the near view plane.</param>
    /// <param name="height">The height of the view volume at the near view plane.</param>
    /// <param name="nearPlaneDistance">The distance to the near view plane.</param>
    /// <param name="farPlaneDistance">The distance to the far view plane.</param>
    /// <returns>The left-handed perspective projection matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 CreatePerspectiveLeftHanded(float width, float height, float nearPlaneDistance, float farPlaneDistance) {
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nearPlaneDistance);
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(farPlaneDistance);
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(nearPlaneDistance, farPlaneDistance);

      var range = farPlaneDistance / (farPlaneDistance - nearPlaneDistance);

      return new(
        2.0f * nearPlaneDistance / width,
        0,
        0,
        0,
        0,
        2.0f * nearPlaneDistance / height,
        0,
        0,
        0,
        0,
        range,
        1,
        0,
        0,
        -range * nearPlaneDistance,
        0
      );
    }

    /// <summary>
    /// Creates a left-handed customized perspective projection matrix.
    /// </summary>
    /// <param name="left">The minimum X-value of the view volume at the near view plane.</param>
    /// <param name="right">The maximum X-value of the view volume at the near view plane.</param>
    /// <param name="bottom">The minimum Y-value of the view volume at the near view plane.</param>
    /// <param name="top">The maximum Y-value of the view volume at the near view plane.</param>
    /// <param name="nearPlaneDistance">The distance to the near view plane.</param>
    /// <param name="farPlaneDistance">The distance to the far view plane.</param>
    /// <returns>The left-handed perspective projection matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 CreatePerspectiveOffCenterLeftHanded(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance) {
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nearPlaneDistance);
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(farPlaneDistance);
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(nearPlaneDistance, farPlaneDistance);

      var rangeZ = farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
      var twoNear = 2.0f * nearPlaneDistance;

      return new(
        twoNear / (right - left),
        0,
        0,
        0,
        0,
        twoNear / (top - bottom),
        0,
        0,
        (left + right) / (right - left),
        (top + bottom) / (top - bottom),
        rangeZ,
        1,
        0,
        0,
        -rangeZ * nearPlaneDistance,
        0
      );
    }

    /// <summary>
    /// Creates a right-handed viewport transformation matrix.
    /// </summary>
    /// <param name="x">The X position of the viewport.</param>
    /// <param name="y">The Y position of the viewport.</param>
    /// <param name="width">The width of the viewport.</param>
    /// <param name="height">The height of the viewport.</param>
    /// <param name="minDepth">The minimum depth of the viewport.</param>
    /// <param name="maxDepth">The maximum depth of the viewport.</param>
    /// <returns>The viewport transformation matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 CreateViewport(float x, float y, float width, float height, float minDepth, float maxDepth) {
      var halfWidth = width * 0.5f;
      var halfHeight = height * 0.5f;
      var depthRange = maxDepth - minDepth;

      return new(
        halfWidth,
        0,
        0,
        0,
        0,
        -halfHeight,
        0,
        0,
        0,
        0,
        depthRange,
        0,
        x + halfWidth,
        y + halfHeight,
        minDepth,
        1
      );
    }

    /// <summary>
    /// Creates a left-handed viewport transformation matrix.
    /// </summary>
    /// <param name="x">The X position of the viewport.</param>
    /// <param name="y">The Y position of the viewport.</param>
    /// <param name="width">The width of the viewport.</param>
    /// <param name="height">The height of the viewport.</param>
    /// <param name="minDepth">The minimum depth of the viewport.</param>
    /// <param name="maxDepth">The maximum depth of the viewport.</param>
    /// <returns>The left-handed viewport transformation matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 CreateViewportLeftHanded(float x, float y, float width, float height, float minDepth, float maxDepth) {
      var halfWidth = width * 0.5f;
      var halfHeight = height * 0.5f;
      var depthRange = maxDepth - minDepth;

      return new(
        halfWidth,
        0,
        0,
        0,
        0,
        -halfHeight,
        0,
        0,
        0,
        0,
        depthRange,
        0,
        x + halfWidth,
        y + halfHeight,
        minDepth,
        1
      );
    }

  }
}

#endif
