// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
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

using System.Collections.Generic;
using SharpDX.Serialization;

namespace SharpDX.Toolkit.Graphics
{
    public partial class EffectData 
    {
        /// <summary>
        /// Describes a pass from a technique.
        /// </summary>
        public sealed class Pass : IDataSerializable
        {
            /// <summary>
            /// Name of this pass.
            /// </summary>
            public string Name;

            /// <summary>
            /// True if this pass is the sub-pass of a root pass.
            /// </summary>
            public bool IsSubPass;

            /// <summary>
            /// List of <see cref="Attribute"/>.
            /// </summary>
            public List<Attribute> Attributes;

            /// <summary>
            /// Description of the shader stage <see cref="Pipeline"/>.
            /// </summary>
            public Pipeline Pipeline;

            public override string ToString()
            {
                return string.Format("Pass: [{0}], SubPass: {1}, Attributes({2})", Name, IsSubPass, Attributes.Count);
            }

            /// <inheritdoc/>
            void IDataSerializable.Serialize(BinarySerializer serializer)
            {
                serializer.AllowNull = true;
                serializer.Serialize(ref Name);
                serializer.AllowNull = false;

                serializer.Serialize(ref IsSubPass);
                serializer.Serialize(ref Attributes);
                serializer.Serialize(ref Pipeline);
            }
        }
    }
}