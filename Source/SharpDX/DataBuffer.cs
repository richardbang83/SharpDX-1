﻿// Copyright (c) 2010-2011 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Runtime.InteropServices;
using SharpDX.Direct3D;

namespace SharpDX
{
    /// <summary>
    /// Provides methods to perform fast read/write random access data on a buffer located in an unmanaged memory.
    /// </summary>
    /// <remarks>
    /// This class doesn't validate the position read/write from. It is the responsability of the client of this class
    /// to verify that access is done within the size of the buffer.
    /// </remarks>
    /// <unmanaged>None</unmanaged>
    public class DataBuffer : IDisposable
    {
        private unsafe sbyte* _buffer;
        private GCHandle _gCHandle;
        private Blob _blob;
        private readonly bool _ownsBuffer;
        private readonly long _size;

        /// <summary>
        ///   Release any resource associated to this DataBuffer
        /// </summary>
        public void Release()
        {
            unsafe
            {
                if (_blob != null)
                {
                    _blob.Release();
                    _blob = null;
                }
                if (_gCHandle.IsAllocated)
                    _gCHandle.Free();
                if (_ownsBuffer && _buffer != (sbyte*)0)
                {
                    Marshal.FreeHGlobal((IntPtr)_buffer);
                    _buffer = (sbyte*)0;
                }
            }
        }

        internal unsafe DataBuffer(Blob buffer)
        {
            System.Diagnostics.Debug.Assert(buffer.GetBufferSize() > 0);

            _buffer = (sbyte*)buffer.GetBufferPointer();
            _size = buffer.GetBufferSize();
            _blob = buffer;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "SharpDX.DataBuffer" /> class, using a managed buffer as a backing store.
        /// </summary>
        /// <param name = "userBuffer">A managed array to be used as a backing store.</param>
        public DataBuffer(Array userBuffer)
        {
            unsafe
            {
                int num;
                if (userBuffer == null)
                {
                    throw new ArgumentNullException("userBuffer");
                }
                GCHandle handle = GCHandle.Alloc(userBuffer, GCHandleType.Pinned);

                _gCHandle = handle;
                IntPtr ptr = _gCHandle.AddrOfPinnedObject();
                _buffer = (sbyte*)ptr.ToPointer();
                if (userBuffer.Length == 0)
                {
                    num = 0;
                }
                else
                {
                    num = Marshal.SizeOf(userBuffer.GetValue(0)) * userBuffer.Length;
                }
                _size = num;
            }
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "SharpDX.DataBuffer" /> class, and allocates a new buffer to use as a backing store.
        /// </summary>
        /// <param name = "sizeInBytes">The size of the buffer to be allocated, in bytes.</param>
        /// <exception cref = "T:System.ArgumentOutOfRangeException">
        ///   <paramref name = "sizeInBytes" /> is less than 1.</exception>
        public DataBuffer(int sizeInBytes)
        {
            unsafe
            {
                System.Diagnostics.Debug.Assert(sizeInBytes > 0);

                _buffer = (sbyte*)Marshal.AllocHGlobal(sizeInBytes);
                _size = sizeInBytes;
                _ownsBuffer = true;
            }
        }

        internal unsafe DataBuffer(void* buffer, int sizeInBytes, bool makeCopy)
        {
            System.Diagnostics.Debug.Assert(sizeInBytes > 0);

            if (makeCopy)
            {
                _buffer = (sbyte*)Marshal.AllocHGlobal(sizeInBytes);
                Utilities.CopyMemory((IntPtr)_buffer, (IntPtr)buffer, sizeInBytes);
            }
            else
            {
                _buffer = (sbyte*)buffer;
            }
            _size = sizeInBytes;
            _ownsBuffer = makeCopy;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "SharpDX.DataBuffer" /> class, using an unmanaged buffer as a backing store.
        /// </summary>
        /// <param name = "userBuffer">A pointer to the buffer to be used as a backing store.</param>
        /// <param name = "sizeInBytes">The size of the buffer provided, in bytes.</param>
        public DataBuffer(IntPtr userBuffer, long sizeInBytes)
        {
            unsafe
            {
                System.Diagnostics.Debug.Assert(userBuffer != IntPtr.Zero);
                System.Diagnostics.Debug.Assert(sizeInBytes > 0);
                _buffer = (sbyte*)userBuffer.ToPointer();
                _size = sizeInBytes;
            }
        }

        /// <summary>
        ///   Gets a single value from the current buffer at the specified position.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <typeparam name = "T">The type of the value to be read from the buffer.</typeparam>
        /// <returns>The value that was read.</returns>
        public T Get<T>(int positionInBytes) where T : struct
        {
            unsafe
            {
                T result = default(T);
                Utilities.Read((IntPtr)(_buffer + positionInBytes), ref result);
                return result;
            }
        }

        /// <summary>
        /// Gets a float.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>a float from the buffer</returns>
        public float GetFloat(int positionInBytes)
        {
            unsafe
            {
                return *((float*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a int.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an int from the buffer</returns>
        public int GetInt(int positionInBytes)
        {
            unsafe
            {
                return *((int*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a short.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an short from the buffer</returns>
        public short GetShort(int positionInBytes)
        {
            unsafe
            {
                return *((short*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a bool.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an bool from the buffer</returns>
        public bool GetBoolean(int positionInBytes)
        {
            unsafe
            {
                return *((int*) (_buffer + positionInBytes)) != 0;
            }
        }

        /// <summary>
        /// Gets a Vector2.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an Vector2 from the buffer</returns>
        public Vector2 GetVector2(int positionInBytes)
        {
            unsafe
            {
                return *((Vector2*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a Vector3.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an Vector3 from the buffer</returns>
        public Vector3 GetVector3(int positionInBytes)
        {
            unsafe
            {
                return *((Vector3*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a Vector4.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an Vector4 from the buffer</returns>
        public Vector4 GetVector4(int positionInBytes)
        {
            unsafe
            {
                return *((Vector4*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a Color3.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an Color3 from the buffer</returns>
        public Color3 GetColor3(int positionInBytes)
        {
            unsafe
            {
                return *((Color3*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a Color4.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an Color4 from the buffer</returns>
        public Color4 GetColor4(int positionInBytes)
        {
            unsafe
            {
                return *((Color4*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a Half.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an Half from the buffer</returns>
        public Half GetHalf(int positionInBytes)
        {
            unsafe
            {
                return *((Half*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a Half2.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an Half2 from the buffer</returns>
        public Half2 GetHalf2(int positionInBytes)
        {
            unsafe
            {
                return *((Half2*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a Half3.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an Half3 from the buffer</returns>
        public Half3 GetHalf3(int positionInBytes)
        {
            unsafe
            {
                return *((Half3*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a Half4.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>an Half4 from the buffer</returns>
        public Half4 GetHalf4(int positionInBytes)
        {
            unsafe
            {
                return *((Half4*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a Matrix.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>a Matrix from the buffer</returns>
        public Matrix GetMatrix(int positionInBytes)
        {
            unsafe
            {
                return *((Matrix*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        /// Gets a Quaternion.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <returns>a Quaternion from the buffer</returns>
        public Quaternion GetQuaternion(int positionInBytes)
        {
            unsafe
            {
                return *((Quaternion*)(_buffer + positionInBytes));
            }
        }

        /// <summary>
        ///   Gets an array of values from a position in the buffer.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <param name="count">number of T instance to get from the positionInBytes</param>
        /// <typeparam name = "T">The type of the values to be read from the buffer.</typeparam>
        /// <returns>An array of values that was read from the current buffer.</returns>
        public T[] GetRange<T>(int positionInBytes, int count) where T : struct
        {
            unsafe
            {
                var result = new T[count];
                Utilities.Read((IntPtr)(_buffer + positionInBytes), result, 0, count);
                return result;
            }
        }

        /// <summary>
        ///   Gets a sequence of elements from a position in the buffer into a target buffer.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to get the data from.</param>
        /// <param name = "buffer">An array of values to be read from the buffer.</param>
        /// <param name = "offset">The zero-based byte offset in buffer at which to begin storing
        ///   the data read from the current buffer.</param>
        /// <param name = "count">The number of values to be read from the current buffer.</param>
        public void GetRange<T>(int positionInBytes, T[] buffer, int offset, int count) where T : struct
        {
            unsafe
            {
                Utilities.Read((IntPtr) (_buffer + positionInBytes), buffer, offset, count);
            }
        }


        /// <summary>
        ///   Sets a single value to the buffer at a specified position.
        /// </summary>
        /// <typeparam name = "T">The type of the value to be written to the buffer.</typeparam>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name = "value">The value to write to the buffer.</param>
        public void Set<T>(int positionInBytes, T value) where T : struct
        {
            unsafe
            {
                Utilities.Write((IntPtr)(_buffer + positionInBytes), ref value);
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, float value)
        {
            unsafe
            {
                *((float*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, int value)
        {
            unsafe
            {
                *((int*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, short value)
        {
            unsafe
            {
                *((short*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, bool value)
        {
            unsafe
            {
                *((int*)(_buffer + positionInBytes)) = value?1:0;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, Vector2 value)
        {
            unsafe
            {
                *((Vector2*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, Vector3 value)
        {
            unsafe
            {
                *((Vector3*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, Vector4 value)
        {
            unsafe
            {
                *((Vector4*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, Color3 value)
        {
            unsafe
            {
                *((Color3*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, Color4 value)
        {
            unsafe
            {
                *((Color4*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, Half value)
        {
            unsafe
            {
                *((Half*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, Half2 value)
        {
            unsafe
            {
                *((Half2*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, Half3 value)
        {
            unsafe
            {
                *((Half3*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, Half4 value)
        {
            unsafe
            {
                *((Half4*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, Matrix value)
        {
            unsafe
            {
                *((Matrix*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name="value">The value.</param>
        public void Set(int positionInBytes, Quaternion value)
        {
            unsafe
            {
                *((Quaternion*)(_buffer + positionInBytes)) = value;
            }
        }

        /// <summary>
        ///   Sets an array of values to a specified position into the buffer.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name = "data">An array of values to be written to the current buffer.</param>
        public void Set<T>(int positionInBytes, T[] data) where T : struct
        {
            Set(positionInBytes, data, 0, data.Length);
        }

        /// <summary>
        ///   Sets a range of data to a specified position into the buffer.
        /// </summary>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name = "source">A pointer to the location to start copying from.</param>
        /// <param name = "count">The number of bytes to copy from source to the current buffer.</param>
        public void Set(int positionInBytes, IntPtr source, long count)
        {
            unsafe
            {
                Utilities.CopyMemory((IntPtr)(_buffer + positionInBytes), source, (ulong)count);
            }
        }

        /// <summary>
        ///   Sets an array of values to a specified position into the buffer.
        /// </summary>
        /// <typeparam name = "T">The type of the values to be written to the buffer.</typeparam>
        /// <param name="positionInBytes">Relative position in bytes from the beginning of the buffer to set the data to.</param>
        /// <param name = "data">An array of values to be written to the buffer.</param>
        /// <param name = "offset">The zero-based offset in data at which to begin copying values to the current buffer.</param>
        /// <param name = "count">The number of values to be written to the current buffer. If this is zero,
        ///   all of the contents <paramref name = "data" /> will be written.</param>
        public void Set<T>(int positionInBytes, T[] data, int offset, int count) where T : struct
        {
            unsafe
            {
                Utilities.Write((IntPtr)(_buffer + positionInBytes), data, offset, count);
            }
        }


        /// <summary>
        ///   Gets a pointer to the buffer used as a backing store..
        /// </summary>
        /// <value>An IntPtr to the buffer being used as a backing store.</value>
        public IntPtr DataPointer
        {
            get
            {
                unsafe
                {
                    return new IntPtr(_buffer);
                }
            }
        }

        /// <summary>
        ///   Gets the length in bytes of the buffer.
        /// </summary>
        /// <value>A long value representing the length of the buffer in bytes.</value>
        public long Size
        {
            get { return _size; }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Release();
        }

        #endregion
    }
}